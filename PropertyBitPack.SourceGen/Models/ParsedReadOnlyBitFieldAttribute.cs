using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class ParsedReadOnlyBitFieldAttribute : AttributeParsedResult, IParsedReadOnlyBitFieldAttribute
{
    public ParsedReadOnlyBitFieldAttribute(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, byte? bitsCount, AccessModifier accessModifier) : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
        ConstructorAccessModifier = accessModifier;
    }

    public AccessModifier ConstructorAccessModifier
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

        return $"{nameof(IReadOnlyBitFieldAttribute)}({nameof(BitsCount)}={BitsCount}, {nameof(FieldName)}={nameOfFieldNameOrJustName}, {nameof(ConstructorAccessModifier)}={ConstructorAccessModifier})";
    }
}
