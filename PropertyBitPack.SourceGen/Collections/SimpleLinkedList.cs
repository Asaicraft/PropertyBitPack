using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
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
}
