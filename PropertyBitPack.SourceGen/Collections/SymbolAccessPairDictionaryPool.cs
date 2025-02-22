using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;

internal static class SymbolAccessPairDictionaryPool
{
    private static readonly ConcurrentStack<Dictionary<SymbolAccessPair, FieldsPropertiesPair>> pool = [];

    static SymbolAccessPairDictionaryPool()
    {
        pool.Push(new(SymbolAccessPairComparer.Default));
        pool.Push(new(SymbolAccessPairComparer.Default));
        pool.Push(new(SymbolAccessPairComparer.Default));
    }

    public static Dictionary<SymbolAccessPair, FieldsPropertiesPair> Rent()
    {
        if (pool.TryPop(out var dictionary))
        {
            return dictionary;
        }

        return new(SymbolAccessPairComparer.Default);
    }

    public static void Return(Dictionary<SymbolAccessPair, FieldsPropertiesPair> dictionary)
    {
        dictionary.Clear();

        if (pool.Count > 4)
        {
            return;
        }

        if (dictionary.Comparer != SymbolAccessPairComparer.Default)
        {
            ThrowHelper.ThrowArgumentException("The dictionary comparer is not the default comparer.");
        }

        pool.Push(dictionary);
    }
}
