using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public sealed class BitFieldPropertyInfoRequest(BitsSpan bitsSpan, BitFieldPropertyInfo bitFieldPropertyInfo)
{
    private readonly BitsSpan _bitsSpan = bitsSpan;
    private readonly BitFieldPropertyInfo _bitFieldPropertyInfo = bitFieldPropertyInfo;

    public BitsSpan BitsSpan => _bitsSpan;

    public BitFieldPropertyInfo BitFieldPropertyInfo => _bitFieldPropertyInfo;

    public AttributeParsedResult AttributeParsedResult => _bitFieldPropertyInfo.AttributeParsedResult;

    public bool IsInit => _bitFieldPropertyInfo.IsInit;

    public bool HasInitOrSet => _bitFieldPropertyInfo.HasInitOrSet;
    
    public SyntaxTokenList SetterOrInitModifiers => _bitFieldPropertyInfo.SetterOrInitModifiers;

    public IPropertySymbol PropertySymbol => _bitFieldPropertyInfo.PropertySymbol;

    public ITypeSymbol PropertyType => PropertySymbol.Type;

    public INamedTypeSymbol Owner => PropertyType.ContainingType;
}
