using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class NamedFieldGsr : GenerateSourceRequest
{

    public NamedFieldGsr(NamedFieldRequest fieldRequest, ImmutableArray<BitFieldPropertyInfoRequest> properties)
    {
        Fields = [fieldRequest];
        Properties = properties;
    }

    public NamedFieldRequest FieldRequest => Unsafe.As<NamedFieldRequest>(Fields[0]);
    public override ImmutableArray<FieldRequest> Fields { get; }
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; }

    
}
