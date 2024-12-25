using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
using PropertyBitPack.SourceGen.BitFieldPropertyParsers;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen;
public abstract partial class PropertyBitPackGeneratorContext
{
    public abstract ImmutableArray<IAttributeParser> AttributeParsers
    {
        get;
    }

    public abstract ImmutableArray<IBitFieldPropertyParser> BitFieldPropertyParsers
    {
        get;
    }

    public abstract ImmutableArray<IBitFieldPropertyAggregator> BitFieldPropertyAggregators
    {
        get;
    }

    public abstract ImmutableArray<IPropertiesSyntaxGenerator> PropertySyntaxGenerators
    {
        get;
    }
}
