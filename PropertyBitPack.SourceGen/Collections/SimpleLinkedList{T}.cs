using CommunityToolkit.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;

[DebuggerTypeProxy(typeof(SimpleLinkedList<>.SimpleLinkedListProxy))]
[DebuggerDisplay("Count = {Count}")]
internal sealed class SimpleLinkedList<T> : ILinkedList<T>
{
    private readonly LinkedList<T> _linkedList;

    public SimpleLinkedList()
    {
        _linkedList = new LinkedList<T>();
    }

    public SimpleLinkedList(IEnumerable<T> collection)
    {
        _linkedList = new LinkedList<T>(collection);
    }

    public void AddRange(IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            _linkedList.AddLast(value);
        }
    }

    public void AddRange(ReadOnlySpan<T> values)
    {
        for(var i = 0; i < values.Length; i++)
        {
            _linkedList.AddLast(values[i]);
        }
    }

    public void AddRange(IReadOnlyList<T> values)
    {
        for (var i = 0; i < values.Count; i++)
        {
            _linkedList.AddLast(values[i]);
        }
    }

    public void AddRange(ImmutableArray<T> values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _linkedList.AddLast(values[i]);
        }
    }

    public int Count => ((ICollection<T>)_linkedList).Count;

    public bool IsReadOnly => ((ICollection<T>)_linkedList).IsReadOnly;

    public void Add(T item)
    {
        ((ICollection<T>)_linkedList).Add(item);
    }

    public bool AddAfter(T existingValue, T value)
    {
        var node = _linkedList.Find(existingValue);

        if (node == null)
        {
            return false;
        }

        _linkedList.AddAfter(node, value);

        return true;
    }

    public bool AddBefore(T existingValue, T value)
    {
        var node = _linkedList.Find(existingValue);
        if (node == null)
        {
            return false;
        }
        _linkedList.AddBefore(node, value);
        return true;
    }

    public void AddFirst(T value) => _linkedList.AddFirst(value);

    public void AddLast(T value) => _linkedList.AddLast(value);

    public void Clear()
    {
        ((ICollection<T>)_linkedList).Clear();
    }

    public bool Contains(T item)
    {
        return ((ICollection<T>)_linkedList).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)_linkedList).CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_linkedList).GetEnumerator();
    }

    public bool Remove(T item)
    {
        return ((ICollection<T>)_linkedList).Remove(item);
    }

    public bool RemoveFirst()
    {
        _linkedList.RemoveFirst();

        return true;
    }

    public bool RemoveLast()
    {
        _linkedList.RemoveLast();
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_linkedList).GetEnumerator();
    }


    internal sealed class SimpleLinkedListProxy
    {
        private readonly SimpleLinkedList<T> _collection;

        public SimpleLinkedListProxy(SimpleLinkedList<T> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(nameof(collection));
            }

            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
