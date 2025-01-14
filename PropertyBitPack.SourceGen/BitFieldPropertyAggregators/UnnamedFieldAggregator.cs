using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal sealed class UnnamedFieldAggregator : BaseBitFieldPropertyAggregator
{
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        using var unnamedFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        foreach (var property in properties)
        {
            var fieldName = property.AttributeParsedResult.FieldName;

            if (fieldName == null || string.IsNullOrWhiteSpace(fieldName.Name))
            {
                if(!ValidateSize(property))
                {
                    var size = BitCountHelper.GetEffectiveBitsCount(property);

                    var diagnostic = Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        property.AttributeParsedResult.BitsCountArgument()?.GetLocation(),
                        "<unnamed>",
                        size
                    );

                    diagnostics.Add(diagnostic);

                    continue;
                }

                unnamedFieldPropertiesBuilder.Add(property);
            }
        }

        var unnamedFieldProperties = unnamedFieldPropertiesBuilder.ToImmutable();

        if (unnamedFieldProperties.Length == 0)
        {
            return;
        }

        var groupedFieldProperties = GroupPropertiesByFieldNameAndOwner(unnamedFieldProperties);

        for (var i = 0; i < groupedFieldProperties.Length; i++)
        {
            var group = groupedFieldProperties[i];

            var owner = group.Owner;
            
            var calculatedBitCount = DistributeBitsIntoFields(group.Properties);
        }
    }

    private static bool ValidateSize(BaseBitFieldPropertyInfo baseBitFieldPropertyInfo)
    {
        var bits = BitCountHelper.GetEffectiveBitsCount(baseBitFieldPropertyInfo);

        return bits > 0;
    }
}
