using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal sealed class SymbolAccessPairComparer : IEqualityComparer<SymbolAccessPair>
{
    public static readonly SymbolAccessPairComparer Default = new();

    public bool Equals(SymbolAccessPair x, SymbolAccessPair y)
    {
        return SymbolEqualityComparer.Default.Equals(x.Owner, y.Owner) && x.AccessModifier == y.AccessModifier;
    }

    public int GetHashCode(SymbolAccessPair obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(obj.Owner);
            hash = hash * 23 + obj.AccessModifier.GetHashCode();

            return hash;
        }
    }
}
