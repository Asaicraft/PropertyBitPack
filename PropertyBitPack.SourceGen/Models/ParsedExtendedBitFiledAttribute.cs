using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedExtendedBitFiledAttribute : AttributeParsedResult
{

    public ParsedExtendedBitFiledAttribute(IFieldName? fieldName, int? bitsCount, ISymbol symbolGetterLargeSizeValue): base(fieldName, bitsCount)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }

    public ISymbol SymbolGetterLargeSizeValue
    {
        get;
    }


}
