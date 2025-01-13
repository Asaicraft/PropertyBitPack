using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// An aggregator that packs bit field properties into as few fields as possible, 
/// respecting existing fields (if any) and user-defined or inferred bit sizes.
/// </summary>
internal sealed class AdvancedBitFieldPropertyAggregator : BaseBitFieldPropertyAggregator
{
    

    public override ImmutableArray<GenerateSourceRequest> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        

        if (properties.Count == 0)
        {
            return requestsBuilder.ToImmutable();
        }

        var namedFieldProperties = new List<BaseBitFieldPropertyInfo>();
        var unnamedFieldProperties = new List<BaseBitFieldPropertyInfo>();

        foreach (var property in properties)
        {
            if (!string.IsNullOrWhiteSpace(property.AttributeParsedResult.FieldName?.Name))
            {
                namedFieldProperties.Add(property);
            }
            else
            {
                unnamedFieldProperties.Add(property);
            }
        }

        var namedGroups = namedFieldProperties
            .GroupBy(
                keySelector: p => (p.Owner, Name: p.AttributeParsedResult.FieldName!.Name!),
                comparer: OwnerFieldNameComparer.Instance) // This is a custom IEqualityComparer, or you can do .ToLookup instead
            .ToList();

        foreach (var group in namedGroups)
        {
            var owner = group.Key.Owner;
            var fieldName = group.Key.Name;
            var propertiesInGroup = group.ToImmutableArray();

            var totalBits = 0;
            var hasInvalid = false;

            for (var i = 0; i < propertiesInGroup.Length; i++)
            {
                var property = propertiesInGroup[i];

                var bits = BitCountHelper.GetEffectiveBitsCount(property);

                if(bits < 0)
                {
                    var location = property.AttributeParsedResult.BitsCountArgument()?.GetLocation();

                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.InvalidBitsCount,
                        location,
                        property.PropertySymbol.Name));


                    hasInvalid = true;
                    break;
                }
                else
                {
                    totalBits += bits;
                }
            }

            if (hasInvalid)
            {
                // Skip the group if it has invalid bit counts
                continue;
            }


        }

        return requestsBuilder.ToImmutable();
    }

}