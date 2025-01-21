using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
internal sealed class UnnamedFieldGsr(NonExistingFieldRequest fieldRequest, ImmutableArray<BitFieldPropertyInfoRequest> properties) : NonExistingFieldGsr
{
    public NonExistingFieldRequest FieldRequest => Unsafe.As<NonExistingFieldRequest>(Fields[0]);
    public override ImmutableArray<NonExistingFieldRequest> NonExistingFieldRequests { get; } = [fieldRequest];
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;
}
