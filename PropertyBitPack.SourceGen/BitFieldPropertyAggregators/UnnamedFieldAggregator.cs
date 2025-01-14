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
                unnamedFieldPropertiesBuilder.Add(property);
            }
        }
    }

}
