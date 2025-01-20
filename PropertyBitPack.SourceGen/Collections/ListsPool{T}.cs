using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;

/// <summary>
/// A generic pool for <see cref="List{T}"/> instances.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
internal static class ListsPool<T>
{
    // Using a thread-safe collection if multithreading is possible in your scenario.
    private static readonly ConcurrentStack<List<T>> pool = new();

    static ListsPool()
    {
        pool.Push([]);
        pool.Push([]);
        pool.Push([]);
    }

    /// <summary>
    /// Rents a <see cref="List{T}"/> instance from the pool, or creates a new one if the pool is empty.
    /// </summary>
    public static List<T> Rent()
    {
        if (pool.TryPop(out var list))
        {
            return list;
        }

        return [];
    }

    /// <summary>
    /// Returns the provided <see cref="List{T}"/> instance to the pool for future reuse.
    /// The list is cleared before being stored.
    /// </summary>
    /// <param name="list">The list to return to the pool.</param>
    public static void Return(List<T> list)
    {
        if(pool.Count > 4)
        {
            // avoid memory leak
            // avoid keeping too many lists in the pool
            return;
        }

        list.Clear();
        pool.Push(list);
    }
}
