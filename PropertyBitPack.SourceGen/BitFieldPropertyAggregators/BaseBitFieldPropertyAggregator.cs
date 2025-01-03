using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal abstract class BaseBitFieldPropertyAggregator: IBitFieldPropertyAggregator, IContextBindable
{
    private PropertyBitPackGeneratorContext? _context;

    public PropertyBitPackGeneratorContext Context
    {
        get
        {
            Debug.Assert(_context is not null);

            return _context!;
        }
    }

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        _context = context;
    }

    public abstract ImmutableArray<GenerateSourceRequest> Aggregate(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics);
}
