---
title: ReadOnlyBitFieldAttribute
---

`ReadOnlyBitFieldAttribute` extends the functionality of `BitFieldAttribute`. It provides the **same bit-packing features** while also **generating constructors** for initializing read-only bit-packed properties.

## **What is ReadOnlyBitFieldAttribute?**
✅ **Inherits from `BitFieldAttribute`**, meaning it supports all standard bit-packing features  
✅ **Generates constructors** automatically for read-only properties  
✅ **Requires either a read-only getter or an `init` accessor**  
✅ **Supports `FieldName` to reference an existing field**, but it **must be `readonly`**  
✅ **Groups constructors based on `ConstructorAccessModifier`**, allowing multiple constructors  
✅ **By default, the constructor is `private`** unless explicitly changed using `ConstructorAccessModifier`

---

## **Example: Read-Only Bit-Packed Properties**
```csharp
public sealed partial class ReadOnlyBitFieldExample
{
    private readonly byte _existing;

    [ReadOnlyBitField] // Default constructor is private
    public partial bool Flag1 { get; }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial bool Flag2 { get; }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial bool Flag3 { get; }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData { get; }

    [ReadOnlyBitField(BitsCount = 15, FieldName = "_bitField", ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData2 { get; }

    [ReadOnlyBitField(BitsCount = 1, FieldName = "_bitField", ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData3 { get; }

    [ReadOnlyBitField(BitsCount = 4, FieldName = nameof(_existing), ConstructorAccessModifier = AccessModifier.Public)]
    public partial byte AdditionalData4 { get; }
}
```

---

## **Constructor Generation Based on `ConstructorAccessModifier`**
The attribute groups properties **by access modifier** when generating constructors.

### **Default (Private) Constructor**
If no `ConstructorAccessModifier` is specified, the constructor is **private**.

```csharp
partial class ReadOnlyBitFieldExample
    {
        private ReadOnlyBitFieldExample(bool Flag1)
        {
            {
                this._Flag1__Flag2__Flag3__AdditionalData__ = (ulong)(Flag1 ? ((this._Flag1__Flag2__Flag3__AdditionalData__) | (((1UL << 1) - 1) << 0)) : (this._Flag1__Flag2__Flag3__AdditionalData__ & ~(((1UL << 1) - 1) << 0)));
            }
        }
    }
```

### **Public Constructor for Properties with `Public` Access**
Properties with the same `ConstructorAccessModifier` **are grouped into the same constructor**.

```csharp
partial class ReadOnlyBitFieldExample
{
    public ReadOnlyBitFieldExample(sbyte AdditionalData4, bool Flag2, bool Flag3, int AdditionalData, int AdditionalData2, int AdditionalData3)
    {
        {
            const byte maxAdditionalData4_ = (1 << 4) - 1;
            var clampedAdditionalData4_ = global::System.Math.Min((byte)(AdditionalData4), maxAdditionalData4_);
            this._existing = (byte)((this._existing & ~(((1 << 4) - 1) << 0)) | ((clampedAdditionalData4_ & ((1 << 4) - 1)) << 0));
        }

        {
            this._Flag1__Flag2__Flag3__AdditionalData__ = (ulong)(Flag2 ? ((this._Flag1__Flag2__Flag3__AdditionalData__) | (((1UL << 1) - 1) << 1)) : (this._Flag1__Flag2__Flag3__AdditionalData__ & ~(((1UL << 1) - 1) << 1)));
        }

        {
            this._Flag1__Flag2__Flag3__AdditionalData__ = (ulong)(Flag3 ? ((this._Flag1__Flag2__Flag3__AdditionalData__) | (((1UL << 1) - 1) << 2)) : (this._Flag1__Flag2__Flag3__AdditionalData__ & ~(((1UL << 1) - 1) << 2)));
        }

        {
            const ulong maxAdditionalData_ = (1UL << 32) - 1;
            var clampedAdditionalData_ = global::System.Math.Min((ulong)(AdditionalData), maxAdditionalData_);
            this._Flag1__Flag2__Flag3__AdditionalData__ = (ulong)((this._Flag1__Flag2__Flag3__AdditionalData__ & ~(((1UL << 32) - 1) << 3)) | ((clampedAdditionalData_ & ((1UL << 32) - 1)) << 3));
        }

        {
            const ushort maxAdditionalData2_ = (1 << 15) - 1;
            var clampedAdditionalData2_ = global::System.Math.Min((ushort)(AdditionalData2), maxAdditionalData2_);
            this._bitField = (ushort)((this._bitField & ~(((1 << 15) - 1) << 0)) | ((clampedAdditionalData2_ & ((1 << 15) - 1)) << 0));
        }

        {
            const ushort maxAdditionalData3_ = (1 << 1) - 1;
            var clampedAdditionalData3_ = global::System.Math.Min((ushort)(AdditionalData3), maxAdditionalData3_);
            this._bitField = (ushort)((this._bitField & ~(((1 << 1) - 1) << 15)) | ((clampedAdditionalData3_ & ((1 << 1) - 1)) << 15));
        }
    }
}
```

---

## **Common Errors**
### **1️⃣ Referencing a Non-Readonly Field**
If `FieldName` references a **non-readonly field**, the generator **throws an error**:

**PRBITS013**  
*Invalid reference to non-readonly field in 'ReadOnlyBitFieldAttribute.FieldName'.*  
The 'FieldName' for property `{0}` must reference a readonly field when using the `nameof` operation.

✅ **Fix:** Ensure the field is declared as `readonly`.

```csharp
private readonly int _bitField; // ✅ Allowed

[ReadOnlyBitField(FieldName = nameof(_bitField))]
public partial int Data { get; }
```

🚫 **Invalid Code (Causes Error)**
```csharp
private int _bitField; // ❌ Not readonly

[ReadOnlyBitField(FieldName = nameof(_bitField))]
public partial int Data { get; } // ❌ Compiler error: PRBITS013
```

---

### **2️⃣ Property Must Be Read-Only or Have `init`**
If a property **uses a setter**, it must be **init-only**. Otherwise, an error occurs:

**PRBITS014**  
*ReadOnlyBitFieldAttribute requires property without setter or with init-only setter.*  
The property `{0}` with `ReadOnlyBitFieldAttribute` must either be read-only or have an `init`-only setter.

🚫 **Invalid Code (Causes Error)**
```csharp
[ReadOnlyBitField]
public partial int Value { get; set; } // ❌ Compiler error: PRBITS014
```

✅ **Valid Code**
```csharp
[ReadOnlyBitField]
public partial int Value { get; } // ✅ Read-only property

[ReadOnlyBitField]
public partial int Value2 { get; init; } // ✅ Init-only setter
```

---

## **Why Use ReadOnlyBitFieldAttribute?**
✅ **Ensures immutability** – Values are **set only through the constructor**  
✅ **Optimized for performance** – Memory-efficient bit-packing with **zero runtime overhead**  
✅ **Supports field customization** – Use `FieldName` to pack properties into an existing field  
✅ **Automatically generates constructors** – Properties with the same `ConstructorAccessModifier` are grouped  
✅ **Defaults to `private` constructors unless explicitly specified**

---

## **Important Notes**
⚠ `ReadOnlyBitFieldAttribute` **inherits from `BitFieldAttribute`**, so it supports **all the same features**.  
⚠ The generator **automatically creates constructors** based on `ConstructorAccessModifier`.  
⚠ If a property **exceeds the allocated bit size**, compilation will **fail with an error**.  
⚠ **By default, the constructor is `private`**. Use `ConstructorAccessModifier` to change visibility.

📖 **[Next: ReadOnlyExtendedBitField →](read-only-extended-bit-field-attribute)**
