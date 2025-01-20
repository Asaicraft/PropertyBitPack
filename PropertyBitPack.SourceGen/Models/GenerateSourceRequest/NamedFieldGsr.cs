using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
internal sealed class NamedFieldGsr(NamedFieldRequest fieldRequest, ImmutableArray<BitFieldPropertyInfoRequest> properties) : NonExistingFieldGsr
{
    public NamedFieldRequest FieldRequest => Unsafe.As<NamedFieldRequest>(Fields[0]);
    public override ImmutableArray<NonExistingFieldRequest> NonExistingFieldRequests { get; } = [fieldRequest];
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
}
