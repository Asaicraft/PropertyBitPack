using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal abstract class BaseBitFieldPropertyInfo
{
    public abstract AttributeParsedResult AttributeParsedResult
    {
        get;
    }

    public abstract bool IsInit
    {
        get;
    }

    public abstract bool HasInitOrSet
    {
        get;
    }

    public abstract SyntaxTokenList SetterOrInitModifiers
    {
        get;
    }

    public abstract IPropertySymbol PropertySymbol
    {
        get;
    }

    public ITypeSymbol PropertyType => PropertySymbol.Type;

    public INamedTypeSymbol Owner => PropertyType.ContainingType;
}
