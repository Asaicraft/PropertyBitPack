using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class SimpleGenerateSourceRequest(ImmutableArray<FieldRequest> fields, ImmutableArray<BitFieldPropertyInfoRequest> properties) : GenerateSourceRequest
{
    public override ImmutableArray<FieldRequest> Fields { get; } = fields;
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
}
