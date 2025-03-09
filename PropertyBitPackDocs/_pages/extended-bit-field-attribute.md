---
title: ExtendedBitFieldAttribute
---

`ExtendedBitFieldAttribute` extends the functionality of `BitFieldAttribute`. It provides the same features as `BitFieldAttribute` but **adds support for handling values that exceed the allocated bit size**.

## **What is ExtendedBitFieldAttribute?**
✅ `ExtendedBitFieldAttribute` **inherits from** `BitFieldAttribute`.  
✅ Supports **bit-packing**, **custom bit sizes**, and **field name customization** like `BitFieldAttribute`.  
✅ If a value **exceeds the specified bit size**, the property getter **calls an alternative source** (method, property, or field).  
✅ The alternative source is specified using `GetterLargeSizeValueName`.

### **Example: Handling Large Values**
```csharp
public partial class ExtendedBitFieldExample
{
    public const int BitsCountSlotCountProperty = 11;

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount { get; internal set; }

    [BitField(BitsCount = BitsCountSlotCountProperty)]
    public partial ushort SlotCount2 { get; internal set; }

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount3 { get; internal set; }

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount4 { get; internal set; }

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount5 { get; internal set; }

    public static int GetLargeSlotCount(int a = 6)
    {
        return ((int)Math.Pow(2, 11)) + 123 * a;
    }
}
```

### **How Does It Work?**
1. `SlotCount` is allocated **11 bits**.
2. If `SlotCount` is **less than 2047 (2^11 - 1)**, the value is stored within the **bit-packed field**.
3. If `SlotCount` **exceeds 2047**, the getter calls `GetLargeSlotCount()`.

---

## **Generated Code**
```csharp
partial class ExtendedBitFieldExample
{
    private ulong _SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__;

    public partial int SlotCount
    {
        get
        {
            const int maxValue_ = (1 << 11) - 1;
            int value_ = (int)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ >> 0) & ((1UL << 11) - 1));
            if (value_ == maxValue_)
            {
                return GetLargeSlotCount();
            }
            return value_;
        }
        internal set
        {
            const ulong maxValue_ = (1UL << 11) - 1;
            var clamped_ = global::System.Math.Min((ulong)(value), maxValue_);
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 0)) 
                | ((clamped_ & ((1UL << 11) - 1)) << 0);
        }
    }
}
```

---

## **GetterLargeSizeValueName**
The `GetterLargeSizeValueName` parameter **specifies what should be used when the value exceeds the allocated bit size**.

### **Valid Options**
✅ **Method** (must return the same type as the property)  
✅ **Another property**  
✅ **A field**

### **Example: Using Another Property**
```csharp
public partial class AlternativeGetterExample
{
    [ExtendedBitField(BitsCount = 6, GetterLargeSizeValueName = nameof(FullValue))]
    public partial int ShortValue { get; set; }

    public int FullValue => 9999;
}
```
If `ShortValue` exceeds **2^6 = 64**, the getter **returns `FullValue` instead**.

---

## **Important Notes**
⚠ **Using `nameof()` is mandatory**. The generator **will not work** without it.  
⚠ **If `GetterLargeSizeValueName` is missing**, the generator **will produce a compilation error**.  
⚠ **The getter method must return the same type** as the bit-packed property.  

📖 **[Next: ReadOnly BitFields →](read-only-bit-field-attribute)**
