---
title: Custom Field Name
---

# Custom Field Name

By default, `BitFieldAttribute` **automatically** assigns a private backing field to store multiple bit-packed properties. However, you can explicitly **specify a field name** to group properties into a specific field.

## **Defining a Custom Field Name**
You can assign multiple properties to the **same field** using the `FieldName` parameter:

```csharp
public partial class Example
{
    [BitField(FieldName = "_customField")]
    public partial bool Flag1 { get; set; }

    [BitField(FieldName = "_customField")]
    public partial bool Flag2 { get; set; }

    [BitField(FieldName = "_customField")]
    public partial int Value { get; set; }
}
```

### **Generated Code:**
```csharp
partial class Example
{
    private ulong _customField;

    public partial bool Flag1
    {
        get { return ((this._customField >> 0) & 1UL) == 1UL; }
        set { this._customField = (value 
            ? (this._customField | (1UL << 0)) 
            : (this._customField & ~(1UL << 0)));
        }
    }

    public partial bool Flag2
    {
        get { return ((this._customField >> 1) & 1UL) == 1UL; }
        set { this._customField = (value 
            ? (this._customField | (1UL << 1)) 
            : (this._customField & ~(1UL << 1)));
        }
    }

    public partial int Value
    {
        get { return (int)((this._customField >> 2) & ((1UL << 32) - 1)); }
        set
        {
            const ulong maxValue_ = (1UL << 32) - 1;
            var clamped_ = global::System.Math.Min((ulong)(value), maxValue_);
            this._customField = (this._customField & ~(((1UL << 32) - 1) << 2)) 
                | ((clamped_ & ((1UL << 32) - 1)) << 2);
        }
    }
}
```

---

## **Field Size Limitation**
🚨 **A single field can store a maximum of 64 bits**.  
If the total bit size exceeds **64 bits**, a **new field will NOT be automatically created**, and the generator **will produce an error**.

### **Example: Attempting to Exceed 64 Bits**
```csharp
public partial class LargeData
{
    [BitField(FieldName = "_packedData")]
    public partial bool Flag1 { get; set; }  // 1 bit

    [BitField(FieldName = "_packedData")]
    public partial bool Flag2 { get; set; }  // 1 bit

    [BitField(FieldName = "_packedData")]
    public partial int LargeValue { get; set; }  // 32 bits

    [BitField(FieldName = "_packedData")]
    public partial short Small1 { get; set; }  // 16 bits

    [BitField(FieldName = "_packedData")]
    public partial byte Byte1 { get; set; }  // 8 bits

    [BitField(FieldName = "_packedData")]
    public partial byte Byte2 { get; set; }  // 8 bits

    [BitField(FieldName = "_packedData")]
    public partial byte Byte3 { get; set; }  // 8 bits

    [BitField(FieldName = "_packedData")]
    public partial bool Flag3 { get; set; }  // 1 bit
}
```

This results in a **compiler error**:

```
Error PRBITS008: Properties with FieldName '_packedData' require 75 bits, which is more than the largest available type (64 bits).
```

✅ **To fix this, use a second field manually:**
```csharp
[BitField(FieldName = "_packedData_1")]
public partial byte Byte3 { get; set; }

[BitField(FieldName = "_packedData_1")]
public partial bool Flag3 { get; set; }
```

---

## **Key Features**
✔ **Manually assign properties to a field** using `FieldName`.  
✔ **Combine multiple properties** into the same storage.  
✔ ❌ **No automatic field splitting beyond 64 bits** → You must manually create a second field.  

📖 **[Next: Reference to Existing Field →](PropertyBitPack/bit-field-attribute-reference-to-existing-field)**