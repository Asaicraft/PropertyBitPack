using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Collections;
internal interface ILinkedList<T>: ICollection<T>
{
    public bool AddAfter(T existingValue, T value);
    public bool AddBefore(T existingValue, T value);

    public void AddFirst(T value);
    public void AddLast(T value);

    public bool RemoveFirst();
    public bool RemoveLast();
}
