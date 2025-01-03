using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;

/// <summary>
/// A generic pool for <see cref="Dictionary{TKey, TValue}"/> instances.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
/// <typeparam name="TValue">The type of the dictionary values.</typeparam>
internal static class DictionariesPool<TKey, TValue>
{
    private static readonly ConcurrentStack<Dictionary<TKey, TValue>> pool = new();

    /// <summary>
    /// Rents a <see cref="Dictionary{TKey, TValue}"/> instance from the pool,
    /// or creates a new one if the pool is empty.
    /// </summary>
    public static Dictionary<TKey, TValue> Rent()
    {
        if (pool.TryPop(out var dictionary))
        {
            return dictionary;
        }

        return [];
    }

    /// <summary>
    /// Returns the provided <see cref="Dictionary{TKey, TValue}"/> instance to the pool for future reuse.
    /// The dictionary is cleared before being stored.
    /// </summary>
    /// <param name="dictionary">The dictionary to return to the pool.</param>
    public static void Return(Dictionary<TKey, TValue> dictionary)
    {
        dictionary.Clear();
        pool.Push(dictionary);
    }
}
