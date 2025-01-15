using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
internal static class SimpleLinkedListsPool<T>
{
    private static readonly ConcurrentStack<SimpleLinkedList<T>> pool = new();

    static SimpleLinkedListsPool()
    {
        pool.Push([]);
        pool.Push([]);
    }

    public static SimpleLinkedList<T> Rent()
    {
        if (pool.TryPop(out var list))
        {
            return list;
        }
        return [];
    }

    public static void Return(SimpleLinkedList<T> list)
    {
        if (pool.Count > 4)
        {
            return;
        }
        list.Clear();
        pool.Push(list);
    }
}
