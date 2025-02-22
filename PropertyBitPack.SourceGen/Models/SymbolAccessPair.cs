using Microsoft.CodeAnalysis;

namespace PropertyBitPack.SourceGen.Models;

internal readonly struct SymbolAccessPair(INamedTypeSymbol owner, AccessModifier accessModifier): IEquatable<SymbolAccessPair>
{
    public readonly INamedTypeSymbol Owner = owner;
    public readonly AccessModifier AccessModifier = accessModifier;

    public readonly override bool Equals(object obj)
    {
        return obj is SymbolAccessPair pair && Equals(pair);
    }

    public readonly bool Equals(SymbolAccessPair other)
    {
        return SymbolEqualityComparer.Default.Equals(Owner, other.Owner) && AccessModifier == other.AccessModifier;
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(Owner);
            hash = hash * 23 + AccessModifier.GetHashCode();

            return hash;
        }
    }

    public static implicit operator (INamedTypeSymbol, AccessModifier)(SymbolAccessPair value)
    {
        return (value.Owner, value.AccessModifier);
    }

    public static implicit operator SymbolAccessPair((INamedTypeSymbol, AccessModifier) value)
    {
        return new SymbolAccessPair(value.Item1, value.Item2);
    }
}