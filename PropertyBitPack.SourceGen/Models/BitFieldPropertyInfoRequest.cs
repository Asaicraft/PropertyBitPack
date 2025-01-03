using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class BitFieldPropertyInfoRequest(BitsSpan bitsSpan, BaseBitFieldPropertyInfo bitFieldPropertyInfo)
{
    private readonly BitsSpan _bitsSpan = bitsSpan;
    private readonly BaseBitFieldPropertyInfo _bitFieldPropertyInfo = bitFieldPropertyInfo;

    public BitsSpan BitsSpan => _bitsSpan;

    public BaseBitFieldPropertyInfo BitFieldPropertyInfo => _bitFieldPropertyInfo;

    public AttributeParsedResult AttributeParsedResult => _bitFieldPropertyInfo.AttributeParsedResult;

    public bool IsInit => _bitFieldPropertyInfo.IsInit;

    public bool HasInitOrSet => _bitFieldPropertyInfo.HasInitOrSet;
    
    public SyntaxTokenList SetterOrInitModifiers => _bitFieldPropertyInfo.SetterOrInitModifiers;

    public IPropertySymbol PropertySymbol => _bitFieldPropertyInfo.PropertySymbol;

    public ITypeSymbol PropertyType => PropertySymbol.Type;

    public INamedTypeSymbol Owner => PropertyType.ContainingType;

    private string GetDebuggerDisplay()
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

        return $"[{AttributeParsedResult}] {PropertySymbol.Type.Name} {PropertySymbol.Name} {{ get; {setterOrInitter} }} => {BitsSpan}";
    }
}
