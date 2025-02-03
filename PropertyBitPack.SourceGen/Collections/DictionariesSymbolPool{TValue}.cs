using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
internal static class DictionariesSymbolPool<TValue>
{
    private static readonly ConcurrentStack<Dictionary<ISymbol, TValue>> pool = [];

    static DictionariesSymbolPool()
    {
        pool.Push(new(SymbolEqualityComparer.Default));
        pool.Push(new(SymbolEqualityComparer.Default));
    }

    public static Dictionary<ISymbol, TValue> Rent()
    {
        if (pool.TryPop(out var dictionary))
        {
            return dictionary;
        }
        return new(SymbolEqualityComparer.Default);
    }

    public static void Return(Dictionary<ISymbol, TValue> dictionary)
    {
        if (pool.Count > 4)
        {
            return;
        }

        if(dictionary.Comparer != SymbolEqualityComparer.Default)
        {
            return;
        }

        dictionary.Clear();
        pool.Push(dictionary);
    }

    public static RentedToken RentToken()
    {
        return new(Rent());
    }

    public readonly ref struct RentedToken
    {
        private readonly Dictionary<ISymbol, TValue> _dictionary;
        public RentedToken(Dictionary<ISymbol, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public void Dispose()
        {
            DictionariesSymbolPool<TValue>.Return(_dictionary);
        }
    }
}
