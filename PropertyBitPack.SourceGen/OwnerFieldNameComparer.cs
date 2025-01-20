using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;

/// <summary>
/// It's need to dictionary key comparison.
/// </summary>
internal sealed class OwnerFieldNameComparer : IEqualityComparer<(INamedTypeSymbol, IFieldName?)>
{
    public static readonly OwnerFieldNameComparer Default = new();

    private OwnerFieldNameComparer()
    {
    }

    public bool Equals((INamedTypeSymbol, IFieldName?) x, (INamedTypeSymbol, IFieldName?) y)
    {
        if(ReferenceEquals(x,y))
        {
            return true;
        }

        return SymbolEqualityComparer.Default.Equals(x.Item1, y.Item1) &&
            FieldNameEqualityComparer.Default.Equals(x.Item2, y.Item2);
    }

    public int GetHashCode((INamedTypeSymbol, IFieldName?) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.Item1);
            hash = hash * 23 + FieldNameEqualityComparer.Default.GetHashCode(obj.Item2);

            return hash;
        }
    }
}
