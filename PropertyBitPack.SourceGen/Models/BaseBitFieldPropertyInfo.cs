using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;


[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
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

    public INamedTypeSymbol Owner => PropertySymbol.ContainingType;

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

        return $"{Owner} => [{AttributeParsedResult}] {PropertySymbol.Type.Name} {PropertySymbol.Name} {{ get; {setterOrInitter} }}";
    }

    private string GetDebuggerDisplay() => ToString();
}
