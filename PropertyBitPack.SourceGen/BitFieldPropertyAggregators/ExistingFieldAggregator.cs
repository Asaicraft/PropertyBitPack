using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// An aggregator that processes properties referencing an existing field (i.e., <see cref="IFieldName.IsSymbolExist"/> is true).
/// It groups them by their owner and existing field, validates bit sizes, and constructs source requests.
/// </summary>
internal sealed class ExistingFieldAggregator : BaseBitFieldPropertyAggregator
{
    /// <summary>
    /// Aggregates properties whose <see cref="IFieldName.IsSymbolExist"/> is true.
    /// </summary>
    /// <param name="properties">
    /// A collection of <see cref="BaseBitFieldPropertyInfo"/> objects to be inspected and grouped.
    /// </param>
    /// <param name="requestsBuilder">
    /// The builder used to add newly generated <see cref="GenerateSourceRequest"/> objects.
    /// </param>
    /// <param name="diagnostics">
    /// The builder used to record diagnostics if any invalid or unsupported scenario is detected.
    /// </param>
    protected override void AggregateCore(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Collect all properties that refer to an existing field.
        using var existingFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        foreach (var property in properties)
        {
            if (property.AttributeParsedResult.FieldName?.IsSymbolExist is true)
            {
                existingFieldPropertiesBuilder.Add(property);
            }
        }

        var existingFieldProperties = existingFieldPropertiesBuilder.ToImmutable();

        // If no properties reference an existing field, exit early.
        if (existingFieldProperties.Length == 0)
        {
            return;
        }

        // Group properties by (Owner, FieldName) without using LINQ.
        var groupedFieldProperties = GroupPropertiesByFieldNameAndOwner(existingFieldProperties);

        for (var i = 0; i < groupedFieldProperties.Length; i++)
        {
            var group = groupedFieldProperties[i];

            // Ensuring the grouped field name is not null and has a valid symbol.
            Debug.Assert(group.FieldName is not null);
            Debug.Assert(group.FieldName!.IsSymbolExist);

            var fieldSymbol = group.FieldName.ExistingSymbol!;
            var propertiesInGroup = group.Properties;

            // Determine the bit size from the field's special type.
            var bitsSize = MapSpecialTypeToBitSize(fieldSymbol.Type.SpecialType);

            // Validate if the total bits for this group fit into the existing field's size.
            if (!ValidateSize(bitsSize, propertiesInGroup))
            {
                // If not valid, report diagnostics for each property.
                var requiredBits = RequiredBits(propertiesInGroup);

                for (var j = 0; j < propertiesInGroup.Length; j++)
                {
                    var property = propertiesInGroup[j];
                    var fieldNameLocation = property.AttributeParsedResult.FieldNameArgument()?.GetLocation();

                    diagnostics.Add(Diagnostic.Create(
                       PropertyBitPackDiagnostics.TooManyBitsForSpecificType,
                       fieldNameLocation,
                       fieldSymbol.Name,
                       requiredBits,
                       (byte)bitsSize
                    ));
                }

                continue;
            }

            // Create a request object referring to the existing field.
            var fieldRequest = new ExistingFieldRequest(fieldSymbol);

            // Convert properties into their associated requests.
            var propertyRequests = ToRequests(fieldRequest, propertiesInGroup);

            // Build a GenerateSourceRequest (GSR) specifically for existing fields.
            var gsr = new ExistingFieldGsr(fieldSymbol, propertyRequests);

            // Store the result in the requests builder.
            requestsBuilder.Add(gsr);

            for (var j = 0; j < propertiesInGroup.Length; j++)
            {
                var usedProperty = propertiesInGroup[j];

                // Remove the property from the linked list.
                properties.Remove(usedProperty);
            }
        }
    }
}