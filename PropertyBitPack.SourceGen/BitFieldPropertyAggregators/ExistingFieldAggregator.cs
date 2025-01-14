using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal sealed class ExistingFieldAggregator : BaseBitFieldPropertyAggregator
{
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        using var existingFieldPropertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

        foreach (var property in properties)
        {
            if (property.AttributeParsedResult.FieldName?.IsSymbolExist is true)
            {
                existingFieldPropertiesBuilder.Add(property);
            }
        }

        var existingFieldProperties = existingFieldPropertiesBuilder.ToImmutable();

        if (existingFieldProperties.Length == 0)
        {
            return;
        }

        var groupedFieldProperties = GroupPropertiesByFieldNameAndOwner(existingFieldProperties);


        for (var i = 0; i < groupedFieldProperties.Length; i++)
        {
            var group = groupedFieldProperties[i];

            Debug.Assert(group.FieldName is not null);
            Debug.Assert(group.FieldName!.IsSymbolExist);

            var fieldSymbol = group.FieldName.ExistingSymbol!;
            var propertiesInGroup = group.Properties;
            var bitsSize = MapSpecialTypeToBitSize(fieldSymbol.Type.SpecialType);

            if (!ValidateSize(bitsSize, propertiesInGroup))
            {
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

            var fieldRequest = new ExistingFieldRequest(fieldSymbol);
            var propertyRequests = ToRequests(fieldRequest, propertiesInGroup);

            var gsr = new ExistingFieldGsr(fieldSymbol, propertyRequests);

            requestsBuilder.Add(gsr);
        }
    }
}
