using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.AttributeParsedResults;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class ParsedReadOnlyExtendedBitFieldAttribute : AttributeParsedResult, IParsedReadOnlyBitFieldAttribute, IParsedExtendedBitFiledAttribute
{
    public ParsedReadOnlyExtendedBitFieldAttribute(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, byte? bitsCount, AccessModifier accessModifier, ISymbol symbolGetterLargeSizeValue) : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
        ConstructorAccessModifier = accessModifier;
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }

    public AccessModifier ConstructorAccessModifier
    {
        get;
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

        return $"{nameof(IReadOnlyBitFieldAttribute)}({nameof(BitsCount)}={BitsCount}, {nameof(FieldName)}={nameOfFieldNameOrJustName}, {nameof(ConstructorAccessModifier)}={ConstructorAccessModifier})";
    }
}
