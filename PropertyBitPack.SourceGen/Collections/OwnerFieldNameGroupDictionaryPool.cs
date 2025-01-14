using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
internal static class OwnerFieldNameGroupDictionaryPool
{
    private static readonly ConcurrentStack<Dictionary<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>>> pool = new();

    static OwnerFieldNameGroupDictionaryPool()
    {
        pool.Push(new(OwnerFieldNameComparer.Default));
        pool.Push(new(OwnerFieldNameComparer.Default));
        pool.Push(new(OwnerFieldNameComparer.Default));
    }

    public static Dictionary<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>> Rent()
    {
        if (pool.TryPop(out var dictionary))
        {
            return dictionary;
        }

        return new(OwnerFieldNameComparer.Default);
    }

    public static void Return(Dictionary<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>> dictionary)
    {
        if (pool.Count > 4)
        {
            return;
        }

        if(dictionary.Comparer != OwnerFieldNameComparer.Default)
        {
            ThrowHelper.ThrowArgumentException("The dictionary comparer is not the default comparer.");
        }

        dictionary.Clear();
        
        pool.Push(dictionary);
    }
}
