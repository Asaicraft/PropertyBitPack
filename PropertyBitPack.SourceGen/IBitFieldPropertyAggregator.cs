using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen;
public interface IBitFieldPropertyAggregator
{
    /// <summary>
    /// Remove properties in <paramref name="properties"/> which aggregated 
    /// </summary>
    public ImmutableArray<GenerateSourceRequest> Aggregate(SemanticModel semanticModel, in ImmutableArrayBuilder<BitFieldPropertyInfo> properties);
}
