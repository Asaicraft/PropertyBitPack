using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal sealed class ReadOnlyBitFieldAggregator : BaseBitFieldPropertyAggregator
{
    public readonly ReadOnlyAggregatorComponent ReadOnlyAggregatorComponent = new();


    public override ImmutableArray<IGenerateSourceRequest> Aggregate(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<IGenerateSourceRequest> readyRequests, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        var readOnlyProperties = ReadOnlyAggregatorComponent.Aggregate(properties, readyRequests, diagnostics);

        return Unsafe.As<ImmutableArray<IReadOnlyFieldGsr>, ImmutableArray<IGenerateSourceRequest>>(ref readOnlyProperties);
    }
}
