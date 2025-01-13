using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal sealed class FieldNameEqualityComparer : IEqualityComparer<IFieldName?>
{
    public static readonly FieldNameEqualityComparer Default = new();

    public bool Equals(IFieldName? x, IFieldName? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if(x.IsSymbolExist == y.IsSymbolExist)
        {
            if(x.IsSymbolExist)
            {
                return SymbolEqualityComparer.Default.Equals(x.ExistingSymbol, y.ExistingSymbol);
            }
        }

        return StringComparer.Ordinal.Equals(x.Name, y.Name);
    }

    public int GetHashCode(IFieldName? obj)
    {
        if (obj is null)
        {
            return 0;
        }
        if (obj.IsSymbolExist)
        {
            return SymbolEqualityComparer.Default.GetHashCode(obj.ExistingSymbol);
        }
        return StringComparer.Ordinal.GetHashCode(obj.Name);
    }
}
