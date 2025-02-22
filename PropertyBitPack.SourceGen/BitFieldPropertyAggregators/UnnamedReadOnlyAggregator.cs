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

    public readonly ReadOnlyAggregatorComponent ReadOnlyAggregatorComponent = new();

    /// <inheritdoc/>
    protected override void SelectCandidatesCore(IReadOnlyCollection<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics, in ImmutableArrayBuilder<BaseBitFieldPropertyInfo> unnamedFieldPropertiesBuilder)
    {
        // Prepare a filtered list of properties that are parsed as read-only
        var filteredProperties = new List<BaseBitFieldPropertyInfo>();
        foreach (var property in properties)
        {
            if (property.AttributeParsedResult is ParsedReadOnlyBitFieldAttribute)
            {
                filteredProperties.Add(property);
            }
        }

        // Now pass only the filtered properties to the base logic,
        // which handles the unnamed-field checks, validation, and diagnostics.
        base.SelectCandidatesCore(filteredProperties, diagnostics, unnamedFieldPropertiesBuilder);
    }

    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<IGenerateSourceRequest> requestsBuilder, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // First we aggregate the unnamed properties
        base.AggregateCore(properties, requestsBuilder, diagnostics);

        // Then we aggregate the read-only properties
        var gsr = ReadOnlyAggregatorComponent.Aggregate(properties, requestsBuilder, diagnostics);

        // Add the generated source request to the builder
        for (var i = 0; i < gsr.Length; i++)
        {
            var request = gsr[i];
            requestsBuilder.Add(request);
        }

    }
}
