using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.FieldRequests;
internal sealed class ExistingFieldRequest(IFieldSymbol fieldSymbol) : FieldRequest(fieldSymbol.Name, fieldSymbol.Type.SpecialType, true)
{
    private readonly IFieldSymbol _fieldSymbol = fieldSymbol;

    public IFieldSymbol FieldSymbol => _fieldSymbol;
}
