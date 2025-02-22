using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// Aggregates read-only properties annotated with <see cref="ParsedReadOnlyBitFieldAttribute"/>.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="BaseBitFieldPropertyAggregator"/> and provides functionality 
/// to filter and aggregate properties marked as read-only for further processing.
/// </remarks>
internal sealed class UnnamedReadOnlyAggregator : BaseUnnamedFieldAggregator
{

    /// <inheritdoc/>
    protected override void SelectCandidatesCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics, in ImmutableArrayBuilder<BaseBitFieldPropertyInfo> unnamedFieldPropertiesBuilder)
    {
        // Iterate over all properties
        foreach (var property in properties)
        {
            var fieldName = property.AttributeParsedResult.FieldName;

            if(property.AttributeParsedResult is not ParsedReadOnlyBitFieldAttribute)
            {
                // If the property is not marked as read-only, skip it
                continue;
            }

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
    }

    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // First we aggregate the unnamed properties
        base.AggregateCore(properties, requestsBuilder, diagnostics);

    }
}
