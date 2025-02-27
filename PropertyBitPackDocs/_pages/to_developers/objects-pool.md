---
title: Objects Pool
---

## Overview

`PropertyBitPack` uses **object pooling** to avoid **excessive memory allocations** for frequently reused data structures. These **pools** hold onto lists, dictionaries, string builders, etc., returning them to a shared pool once they’re no longer needed. In high-volume or repetitive code paths, this significantly cuts down on **GC pressure**.

### Why Object Pooling?

- **Performance**: Reduces GC (Garbage Collection) overhead by reusing objects.
- **Memory Efficiency**: Cuts down on new allocations for short-lived or medium-lived objects.
- **Thread-Safety**: Pools often rely on **thread-safe** collections (e.g., `ConcurrentStack`).

---

## Key Pool Classes

1. **`StringBuildersPool`**  
   - Reuses `StringBuilder` instances.  
   - Useful for **string concatenation** or generating dynamic names in a tight loop.  
   - **Example** usage:
     ```csharp
     using var sbRent = StringBuildersPool.Rent();
     var sb = sbRent.StringBuilder;
     sb.Append("Hello, world!");

     // Once out of scope, the 'sbRent' automatically returns the StringBuilder to the pool.
     ```

2. **`OwnerFieldNameGroupDictionaryPool`**  
   - Provides dictionaries of type `Dictionary<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>>`.  
   - Groups properties by `(Owner, FieldName?)` in aggregator logic.

3. **`ListsPool<T>`**  
   - Returns and reuses `List<T>` objects.  
   - **Clears** the list when returning it to ensure no leftover items.  
   - Example usage:
     ```csharp
     var list = ListsPool<MyType>.Rent();
     // ... work with list ...
     ListsPool<MyType>.Return(list); // Clears and reuses next time
     ```

4. **`DictionariesSymbolPool<TValue>`**  
   - Provides dictionaries keyed by **`ISymbol`** (`SymbolEqualityComparer.Default`).  
   - For quick lookups in Roslyn-based logic (where symbols must be equal by reference or actual symbol identity).

5. **`DictionariesPool<TKey, TValue>`**  
   - Generic pool for `Dictionary<TKey, TValue>`.  
   - Similar to `ListsPool<T>`, but for dictionaries.

6. **`SymbolAccessPairDictionaryPool`**  
   - Specialized pool for `Dictionary<SymbolAccessPair, FieldsPropertiesPair>` with `SymbolAccessPairComparer.Default`.

7. **`SimpleLinkedListsPool`**  
   - Returns `SimpleLinkedList<T>` objects (a custom linked list implementation).  
   - Often used for chaining property or aggregator data in a memory-efficient way.

---

## How It Works

1. **Initialization**: Each pool typically **pushes** a few pre-allocated objects onto a `ConcurrentStack` in a static constructor.
2. **Rent**:  
   - **Pop** an instance from the stack if available, or create a new one if the stack is empty.
3. **Use**:  
   - Perform operations on the rented object (e.g., append, add items).
4. **Return**:  
   - Clear or reset the object’s state.  
   - **Push** it back onto the stack for future reuse.  
   - **Guard**: Some pools limit the total items to avoid hoarding (e.g., “if `(pool.Count > 4) return;`”).

---

## Example: `StringBuildersPool`

```csharp
public static class StringBuildersPool
{
    private static readonly ConcurrentStack<StringBuilder> _stringBuilders = new();

    static StringBuildersPool()
    {
        // Pre-allocate 3 StringBuilders
        for (var i = 0; i < 3; i++)
        {
            _stringBuilders.Push(new());
        }
    }

    public static RentedStringBuilderPool Rent()
    {
        return new RentedStringBuilderPool(RentInternal());
    }

    private static StringBuilder RentInternal()
    {
        return _stringBuilders.TryPop(out var stringBuilder) ? stringBuilder : new();
    }

    public static void Return(StringBuilder stringBuilder)
    {
        if (stringBuilder == null) return;
        // Limit to 4 stored builders
        if (_stringBuilders.Count > 4) return;

        stringBuilder.Clear();
        _stringBuilders.Push(stringBuilder);
    }

    public readonly ref struct RentedStringBuilderPool(StringBuilder stringBuilder)
    {
        private readonly StringBuilder _stringBuilder = stringBuilder;
        public StringBuilder StringBuilder => _stringBuilder;

        public void Dispose()
        {
            Return(_stringBuilder);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
```

### Usage Pattern

```csharp
using var sbRent = StringBuildersPool.Rent();
var sb = sbRent.StringBuilder;
sb.AppendLine("hello world!");
// automatically returned to the pool on dispose
```

**Note**: The pool stores up to **4** `StringBuilder` instances at once. If the pool is “full,” extra objects aren’t returned.

---

## Guidelines

1. **Reset State** Before returning, always **clear** or **reset** the container (lists, dictionaries, string builders).
2. **Thread-Safe** Each pool uses a `ConcurrentStack` (or some concurrency-safe approach).  
3. **Limit the Pool Size** to prevent hoarding large numbers of unused objects. Typically 3–5 is enough for small to medium usage.  
4. **Use in Short Scopes**. The pattern usually involves using `using var ...` or manually disposing to ensure timely return.

---

## Summary

**Object pools** in `PropertyBitPack` significantly **optimize** performance by reusing data structures in the Roslyn source generation pipeline. This design pattern helps:

- **Minimize GC overhead**,  
- **Prevent frequent allocations**,  
- Keep the generation logic **fast** and **efficient**.

By following the **rent/return** protocol and clearing objects before returning them, the code ensures safe, repeatable usage. Adhering to pool size limits avoids unbounded memory growth, keeping memory usage balanced.
