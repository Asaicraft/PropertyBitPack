using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal sealed class UnnamedFieldAggregator : BaseBitFieldPropertyAggregator
{
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder,in ImmutableArrayBuilder<Diagnostic> diagnostics)
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

            try
            {
                AddGsrs(group, calculatedBitCount, in requestsBuilder);

                for (var j = 0; j < group.Properties.Length; j++)
                {
                    var property = group.Properties[j];
                    properties.Remove(property);
                }
            }
            catch(Exception exc)
            {
                // This is a critical error, we should break the debugger here.
                Debug.WriteLine(exc);
                Debugger.Break();
                continue;
            }
        }
    }

    private static bool ValidateSize(BaseBitFieldPropertyInfo baseBitFieldPropertyInfo)
    {
        var bits = BitCountHelper.GetEffectiveBitsCount(baseBitFieldPropertyInfo);

        return bits > 0;
    }

    private static void AddGsrs(OwnerFieldNameGroup ownerFieldNameGroup, ImmutableArray<CalculatedBits> calculatedBits, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder)
    {
        using var list = ListsPool.Rent<BaseBitFieldPropertyInfo>();
        
        list.AddRange(ownerFieldNameGroup.Properties);

        for (var i = 0; i < calculatedBits.Length; i++)
        {
            var calculatedBit = calculatedBits[i];
            using var calculatedBitList = ListsPool.Rent<BitFieldPropertyInfoRequest>();
            using var candidateProperties = ListsPool.Rent<BaseBitFieldPropertyInfo>();


            for (var j = 0; j < calculatedBit.Bits.Length; j++)
            {
                var bitsSize = calculatedBit.Bits[j];

                var candidateProperty = list.FirstOrDefault(x => BitCountHelper.GetEffectiveBitsCount(x) == bitsSize);

                if (candidateProperty is null)
                {
                    ThrowHelper.ThrowUnreachableException("Property not found.");
                }

                list.Remove(candidateProperty);
                
                candidateProperties.Add(candidateProperty);
            }

            var fieldName = GetFieldName(in candidateProperties);
            var fieldRequest = new NonExistingFieldRequest(fieldName, MapBitSizeToSpecialType(calculatedBit.FieldCapacity));

            byte offset = 0;

            for (var j = 0; j < candidateProperties.Count; j++)
            {
                var candidateProperty = candidateProperties[j];
                var bitsSize = calculatedBit.Bits[j];

                var bitsSpan = new BitsSpan(fieldRequest, offset, bitsSize);
                var bitFieldPropertyInfoRequest = new BitFieldPropertyInfoRequest(bitsSpan, candidateProperty);

                calculatedBitList.Add(bitFieldPropertyInfoRequest);
            }

            var gsr = new UnnamedFieldGsr(fieldRequest, [.. calculatedBitList]);

            requestsBuilder.Add(gsr);
        }


    }

    private static string GetFieldName(ref readonly ListsPool.RentedListPool<BaseBitFieldPropertyInfo> baseBitFieldPropertyInfos)
    {
        using var rentedStringBuilder = StringBuildersPool.Rent();
        
        var stringBuilder = rentedStringBuilder.StringBuilder;
        stringBuilder.Append("_");

        for (var i = 0; i < baseBitFieldPropertyInfos.Count; i++)
        {
            var property = baseBitFieldPropertyInfos[i];

            stringBuilder.Append(property.PropertySymbol.Name);
            stringBuilder.Append("__");
        }

        return stringBuilder.ToString();
    }
}
