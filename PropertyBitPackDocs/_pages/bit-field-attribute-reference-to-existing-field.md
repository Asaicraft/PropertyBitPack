---
title: Reference to Existing Field
---

Instead of allowing `BitFieldAttribute` to automatically generate a backing field, you can **manually reference an existing field** by specifying its name using `nameof`. This is useful when working with predefined data structures or when controlling memory layout.

## **Referencing an Existing Field**

You can explicitly tell `BitFieldAttribute` to store multiple properties in the same **manually defined field**.

### **Example: Using an Existing Field**
```csharp
public partial struct PackedStruct
{
    private long _packed; // Manually defined field

    [BitField(FieldName = nameof(_packed))]
    public partial bool Flag1 { get; set; }

    [BitField(FieldName = nameof(_packed))]
    public partial bool Flag2 { get; set; }

    [BitField(FieldName = nameof(_packed))]
    public partial int Value { get; set; }
}
```

### **Generated Code**
```csharp
partial struct PackedStruct
{
    public partial bool Flag1
    {
        get => ((this._packed >> 0) & 1L) == 1L;
        set => this._packed = value 
            ? (this._packed | (1L << 0)) 
            : (this._packed & ~(1L << 0));
    }

    public partial bool Flag2
    {
        get => ((this._packed >> 1) & 1L) == 1L;
        set => this._packed = value 
            ? (this._packed | (1L << 1)) 
            : (this._packed & ~(1L << 1));
    }

    public partial int Value
    {
        get => (int)((this._packed >> 2) & ((1L << 32) - 1));
        set
        {
            const long maxValue_ = (1L << 32) - 1;
            var clamped_ = global::System.Math.Min((long)value, maxValue_);
            this._packed = (this._packed & ~(((1L << 32) - 1) << 2)) 
                | ((clamped_ & ((1L << 32) - 1)) << 2);
        }
    }
}
```

## **Field Size Limitation**
🚨 **The referenced field cannot store more bits than its type allows.**  
For example, a `long` (64 bits) can hold **at most 64 bits**.  
If you exceed this, **a compilation error will occur.**

### **Exceeding Field Size**
```csharp
public partial struct OverflowExample
{
    private int _data; // 32-bit storage

    [BitField(FieldName = nameof(_data))]
    public partial bool Bool1 { get; set; }  // 1 bit ✅

    [BitField(FieldName = nameof(_data))]
    public partial int Int1 { get; set; }    // 32 bits ✅

    [BitField(FieldName = nameof(_data))]
    public partial bool Bool2 { get; set; }  // 1 bit ❌ ERROR (Total: 34 bits)
}
```

### **Compiler Error**
```
Error PRBITS012: The field '_data' requires '34' bits, which exceeds the capacity of type 'int' that can hold a maximum of '32' bits.
```

✅ **To fix this, use a `long` instead of an `int`:**
```csharp
private long _data; // Now supports 64 bits
```

---

## **Key Features**
✔ **Reference existing fields** using `FieldName = nameof(SomeField)`.  
✔ **Avoid unnecessary extra fields** for better memory control.  
✔ **Ensure total bit count stays within field size limits**.  
✔ ❌ **Exceeding the field’s capacity results in a compiler error (`PRBITS012`).**

📖 **[Next: Custom Bits Size →](bit-field-attribute-custom-bits-size)**
