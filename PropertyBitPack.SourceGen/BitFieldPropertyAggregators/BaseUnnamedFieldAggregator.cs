using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// Aggregates bit-field properties that do not have an explicit field name specified 
/// (i.e., <c>fieldName == null</c> or only whitespace). Distributes them into unnamed fields,
/// validating bit sizes and generating appropriate <see cref="GenerateSourceRequest"/> objects.
/// </summary>
internal abstract class BaseUnnamedFieldAggregator: BaseBitFieldPropertyAggregator
{

    protected virtual ImmutableArray<BaseBitFieldPropertyInfo> SelectCandiadates(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // We'll gather properties that do not define a valid field name into a temporary builder
        using var unnamedFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        // Iterate over all properties
        foreach (var property in properties)
        {
            var fieldName = property.AttributeParsedResult.FieldName;

            // If field name is null or whitespace, treat it as an unnamed field
            if (fieldName == null || string.IsNullOrWhiteSpace(fieldName.Name))
            {
                // Validate that the bit size is permissible (non-zero, etc.)
                if (!ValidateSize(property))
                {
                    // If invalid, we create a diagnostic about "TooManyBitsForAnyType"
                    var size = BitCountHelper.GetEffectiveBitsCount(property);

                    var diagnostic = Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        property.AttributeParsedResult.BitsCountArgument()?.GetLocation(),
                        "<unnamed>",
                        size
                    );

                    diagnostics.Add(diagnostic);

                    // Continue to the next property, skipping addition to the aggregator
                    continue;
                }

                // If it's valid, add it to the unnamed property list
                unnamedFieldPropertiesBuilder.Add(property);
            }
        }

        // Convert the collected unnamed properties to an immutable array
        var unnamedFieldProperties = unnamedFieldPropertiesBuilder.ToImmutable();

        return unnamedFieldProperties;
    }

    /// <summary>
    /// Core logic for aggregating unnamed field properties. 
    /// Checks each property for a valid bit size, groups properties, calculates bits 
    /// distribution, and produces <see cref="GenerateSourceRequest"/> entries.
    /// </summary>
    /// <param name="properties">
    /// A linked list of <see cref="BaseBitFieldPropertyInfo"/> that may or may not have field names.
    /// </param>
    /// <param name="requestsBuilder">
    /// An <see cref="ImmutableArrayBuilder{T}"/> used to collect <see cref="GenerateSourceRequest"/> instances.
    /// </param>
    /// <param name="diagnostics">
    /// An <see cref="ImmutableArrayBuilder{T}"/> for adding any <see cref="Diagnostic"/> objects related to errors.
    /// </param>
    protected override void AggregateCore(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        var unnamedFieldProperties = SelectCandiadates(properties, in diagnostics);

        // If there are no unnamed properties, there's nothing to do here
        if (unnamedFieldProperties.Length == 0)
        {
            return;
        }

        // Group the unnamed properties by potential "field name" and owner type
        var groupedFieldProperties = GroupPropertiesByFieldNameAndOwner(unnamedFieldProperties);

        // Process each group
        for (var i = 0; i < groupedFieldProperties.Length; i++)
        {
            var group = groupedFieldProperties[i];
            var owner = group.Owner;

            // Distribute bit sizes among the fields
            var calculatedBitCount = DistributeBitsIntoFields(group.Properties);

            try
            {
                // Add the GenerateSourceRequests for this group
                AddUnnamedFieldRequests(group, calculatedBitCount, in requestsBuilder);

                // After successfully generating requests, remove these properties from the list
                for (var j = 0; j < group.Properties.Length; j++)
                {
                    var property = group.Properties[j];
                    properties.Remove(property);
                }
            }
            catch (Exception exc)
            {
                // In case of unexpected errors, break into the debugger (in debug builds).
                Debug.WriteLine(exc);
                Debugger.Break();
                continue;
            }
        }
    }

    /// <summary>
    /// Creates and adds <see cref="GenerateSourceRequest"/> instances for the properties in the group,
    /// based on their calculated bit distributions.
    /// </summary>
    /// <param name="ownerFieldNameGroup">A group of unnamed field properties associated with a specific owner.</param>
    /// <param name="calculatedBits">An array of <see cref="CalculatedBits"/> that indicates how the bits are allocated.</param>
    /// <param name="requestsBuilder">
    /// A builder used to collect <see cref="GenerateSourceRequest"/> objects.
    /// </param>
    protected virtual void AddUnnamedFieldRequests(
        OwnerFieldNameGroup ownerFieldNameGroup,
        ImmutableArray<CalculatedBits> calculatedBits,
        in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder)
    {
        // We start by copying the properties into a list we can mutate
        using var list = ListsPool.Rent<BaseBitFieldPropertyInfo>();
        list.AddRange(ownerFieldNameGroup.Properties);

        // Process each "calculated bit" configuration
        for (var i = 0; i < calculatedBits.Length; i++)
        {
            var calculatedBit = calculatedBits[i];

            // We'll gather each resulting BitFieldPropertyInfoRequest into a local list
            using var calculatedBitList = ListsPool.Rent<BitFieldPropertyInfoRequest>();

            // We'll also gather candidate properties for the current arrangement
            using var candidateProperties = ListsPool.Rent<BaseBitFieldPropertyInfo>();

            // For each of the bit sizes in the current CalculatedBits, find the matching property
            for (var j = 0; j < calculatedBit.Bits.Length; j++)
            {
                var bitsSize = calculatedBit.Bits[j];

                // Match the property with the same bit size
                var candidateProperty = list.FirstOrDefault(
                    x => BitCountHelper.GetEffectiveBitsCount(x) == bitsSize
                );

                if (candidateProperty is null)
                {
                    // If not found, it's an internal error or mismatch
                    ThrowHelper.ThrowUnreachableException("Property not found.");
                }

                // Remove the matched property from the main list so it isn't reused
                list.Remove(candidateProperty);

                // Keep track of it locally
                candidateProperties.Add(candidateProperty);
            }

            // Create a field name for the unnamed field
            var fieldName = GetFieldName(in candidateProperties);
            // Build a FieldRequest object (non-existing field for unnamed usage)
            var fieldRequest = new NonExistingFieldRequest(
                fieldName,
                MapBitSizeToSpecialType(calculatedBit.FieldCapacity)
            );

            // We'll compute offsets as we move through each property
            byte offset = 0;

            // For each property in candidateProperties, assign a BitsSpan and create the request
            for (var j = 0; j < candidateProperties.Count; j++)
            {
                var candidateProperty = candidateProperties[j];
                var bitsSize = calculatedBit.Bits[j];

                var bitsSpan = new BitsSpan(fieldRequest, offset, bitsSize);
                var bitFieldPropertyInfoRequest = new BitFieldPropertyInfoRequest(bitsSpan, candidateProperty);

                calculatedBitList.Add(bitFieldPropertyInfoRequest);
                offset += bitsSize;
            }

            // Finally, create a request object (UnnamedFieldGsr) and add it to the requestsBuilder
            var gsr = new UnnamedFieldGsr(fieldRequest, [.. calculatedBitList]);
            requestsBuilder.Add(gsr);
        }
    }

    protected virtual UnnamedFieldGsr CreateGsr(
        NonExistingFieldRequest fieldRequest,
        ImmutableArray<BitFieldPropertyInfoRequest> properties)
    {
        return new UnnamedFieldGsr(fieldRequest, properties);
    }

    /// <summary>
    /// Builds a synthetic field name for unnamed properties by concatenating 
    /// the property symbols' names with underscores.
    /// </summary>
    /// <param name="baseBitFieldPropertyInfos">The list of candidate properties that share the unnamed field.</param>
    /// <returns>A string that represents the generated field name (e.g. "_Prop1__Prop2__").</returns>
    protected virtual string GetFieldName(
        ref readonly ListsPool.RentedListPool<BaseBitFieldPropertyInfo> baseBitFieldPropertyInfos)
    {
        using var rentedStringBuilder = StringBuildersPool.Rent();
        var stringBuilder = rentedStringBuilder.StringBuilder;
        stringBuilder.Append("_");

        // Append each property name and double underscores
        for (var i = 0; i < baseBitFieldPropertyInfos.Count; i++)
        {
            var property = baseBitFieldPropertyInfos[i];
            stringBuilder.Append(property.PropertySymbol.Name);
            stringBuilder.Append("__");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Validates the bit size of a property to ensure it is acceptable (e.g., &gt; 0).
    /// </summary>
    /// <param name="baseBitFieldPropertyInfo">Property to check.</param>
    /// <returns><c>true</c> if the bit size is valid; otherwise <c>false</c>.</returns>
    protected static bool ValidateSize(BaseBitFieldPropertyInfo baseBitFieldPropertyInfo)
    {
        var bits = BitCountHelper.GetEffectiveBitsCount(baseBitFieldPropertyInfo);
        return bits > 0;
    }
}
