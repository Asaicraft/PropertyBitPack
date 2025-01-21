using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// An aggregator that organizes bit field properties with user-defined field names 
/// and packs them into the most efficient fields possible, 
/// while respecting bit size constraints (maximum 64 bits per field).
/// </summary>
internal sealed class NamedFieldPropertyAggregator : BaseBitFieldPropertyAggregator
{

    /// <summary>
    /// Aggregates properties into named fields, grouping by field name and owner,
    /// and ensuring each field's bit size does not exceed the maximum allowed capacity.
    /// </summary>
    /// <param name="properties">
    /// A collection of <see cref="BaseBitFieldPropertyInfo"/> to be processed.
    /// </param>
    /// <param name="requestsBuilder">
    /// The builder used to accumulate generated source requests.
    /// </param>
    /// <param name="diagnostics">
    /// The builder used to collect any diagnostic issues encountered during processing.
    /// </param>
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Collects properties that have named but non-existing fields.
        using var unnamedFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        foreach (var property in properties)
        {
            var fieldName = property.AttributeParsedResult.FieldName;

            // Skip properties that refer to existing fields.
            if (fieldName?.IsSymbolExist is not false)
            {
                continue;
            }

            // Skip properties with no valid field name.
            if (string.IsNullOrWhiteSpace(fieldName?.Name))
            {
                continue;
            }

            unnamedFieldPropertiesBuilder.Add(property);
        }

        var unnamedFieldProperties = unnamedFieldPropertiesBuilder.ToImmutable();

        // If no unnamed properties are found, exit early.
        if (unnamedFieldProperties.Length == 0)
        {
            return;
        }

        // Group properties by field name and owner.
        var groupedFieldProperties = GroupPropertiesByFieldNameAndOwner(unnamedFieldProperties);

        for (var i = 0; i < groupedFieldProperties.Length; i++)
        {
            var group = groupedFieldProperties[i];

            Debug.Assert(group.FieldName is not null);
            Debug.Assert(!string.IsNullOrWhiteSpace(group.FieldName?.Name));

            var owner = group.Owner;
            var groupProperties = group.Properties;

            // Calculate the total required bits for the group.
            var requiredBits = RequiredBits(groupProperties);
            var fieldName = group.FieldName!.Name!;

            // If the required bits exceed 64, report a diagnostic error.
            if (requiredBits > 64)
            {
                var location = groupProperties[0].AttributeParsedResult.BitsCountArgument()?.GetLocation();

                var diagnostic = Diagnostic.Create(
                    PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                    location,
                    fieldName,
                    requiredBits
                );

                diagnostics.Add(diagnostic);

                continue;
            }

            // Determine the minimal field size for the required bits.
            var bitSize = DetermineFieldBitSize((byte)requiredBits);

            // Create a GenerateSourceRequest for the group.
            var gsr = CreateGsr(group, bitSize);

            // Remove processed properties from the original list.
            for (var j = 0; j < groupProperties.Length; j++)
            {
                var property = groupProperties[j];
                properties.Remove(property);
            }

            // Add the generated source request to the builder.
            requestsBuilder.Add(gsr);
        }
    }

    /// <summary>
    /// Creates a <see cref="NamedFieldGsr"/> for a group of properties sharing the same owner and field name.
    /// </summary>
    /// <param name="ownerFieldNameGroup">
    /// The group of properties to process, containing the owner and field name.
    /// </param>
    /// <param name="bitSize">
    /// The calculated bit size for the field based on the group's properties.
    /// </param>
    /// <returns>
    /// A <see cref="NamedFieldGsr"/> instance containing the generated field request and property requests.
    /// </returns>
    private static NamedFieldGsr CreateGsr(OwnerFieldNameGroup ownerFieldNameGroup, BitSize bitSize)
    {
        var fieldName = ownerFieldNameGroup.FieldName!.Name!;

        var fieldRequest = new NamedFieldRequest(fieldName, MapBitSizeToSpecialType(bitSize));

        var requestProperties = ToRequests(fieldRequest, ownerFieldNameGroup.Properties);

        var gsr = new NamedFieldGsr(fieldRequest, requestProperties);

        return gsr;
    }
}