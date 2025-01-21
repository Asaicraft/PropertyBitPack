using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal interface IBitFieldPropertyAggregator
{
    /// <summary>
    /// Remove properties in <paramref name="properties"/> which aggregated 
    /// </summary>
    public ImmutableArray<GenerateSourceRequest> Aggregate(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics);
}
