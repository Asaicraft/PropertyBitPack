using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// An aggregator that groups bit field properties by the field name
/// and creates one or more <see cref="GenerateSourceRequest"/> instances.
/// </summary>
internal sealed class AggregateByNameBitFieldAggregator : BaseBitFieldPropertyAggregator
{
    public override ImmutableArray<GenerateSourceRequest> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Use a builder to collect the final set of GenerateSourceRequest instances.
        using var generateSourceRequests = ImmutableArrayBuilder<GenerateSourceRequest>.Rent();

        // Step 1: Group properties by their IFieldName.Name.
        // For those without a valid name, we keep them in a separate list.
        var groupedProperties = new Dictionary<string, List<BaseBitFieldPropertyInfo>>();
        var unnamedProperties = new List<BaseBitFieldPropertyInfo>();

        foreach (var property in properties)
        {
            var fieldName = property.AttributeParsedResult.FieldName?.Name;

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                unnamedProperties.Add(property);
            }
            else
            {
                if (!groupedProperties.TryGetValue(fieldName, out var list))
                {
                    list = [];
                    groupedProperties[fieldName] = list;
                }
                list.Add(property);
            }
        }

        // Step 2: Process each group that has a named field.
        foreach (var fieldNameProperties in groupedProperties)
        {
            var currentFieldName = fieldNameProperties.Key;
            var props = fieldNameProperties.Value;

            using var fieldRequestsBuilder = ImmutableArrayBuilder<FieldRequest>.Rent();
            using var propertyRequestsBuilder = ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent();

            foreach (var prop in props)
            {
                var bitsCount = prop.AttributeParsedResult.BitsCount ?? 1;
                // For a real scenario, you might compute an offset, etc.
                var bitsSpan = new BitsSpan(new FieldRequest(currentFieldName, SpecialType.System_UInt32), 0, (byte)bitsCount);
                propertyRequestsBuilder.Add(new BitFieldPropertyInfoRequest(bitsSpan, prop));
            }

            var propertyRequests = propertyRequestsBuilder.ToImmutable();
            // We assume a single field for this group (adjust as needed).
            fieldRequestsBuilder.Add(new FieldRequest(currentFieldName, SpecialType.System_UInt32));
            var fields = fieldRequestsBuilder.ToImmutable();

            // Construct and add our request.
            generateSourceRequests.Add(new SimpleGenerateSourceRequest(fields, propertyRequests));

            // Remove these properties from the main list to avoid re-processing.
            foreach (var prop in props)
            {
                properties.Remove(prop);
            }
        }

        // Step 3: Handle unnamed properties by generating a combined name.
        if (unnamedProperties.Count > 0)
        {
            using var fieldRequestsBuilder = ImmutableArrayBuilder<FieldRequest>.Rent();
            using var propertyRequestsBuilder = ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent();

            // Create a combined field name from property symbols.
            var combinedFieldName = string.Join("__", unnamedProperties.Select(p => p.PropertySymbol.Name));

            // Optionally, if combinedFieldName is empty (edge case), define a default.
            if (string.IsNullOrWhiteSpace(combinedFieldName))
            {
                combinedFieldName = "AutoField";
            }

            // For each property, we create a BitsSpan referencing the same field.
            foreach (var prop in unnamedProperties)
            {
                var bitsCount = prop.AttributeParsedResult.BitsCount ?? 1;
                var bitsSpan = new BitsSpan(
                    new FieldRequest(combinedFieldName, SpecialType.System_UInt32),
                    0,
                    (byte)bitsCount);

                propertyRequestsBuilder.Add(new BitFieldPropertyInfoRequest(bitsSpan, prop));
            }

            // Only one field request for all unnamed properties in this example.
            fieldRequestsBuilder.Add(new FieldRequest(combinedFieldName, SpecialType.System_UInt32));

            var unnamedProps = propertyRequestsBuilder.ToImmutable();
            var unnamedFields = fieldRequestsBuilder.ToImmutable();

            generateSourceRequests.Add(new SimpleGenerateSourceRequest(unnamedFields, unnamedProps));

            // Remove them from the main list.
            foreach (var prop in unnamedProperties)
            {
                properties.Remove(prop);
            }
        }

        // Return the final array of requests.
        return generateSourceRequests.ToImmutable();
    }
}
