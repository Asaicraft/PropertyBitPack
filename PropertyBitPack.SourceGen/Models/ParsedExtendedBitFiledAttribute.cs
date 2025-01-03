using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class ParsedExtendedBitFiledAttribute : AttributeParsedResult
{

    public ParsedExtendedBitFiledAttribute(IFieldName? fieldName, int? bitsCount, ISymbol symbolGetterLargeSizeValue): base(fieldName, bitsCount)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }

    public ISymbol SymbolGetterLargeSizeValue
    {
        get;
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    public override string ToString()
    {
        var nameOfFieldNameOrJustName = FieldName?.IsSymbolExist ?? false
            ? $"nameof({FieldName.Name})"
            : FieldName?.Name ?? "<unnamed>";

        return $"{nameof(IExtendedBitFieldAttribute)}({nameof(BitsCount)}={BitsCount}, {nameof(FieldName)}={nameOfFieldNameOrJustName}, {nameof(SymbolGetterLargeSizeValue)}={SymbolGetterLargeSizeValue})";
    }
}
