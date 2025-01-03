using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedReadOnlyBitFieldAttribute : AttributeParsedResult
{
    public ParsedReadOnlyBitFieldAttribute(IFieldName? fieldName, int? bitsCount, ISymbol symbolGetterLargeSizeValue) : base(fieldName, bitsCount)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }

    public ISymbol SymbolGetterLargeSizeValue
    {
        get;
    }
}
