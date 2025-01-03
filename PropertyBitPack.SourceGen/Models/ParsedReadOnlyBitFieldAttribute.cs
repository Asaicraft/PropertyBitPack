using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedReadOnlyBitFieldAttribute : AttributeParsedResult
{
    public ParsedReadOnlyBitFieldAttribute(IFieldName? fieldName, int? bitsCount, AccessModifier accessModifier) : base(fieldName, bitsCount)
    {
        SymbolGetterLargeSizeValue = accessModifier;
    }

    public ISymbol SymbolGetterLargeSizeValue
    {
        get;
    }
}
