using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// Aggregates read-only properties annotated with <see cref="ParsedReadOnlyBitFieldAttribute"/>.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="BaseBitFieldPropertyAggregator"/> and provides functionality 
/// to filter and aggregate properties marked as read-only for further processing.
/// </remarks>
internal sealed class ReadOnlyAggregator: BaseBitFieldPropertyAggregator
{

    /// <summary>
    /// Aggregates properties with the <see cref="ParsedReadOnlyBitFieldAttribute"/> into an immutable array.
    /// </summary>
    /// <param name="properties">
    /// A linked list of <see cref="BaseBitFieldPropertyInfo"/> representing the properties to be aggregated.
    /// </param>
    /// <param name="requestsBuilder">
    /// An <see cref="ImmutableArrayBuilder{T}"/> used to accumulate generation requests.
    /// </param>
    /// <param name="diagnostics">
    /// An <see cref="ImmutableArrayBuilder{T}"/> used to collect diagnostics during the aggregation process.
    /// </param>
    /// <remarks>
    /// This method filters properties that have a parsed result of type <see cref="ParsedReadOnlyBitFieldAttribute"/>
    /// and collects them into a temporary builder, which is then converted into an immutable array for further use.
    /// </remarks>
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // We'll gather properties that do not define a valid field name into a temporary builder
        using var readonlyFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        foreach (var property in properties)
        {
            if (property.AttributeParsedResult is IParsedReadOnlyBitFieldAttribute)
            {
                readonlyFieldPropertiesBuilder.Add(property);
            }
        }

        var readonlyFieldProperties = readonlyFieldPropertiesBuilder.ToImmutable();

        if(readonlyFieldProperties.IsDefaultOrEmpty)
        {
            return;
        }

        var grouped = GroupPropertiesByFieldNameAndOwner(readonlyFieldProperties);


    }
}
