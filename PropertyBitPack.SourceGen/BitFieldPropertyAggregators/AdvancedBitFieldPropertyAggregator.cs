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
    protected override void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, ImmutableArrayBuilder<Diagnostic> diagnostics)
    {

    }

}