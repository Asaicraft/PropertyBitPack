---
title: Custom Bit's Size
---

# Custom Bit's Size

By default, **BitFieldAttribute** automatically determines the number of bits based on the property type. However, you can **explicitly specify the number of bits** using the `BitsCount` parameter.

## **Defining Custom Bit Sizes**

### **Example: Using Custom Bit Sizes**
```csharp
public partial class CustomBitSizeExample
{
    [BitField(BitsCount = 4)]
    public partial byte SmallNumber { get; set; }

    [BitField(BitsCount = 11)]
    public partial int LargerValue { get; set; }
}
```

### **Generated Code**
```csharp
partial class CustomBitSizeExample
{
    private ushort _SmallNumber__LargerValue__; // 4 + 11 = 15 bits (fits in ushort)

    public partial byte SmallNumber
    {
        get => (byte)((this._SmallNumber__LargerValue__ >> 0) & ((1 << 4) - 1));
        set
        {
            const ushort maxValue_ = (1 << 4) - 1;
            var clamped_ = global::System.Math.Min((ushort)value, maxValue_);
            this._SmallNumber__LargerValue__ = (ushort)((this._SmallNumber__LargerValue__ & ~(((1 << 4) - 1) << 0)) | ((clamped_ & ((1 << 4) - 1)) << 0));
        }
    }

    public partial int LargerValue
    {
        get => (int)((this._SmallNumber__LargerValue__ >> 4) & ((1 << 11) - 1));
        set
        {
            const ushort maxValue_ = (1 << 11) - 1;
            var clamped_ = global::System.Math.Min((ushort)value, maxValue_);
            this._SmallNumber__LargerValue__ = (ushort)((this._SmallNumber__LargerValue__ & ~(((1 << 11) - 1) << 4)) | ((clamped_ & ((1 << 11) - 1)) << 4));
        }
    }
}
```

---

## **Handling Large Bit Sizes**

If the total **bit size exceeds 64 bits**, the generator will **split the properties into multiple fields**.

### **Example: When Fields Exceed 64 Bits**
```csharp
public partial class MultiFieldExample
{
    [BitField] public partial bool Flag1 { get; set; }
    [BitField] public partial bool Flag2 { get; set; }
    [BitField(BitsCount = 47)] public partial long LargeInt { get; set; }
    [BitField(BitsCount = 14)] public partial short SmallValue { get; set; }
    [BitField(BitsCount = 6)] public partial byte Byte1 { get; set; }
    [BitField(BitsCount = 4)] public partial byte Byte2 { get; set; }
    [BitField(BitsCount = 7)] public partial byte Byte3 { get; set; }
    [BitField] public partial bool Flag3 { get; set; }
}
```

### **Total Bit Calculation**
```
1 + 1 + 47 + 14 + 6 + 4 + 7 + 1 = 81 bits
```
Since **64 bits is the max per field**, the generator **splits it into two fields**:
- **First field (58 bits)**: `_Flag1__Flag2__LargeInt__SmallValue__`
- **Second field (17 bits)**: `_Byte1__Byte2__Byte3__Flag3__`

### **Generated Code**
```csharp
partial class MultiFieldExample
{
    private ulong _Flag1__Flag2__LargeInt__SmallValue__; // First 58 bits, max capacity 64

    public partial bool Flag1
    {
        get => ((this._Flag1__Flag2__LargeInt__SmallValue__ >> 0) & 1UL) == 1UL;
        set => this._Flag1__Flag2__LargeInt__SmallValue__ = value 
            ? (this._Flag1__Flag2__LargeInt__SmallValue__ | (1UL << 0)) 
            : (this._Flag1__Flag2__LargeInt__SmallValue__ & ~(1UL << 0));
    }

    public partial long LargeInt
    {
        get => (long)((this._Flag1__Flag2__LargeInt__SmallValue__ >> 2) & ((1UL << 47) - 1));
        set
        {
            const ulong maxValue_ = (1UL << 47) - 1;
            var clamped_ = global::System.Math.Min((ulong)value, maxValue_);
            this._Flag1__Flag2__LargeInt__SmallValue__ = (this._Flag1__Flag2__LargeInt__SmallValue__ & ~(((1UL << 47) - 1) << 2)) 
                | ((clamped_ & ((1UL << 47) - 1)) << 2);
        }
    }
}
```

```csharp
partial class MultiFieldExample
{
    private uint _Byte1__Byte2__Byte3__Flag3__; // Second field with 17 bits, max capacity 32

    public partial byte Byte1
    {
        get => (byte)((this._Byte1__Byte2__Byte3__Flag3__ >> 0) & ((1U << 6) - 1));
        set
        {
            const uint maxValue_ = (1U << 6) - 1;
            var clamped_ = global::System.Math.Min((uint)value, maxValue_);
            this._Byte1__Byte2__Byte3__Flag3__ = (this._Byte1__Byte2__Byte3__Flag3__ & ~(((1U << 6) - 1) << 0)) | ((clamped_ & ((1U << 6) - 1)) << 0);
        }
    }

    public partial bool Flag3
    {
        get => ((this._Byte1__Byte2__Byte3__Flag3__ >> 17) & (1U)) == 1U;
        set => this._Byte1__Byte2__Byte3__Flag3__ = value 
            ? (this._Byte1__Byte2__Byte3__Flag3__ | (1U << 17)) 
            : (this._Byte1__Byte2__Byte3__Flag3__ & ~(1U << 17));
    }
}
```

---

## **Handling Overflow Errors**
🚨 **If the total bits exceed 64 bits** and the generator **cannot create a second field**, **a compilation error will occur**.

### **Example of an Error**
```csharp
public partial class OverflowExample
{
    [BitField(BitsCount = 65)]
    public partial ulong LargeData { get; set; }
}
```

### **Compiler Error**
```
Error PRBITS001: The BitsCount for property 'LargeData' must be a positive integer and cannot exceed 64.
```

✅ **To fix this, reduce the bit count or split properties manually.**

---

## **Key Features**
✔ **Specify custom bit sizes** for properties.  
✔ **Automatic field allocation** based on total bits.  
✔ **Splits properties across multiple fields** if needed.  
✔ **Compilation error (`PRBITS001`)** if exceeding 64 bits without splitting.


📖 **[Next: ReadOnly BitFields →](PropertyBitPack/extended-bit-field-attribute)**