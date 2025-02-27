# 🏗️ ImmutableArrayBuilder<T>

## Overview

`ImmutableArrayBuilder<T>` is a **high-performance builder** for `ImmutableArray<T>`, allowing efficient **buffered** writes before finalizing an immutable collection. 

This structure minimizes memory allocations and uses **buffer pooling** (`ArrayPool<T>`) to enhance performance.

## ✨ Key Features
- **Pooled buffer allocation** – avoids unnecessary GC allocations.
- **Efficient appends** – supports adding single values and bulk operations.
- **`ReadOnlySpan<T>` support** – allows direct access to written data without allocations.
- **No heap allocations** – uses `ref struct` to enforce stack-only usage.
- **Implements `IDisposable`** – ensures pooled memory is released properly.

---

## 🔹 How It Works

### 🚀 Renting a Builder
Instead of directly allocating memory, you **rent** a builder:

```csharp
using var builder = ImmutableArrayBuilder<int>.Rent();

builder.Add(10);
builder.AddRange(new[] { 20, 30, 40 });

// Convert to an ImmutableArray
ImmutableArray<int> result = builder.ToImmutable();
```
When the `using` block exits, the **pooled buffer is automatically returned**.

### 🔹 Adding Items
You can **add** elements dynamically:
```csharp
builder.Add(5);
builder.AddRange(new int[] { 6, 7, 8 });
```

### 🔹 Accessing Data
- `builder.WrittenSpan` → Provides a **readonly view** of written items.
- `builder.ToArray()` → Returns a **new array**.
- `builder.ToImmutable()` → Converts to an **ImmutableArray<T>**.

### 🔹 Performance Optimization
- Uses **aggressive inlining** (`[MethodImpl(MethodImplOptions.AggressiveInlining)]`) for performance.
- `ArrayPool<T>.Shared` handles **buffer reuse** to reduce heap allocations.
- Automatic **buffer resizing** ensures space without frequent reallocations.

---

## 🛠️ Internals & Implementation Details

### 📌 `ImmutableArrayBuilder<T>`
The main structure **wraps a pooled writer**:
```csharp
internal ref struct ImmutableArrayBuilder<T>
{
    private Writer? writer;

    public static ImmutableArrayBuilder<T> Rent() => new(new Writer());

    public static ImmutableArrayBuilder<T> Rent(int initialCapacity) => new(new Writer(initialCapacity));

    public bool IsDefault => writer is null;

    public ReadOnlySpan<T> WrittenSpan => writer!.WrittenSpan;

    public void Add(T item) => writer!.Add(item);

    public void AddRange(ReadOnlySpan<T> items) => writer!.AddRange(items);

    public ImmutableArray<T> ToImmutable()
    {
        var array = writer!.WrittenSpan.ToArray();
        return Unsafe.As<T[], ImmutableArray<T>>(ref array);
    }

    public void Dispose()
    {
        var w = writer;
        writer = null;
        w?.Dispose();
    }
}
```

### 📌 `Writer` (Internal Buffer Manager)
Handles buffer **allocation, resizing, and disposal**:
```csharp
private sealed class Writer : ICollection<T>, IDisposable
{
    private T?[]? array;
    private int index;

    public Writer() => array = ArrayPool<T?>.Shared.Rent(8);

    public void Add(T value)
    {
        EnsureCapacity(1);
        array![index++] = value;
    }

    public void Dispose()
    {
        var arr = array;
        array = null;
        if (arr is not null)
        {
            ArrayPool<T?>.Shared.Return(arr, clearArray: typeof(T) != typeof(char));
        }
    }
}
```

### 📌 Dynamic Resizing
To avoid unnecessary allocations, the buffer **doubles in size** when needed:
```csharp
private void EnsureCapacity(int requestedSize)
{
    if (requestedSize > array!.Length - index)
    {
        ResizeBuffer(requestedSize);
    }
}
```
A new **pooled array** is allocated only when necessary.

---

## 🔥 Performance Benefits

| Feature                | Benefit 🚀 |
|------------------------|-----------|
| **Stack-only usage** | No GC pressure, super-fast execution. |
| **Span-based API** | Efficient memory access, avoids extra copies. |
| **Pooled memory** | Reduces allocations, increases performance. |
| **Auto-growing buffer** | Handles dynamic size needs. |

---

## ⚠️ Important Notes

- `ImmutableArrayBuilder<T>` is a **ref struct**, meaning:
  - It **cannot be stored on the heap** (e.g., in fields, async methods, or lambda captures).
  - Always use it **inside a method** or a `using` block.
- Call `.Dispose()` **manually** if not using `using` to release resources.

---

## 🏆 Summary

🔹 **`ImmutableArrayBuilder<T>`** is an **optimized way** to **build immutable arrays efficiently** without frequent allocations.  
🔹 Uses **buffer pooling**, **aggressive inlining**, and **stack-only constraints** to minimize memory usage.  
🔹 Ensures **high-performance immutable array construction** without GC overhead.

---
🚀 Use it in **performance-critical scenarios** where immutable collections are frequently created!
