using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using CommunityToolkit.Diagnostics;

namespace PropertyBitPack.SourceGen.Collections;
internal static partial class ListsPool
{
    public static RentedListPool<T> Rent<T>()
    {
        return new RentedListPool<T>(ListsPool<T>.Rent());
    }

    public static void Return<T>(List<T> list)
    {
        ListsPool<T>.Return(list);
    }

    public static void Return<T>(RentedListPool<T> list)
    {
        list.Dispose();
    }

    public readonly ref struct RentedListPool<T>(List<T> list) : IList<T>
    {
        private readonly List<T> _list = list;

        public void Dispose()
        {
            ListsPool<T>.Return(_list);
        }

        #region IList<T> implementation
        public T this[int index] { get => ((IList<T>)_list)[index]; set => ((IList<T>)_list)[index] = value; }

        public int Count => ((ICollection<T>)_list).Count;

        public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

        public void Add(T item)
        {
            ((ICollection<T>)_list).Add(item);
        }

        public void Clear()
        {
            ((ICollection<T>)_list).Clear();
        }

        public bool Contains(T item)
        {
            return ((ICollection<T>)_list).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((ICollection<T>)_list).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)_list).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)_list).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((ICollection<T>)_list).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)_list).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        #endregion

        public ImmutableArray<T> ToImmutable()
        {
            return [.. _list];
        }

        public void AddRange(IList<T> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                _list.Add(items[i]);
            }
        }

        public void AddRange(ImmutableArray<T> items)
        {
            AddRange(items.AsSpan());
        }

        public void AddRange(IEnumerable<T> items)
        {
            _list.AddRange(items);
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                _list.Add(items[i]);
            }
        }

        public T First(Func<T, bool> predicate)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                if (predicate(item))
                {
                    return item;
                }
            }

            return ThrowHelper.ThrowInvalidOperationException<T>("No item found.");
        }

        public T FirstOrDefault(Func<T, bool> predicate, T defaultValue = default!)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                if (predicate(item))
                {
                    return item;
                }
            }
            return defaultValue;
        }
    }
}
