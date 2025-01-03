using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

internal abstract class GenerateSourceRequest
{
    public abstract ImmutableArray<FieldRequest> Fields { get; }

    public abstract ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; }
}
