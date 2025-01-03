using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
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

    public override string ToString()
    {
        var setterOrInitter = HasInitOrSet
            ? IsInit
                ? "init;"
                : "set;"
            : string.Empty;

        if (HasInitOrSet)
        {
            setterOrInitter = $"{SetterOrInitModifiers.ToFullString()} {setterOrInitter}";
        }

        return $"[{AttributeParsedResult}] {PropertySymbol.Type.Name} {PropertySymbol.Name} {{ get; {setterOrInitter} }}";
    }

    private string GetDebuggerDisplay() => ToString();
}
