using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

internal sealed class BitFieldPropertyInfo(
    PropertyDeclarationSyntax propertyDeclarationSyntax,
    IAttributeParsedResult attributeParsedResult,
    bool isInit,
    bool hasInitOrSet,
    SyntaxTokenList setterOrInitModifiers,
    IPropertySymbol propertySymbol) : BaseBitFieldPropertyInfo
{
    public override PropertyDeclarationSyntax PropertyDeclarationSyntax { get; } = propertyDeclarationSyntax;
    public override IAttributeParsedResult AttributeParsedResult { get; } = attributeParsedResult;
    public override bool IsInit { get; } = isInit;
    public override bool HasInitOrSet { get; } = hasInitOrSet;
    public override SyntaxTokenList SetterOrInitModifiers { get; } = setterOrInitModifiers;
    public override IPropertySymbol PropertySymbol { get; } = propertySymbol;
}
