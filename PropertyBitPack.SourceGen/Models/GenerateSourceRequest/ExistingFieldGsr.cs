using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
internal sealed class ExistingFieldGsr : GenerateSourceRequest
{
    private readonly ImmutableArray<BitFieldPropertyInfoRequest> _bitFieldPropertyInfoRequests;
    private readonly ImmutableArray<IFieldRequest> _singleFields;

    public ExistingFieldGsr(IFieldSymbol fieldSymbol, ImmutableArray<BitFieldPropertyInfoRequest> bitFieldPropertyInfoRequests)
    {
        _bitFieldPropertyInfoRequests = bitFieldPropertyInfoRequests;
        _singleFields = [new ExistingFieldRequest(fieldSymbol)];

    }

    public IFieldSymbol FieldSymbol => Unsafe.As<ExistingFieldRequest>(_singleFields[0]).FieldSymbol;
    public override ImmutableArray<IFieldRequest> Fields => _singleFields;
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties => _bitFieldPropertyInfoRequests;
}
