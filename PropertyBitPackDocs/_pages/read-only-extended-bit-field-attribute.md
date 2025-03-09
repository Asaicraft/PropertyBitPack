---
title: ReadOnlyExtendedBitFieldAttribute
---

`ReadOnlyExtendedBitFieldAttribute` combines the functionality of **`ReadOnlyBitFieldAttribute`** and **`ExtendedBitFieldAttribute`**. It provides **bit-packing for read-only properties** while allowing values **larger than the allocated bit size** by redirecting to an external getter.

## **What is ReadOnlyExtendedBitFieldAttribute?**
✅ **Inherits from both `ReadOnlyBitFieldAttribute` and `ExtendedBitFieldAttribute`**  
✅ **Supports all standard bit-packing features** from `BitFieldAttribute`  
✅ **Automatically generates constructors** for read-only properties  
✅ **Allows properties to store values larger than allocated bits**  
✅ **Uses an external getter when values exceed the bit limit**  
✅ **Requires `GetterLargeSizeValueName` for overflow handling**  
✅ **Supports `FieldName` for referencing existing read-only fields**  

---

## **Example: Read-Only Bit-Packed Properties with Large Values**
```csharp
public sealed partial class ReadOnlyExtendedBitFieldExample
{
    private readonly int _existingField;

    [ReadOnlyExtendedBitField(BitsCount = 5, GetterLargeSizeValueName = nameof(GetMaxValue))]
    public partial int Data1 { get; }

    [ReadOnlyExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(GetMaxValue), ConstructorAccessModifier = AccessModifier.Public)]
    public partial int Data2 { get; }

    [ReadOnlyExtendedBitField(BitsCount = 6, FieldName = nameof(_existingField), GetterLargeSizeValueName = nameof(GetMaxValue), ConstructorAccessModifier = AccessModifier.Public)]
    public partial int Data3 { get; }

    public static int GetMaxValue() => 1024; // External getter for overflow values
}
```

---

## **How It Works**
1️⃣ `BitsCount` **allocates bits for storage**, but if the value exceeds the max, the getter calls `GetterLargeSizeValueName`.  
2️⃣ `ReadOnlyExtendedBitFieldAttribute` **ensures immutability** by allowing only read-only properties.  
3️⃣ **Constructors are automatically generated**, grouping properties with the same `ConstructorAccessModifier`.  
4️⃣ `FieldName` **allows referencing existing fields**, but they must be `readonly`.  

---

## **Generated Code**
### **1️⃣ Constructor for Properties with `Private` Access (Default)**
```csharp
partial class ReadOnlyExtendedBitFieldExample
{
    private ReadOnlyExtendedBitFieldExample(int Data1)
    {
        {
            const ushort maxData1_ = (1 << 5) - 1;
            var clampedData1_ = global::System.Math.Min((ushort)(Data1), maxData1_);
            this._Data1__Data2__ = (ushort)((this._Data1__Data2__ & ~(((1 << 5) - 1) << 0)) | ((clampedData1_ & ((1 << 5) - 1)) << 0));
        }
    }
}
```

### **2️⃣ Public Constructor for Properties with `Public` Access**
```csharp
partial class ReadOnlyExtendedBitFieldExample
{
    public ReadOnlyExtendedBitFieldExample(int Data3, int Data2)
    {
        {
            const int maxData3_ = (1 << 6) - 1;
            var clampedData3_ = global::System.Math.Min((int)(Data3), maxData3_);
            this._existingField = (int)((this._existingField & ~(((1 << 6) - 1) << 0)) | ((clampedData3_ & ((1 << 6) - 1)) << 0));
        }

        {
            const ushort maxData2_ = (1 << 10) - 1;
            var clampedData2_ = global::System.Math.Min((ushort)(Data2), maxData2_);
            this._Data1__Data2__ = (ushort)((this._Data1__Data2__ & ~(((1 << 10) - 1) << 5)) | ((clampedData2_ & ((1 << 10) - 1)) << 5));
        }
    }
}
```

---

## **Common Errors**
### **1️⃣ Referencing a Non-Readonly Field**
If `FieldName` references a **non-readonly field**, an error occurs:

**PRBITS013**  
*Invalid reference to non-readonly field in 'ReadOnlyBitFieldAttribute.FieldName'.*  
The 'FieldName' for property `{0}` must reference a readonly field when using the `nameof` operation.

✅ **Fix:** Ensure the field is `readonly`.

```csharp
private readonly int _bitField; // ✅ Allowed

[ReadOnlyExtendedBitField(FieldName = nameof(_bitField), BitsCount = 8, GetterLargeSizeValueName = nameof(GetMaxValue))]
public partial int Data { get; }
```

🚫 **Invalid Code (Causes Error)**
```csharp
private int _bitField; // ❌ Not readonly

[ReadOnlyExtendedBitField(FieldName = nameof(_bitField), BitsCount = 8, GetterLargeSizeValueName = nameof(GetMaxValue))]
public partial int Data { get; } // ❌ Compiler error: PRBITS013
```

---

### **2️⃣ Property Must Be Read-Only**
If a property **has a setter**, it must be **init-only** or read-only. Otherwise, an error occurs:

**PRBITS014**  
*ReadOnlyBitFieldAttribute requires property without setter or with init-only setter.*  
The property `{0}` with `ReadOnlyBitFieldAttribute` must either be read-only or have an `init`-only setter.

🚫 **Invalid Code (Causes Error)**
```csharp
[ReadOnlyExtendedBitField(BitsCount = 6, GetterLargeSizeValueName = nameof(GetMaxValue))]
public partial int Value { get; set; } // ❌ Compiler error: PRBITS014
```

✅ **Valid Code**
```csharp
[ReadOnlyExtendedBitField(BitsCount = 6, GetterLargeSizeValueName = nameof(GetMaxValue))]
public partial int Value { get; } // ✅ Read-only property

[ReadOnlyExtendedBitField(BitsCount = 6, GetterLargeSizeValueName = nameof(GetMaxValue))]
public partial int Value2 { get; init; } // ✅ Init-only setter
```

---

## **Why Use ReadOnlyExtendedBitFieldAttribute?**
✅ **Ensures immutability** – Values **set only through constructor** or **external getter**  
✅ **Optimized for large values** – Handles numbers **beyond allocated bits**  
✅ **Automatically generates constructors** – Groups properties with same `ConstructorAccessModifier`  
✅ **Uses `FieldName` to pack into existing fields** – But they **must be `readonly`**  
✅ **Defaults to `private` constructor** unless explicitly changed  

---

## **Important Notes**
⚠ `ReadOnlyExtendedBitFieldAttribute` **inherits from both `ReadOnlyBitFieldAttribute` and `ExtendedBitFieldAttribute`**, meaning it supports **all features** of both.  
⚠ The generator **automatically creates constructors** based on `ConstructorAccessModifier`.  
⚠ **If a property exceeds its bit size, the getter redirects to `GetterLargeSizeValueName`.**  
⚠ **By default, the constructor is `private`**, unless `ConstructorAccessModifier` is specified.  
