using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class UnnamedFieldGsr(FieldRequest field, ImmutableArray<BitFieldPropertyInfoRequest> properties) : GenerateSourceRequest
{
    public override ImmutableArray<FieldRequest> Fields { get; } = [field];
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
}
