﻿using CommunityToolkit.Diagnostics;
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

namespace PropertyBitPack.SourceGen.Models.AttributeParsedResults;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class ParsedExtendedBitFiledAttribute : AttributeParsedResult, IParsedExtendedBitFiledAttribute
{

    public ParsedExtendedBitFiledAttribute(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, byte? bitsCount, ISymbol symbolGetterLargeSizeValue) : base(attributeSyntax, attributeData, fieldName, bitsCount)
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
