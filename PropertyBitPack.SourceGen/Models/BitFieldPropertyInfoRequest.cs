using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    public IAttributeParsedResult AttributeParsedResult => _bitFieldPropertyInfo.AttributeParsedResult;

    public bool IsInit => _bitFieldPropertyInfo.IsInit;

    public bool HasInitOrSet => _bitFieldPropertyInfo.HasInitOrSet;
    
    public SyntaxTokenList SetterOrInitModifiers => _bitFieldPropertyInfo.SetterOrInitModifiers;

    public IPropertySymbol PropertySymbol => _bitFieldPropertyInfo.PropertySymbol;

    public PropertyDeclarationSyntax PropertyDeclarationSyntax => _bitFieldPropertyInfo.PropertyDeclarationSyntax;

    public ITypeSymbol PropertyType => PropertySymbol.Type;

    public INamedTypeSymbol Owner => PropertySymbol.ContainingType;

    public override string ToString() => $"{_bitFieldPropertyInfo} => {BitsSpan}";

    private string GetDebuggerDisplay() => ToString();
}
