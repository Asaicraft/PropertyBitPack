using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;


[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class ParsedBitFiledAttribute : AttributeParsedResult
{
    public ParsedBitFiledAttribute(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, byte? bitsCount) : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
    }

    public override string ToString()
    {
        var nameOfFieldNameOrJustName = FieldName?.IsSymbolExist ?? false
            ? $"nameof({FieldName.Name})"
            : FieldName?.Name ?? "<unnamed>";

        return $"{nameof(BitFieldAttribute)}({nameof(BitsCount)}={BitsCount}, {nameof(FieldName)}={nameOfFieldNameOrJustName})";
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
