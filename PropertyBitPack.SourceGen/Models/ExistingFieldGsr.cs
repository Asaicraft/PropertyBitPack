using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class ExistingFieldGsr: GenerateSourceRequest
{
    private readonly IFieldSymbol _fieldSymbol;
    private readonly ImmutableArray<BitFieldPropertyInfoRequest> _bitFieldPropertyInfoRequests;
    private readonly ImmutableArray<FieldRequest> _singleFields;

    public ExistingFieldGsr(IFieldSymbol fieldSymbol, ImmutableArray<BitFieldPropertyInfoRequest> bitFieldPropertyInfoRequests)
    {
        _fieldSymbol = fieldSymbol;
        _bitFieldPropertyInfoRequests = bitFieldPropertyInfoRequests;
        _singleFields = [new ExistingFieldRequest(_fieldSymbol)];

    }

    public override ImmutableArray<FieldRequest> Fields => _singleFields;
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties => _bitFieldPropertyInfoRequests;
}
