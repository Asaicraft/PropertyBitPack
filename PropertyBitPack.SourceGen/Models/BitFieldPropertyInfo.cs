using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

internal sealed class BitFieldPropertyInfo(
    AttributeParsedResult attributeParsedResult,
    bool isInit,
    bool hasInitOrSet,
    SyntaxTokenList setterOrInitModifiers,
    IPropertySymbol propertySymbol) : BaseBitFieldPropertyInfo
{
    public override AttributeParsedResult AttributeParsedResult { get; } = attributeParsedResult;
    public override bool IsInit { get; } = isInit;
    public override bool HasInitOrSet { get; } = hasInitOrSet;
    public override SyntaxTokenList SetterOrInitModifiers { get; } = setterOrInitModifiers;
    public override IPropertySymbol PropertySymbol { get; } = propertySymbol;
}
