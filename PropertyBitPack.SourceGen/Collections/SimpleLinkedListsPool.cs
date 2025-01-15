using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
internal static class SimpleLinkedListsPool
{
    public static RentedSimpleLinkedList<T> Rent<T>()
    {
        return new RentedSimpleLinkedList<T>(SimpleLinkedListsPool<T>.Rent());
    }

    public static void Return<T>(SimpleLinkedList<T> list)
    {
        SimpleLinkedListsPool<T>.Return(list);
    }

    public static void Return<T>(RentedSimpleLinkedList<T> list)
    {
        list.Dispose();
    }

    public readonly ref struct RentedSimpleLinkedList<T>(SimpleLinkedList<T> list): ILinkedList<T>
    {
        private readonly SimpleLinkedList<T> _list = list;

        public SimpleLinkedList<T> List => _list;
        public void Dispose()
        {
            SimpleLinkedListsPool<T>.Return(_list);
        }

        #region ILinkedList<T> implementation
        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public void Add(T item)
        {
            _list.Add(item);
        }

        public bool AddAfter(T existingValue, T value)
        {
            return _list.AddAfter(existingValue, value);
        }

        public bool AddBefore(T existingValue, T value)
        {
            return _list.AddBefore(existingValue, value);
        }

        public void AddFirst(T value)
        {
            _list.AddFirst(value);
        }

        public void AddLast(T value)
        {
            _list.AddLast(value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public bool RemoveFirst()
        {
            return _list.RemoveFirst();
        }

        public bool RemoveLast()
        {
            return _list.RemoveLast();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        #endregion
    }
}