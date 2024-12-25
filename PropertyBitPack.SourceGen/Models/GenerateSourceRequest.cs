using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public abstract class GenerateSourceRequest
{
    public ImmutableArray<FieldRequest> Fields { get; }

    public ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; }
}
