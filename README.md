[![NuGet Stats](https://img.shields.io/nuget/v/PropertyBitPack.svg)](https://www.nuget.org/packages/PropertyBitPack)

# PropertyBitPack

PropertyBitPack is a Roslyn source generator that simplifies defining and managing bit-packed properties in C#. It allows developers to decorate properties with custom attributes to automatically generate efficient bit manipulation code.

## Installation

PropertyBitPack is available as a NuGet package. You can install it using the following command:
```bash
dotnet add package PropertyBitPack
```

## Usage

### Define Bit-Packed Boolean Properties

By default, boolean properties are stored using at least 1 byte of memory. To reduce memory usage, you can use the `BitFieldAttribute` to pack multiple bool properties into a single byte.

The following example demonstrates how to define bit-packed boolean properties using the `BitFieldAttribute`:

```csharp
using PropertyBitPack;

namespace SomeNamespace;

public partial class BitPackedBools
{
    [BitField]
    public partial bool Bool1 { get; set; }
	
    [BitField]
    public partial bool Bool2 { get; set; }
	
    [BitField]
    public partial bool Bool3 { get; set; }
	
    [BitField]
    public partial bool Bool4 { get; set; }
}
```

This will generate the following code:

```csharp
partial class BitPackedBools
{
    private byte _Bool1__Bool2__Bool3__Bool4__;
    public partial bool Bool1
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__ >> 0) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value ? ((this._Bool1__Bool2__Bool3__Bool4__) | (((1 << 1) - 1) << 0)) : (this._Bool1__Bool2__Bool3__Bool4__ & ~(((1 << 1) - 1) << 0)));
        }
    }

    public partial bool Bool2
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__ >> 1) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value ? ((this._Bool1__Bool2__Bool3__Bool4__) | (((1 << 1) - 1) << 1)) : (this._Bool1__Bool2__Bool3__Bool4__ & ~(((1 << 1) - 1) << 1)));
        }
    }

    public partial bool Bool3
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__ >> 2) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value ? ((this._Bool1__Bool2__Bool3__Bool4__) | (((1 << 1) - 1) << 2)) : (this._Bool1__Bool2__Bool3__Bool4__ & ~(((1 << 1) - 1) << 2)));
        }
    }

    public partial bool Bool4
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__ >> 3) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value ? ((this._Bool1__Bool2__Bool3__Bool4__) | (((1 << 1) - 1) << 3)) : (this._Bool1__Bool2__Bool3__Bool4__ & ~(((1 << 1) - 1) << 3)));
        }
    }
}
```

### Define Bit-Packed Properties with Mixed Types

The `BitFieldAttribute` can also be used to pack multiple properties of different types into a single field. The following example demonstrates how to define bit-packed properties for booleans and a byte using the `BitFieldAttribute`:

```csharp
using PropertyBitPack;

namespace SomeNamespace;

public partial class BitPackedBools
{
    [BitField]
    public partial bool Bool1 { get; set; }
	
    [BitField]
    public partial bool Bool2 { get; set; }
	
    [BitField]
    public partial bool Bool3 { get; set; }

    [BitField]
    public partial bool Bool4 { get; set; }

    [BitField]
    public partial byte Byte1 { get; set; }
}
```

This will generate the following code:

```csharp
partial class BitPackedBools
{
    private ushort _Bool1__Bool2__Bool3__Bool4__Byte1__;
    public partial bool Bool1
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__Byte1__ >> 0) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__Byte1__ = (ushort)(value ? ((this._Bool1__Bool2__Bool3__Bool4__Byte1__) | (((1 << 1) - 1) << 0)) : (this._Bool1__Bool2__Bool3__Bool4__Byte1__ & ~(((1 << 1) - 1) << 0)));
        }
    }

    public partial bool Bool2
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__Byte1__ >> 1) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__Byte1__ = (ushort)(value ? ((this._Bool1__Bool2__Bool3__Bool4__Byte1__) | (((1 << 1) - 1) << 1)) : (this._Bool1__Bool2__Bool3__Bool4__Byte1__ & ~(((1 << 1) - 1) << 1)));
        }
    }

    public partial bool Bool3
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__Byte1__ >> 2) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__Byte1__ = (ushort)(value ? ((this._Bool1__Bool2__Bool3__Bool4__Byte1__) | (((1 << 1) - 1) << 2)) : (this._Bool1__Bool2__Bool3__Bool4__Byte1__ & ~(((1 << 1) - 1) << 2)));
        }
    }

    public partial bool Bool4
    {
        get
        {
            return ((this._Bool1__Bool2__Bool3__Bool4__Byte1__ >> 3) & (1)) == 1;
        }
        set
        {
            this._Bool1__Bool2__Bool3__Bool4__Byte1__ = (ushort)(value ? ((this._Bool1__Bool2__Bool3__Bool4__Byte1__) | (((1 << 1) - 1) << 3)) : (this._Bool1__Bool2__Bool3__Bool4__Byte1__ & ~(((1 << 1) - 1) << 3)));
        }
    }

    public partial byte Byte1
    {
        get
        {
            return (byte)((this._Bool1__Bool2__Bool3__Bool4__Byte1__ >> 4) & ((1 << 8) - 1));
        }
        set
        {
            const ushort maxValue_ = (1 << 8) - 1;
            var clamped_ = global::System.Math.Min((ushort)(value), maxValue_);
            this._Bool1__Bool2__Bool3__Bool4__Byte1__ = (ushort)((this._Bool1__Bool2__Bool3__Bool4__Byte1__ & ~(((1 << 8) - 1) << 4)) | ((clamped_ & ((1 << 8) - 1)) << 4));
        }
    }
}
```

As you can see, the `BitFieldAttribute` can pack multiple properties of different types into a single field. In this example, 1 + 1 + 1 + 1 + 8 = 12 bits are packed into a single `ushort` field.

### Define Bit-Packed Properties with Custom Bit Sizes

The `BitFieldAttribute` can also be used to pack properties with custom bit sizes. The following example demonstrates how to define bit-packed properties with custom bit sizes:

```csharp
internal sealed partial record RecordExample
{
    [BitField(BitsCount = 4)]
    public partial byte Property1 { get; init; }

    [BitField(BitsCount = 11)]
    public partial int Property2 { get; init; }
}
```

This will generate the following code:

```csharp
partial record RecordExample
{
    private ushort _Property1__Property2__;
    public partial byte Property1
    {
        get
        {
            return (byte)((this._Property1__Property2__ >> 0) & ((1 << 4) - 1));
        }
        init
        {
            const ushort maxValue_ = (1 << 4) - 1;
            var clamped_ = global::System.Math.Min((ushort)(value), maxValue_);
            this._Property1__Property2__ = (ushort)((this._Property1__Property2__ & ~(((1 << 4) - 1) << 0)) | ((clamped_ & ((1 << 4) - 1)) << 0));
        }
    }

    public partial int Property2
    {
        get
        {
            return (int)((this._Property1__Property2__ >> 4) & ((1 << 11) - 1));
        }
        init
        {
            const ushort maxValue_ = (1 << 11) - 1;
            var clamped_ = global::System.Math.Min((ushort)(value), maxValue_);
            this._Property1__Property2__ = (ushort)((this._Property1__Property2__ & ~(((1 << 11) - 1) << 4)) | ((clamped_ & ((1 << 11) - 1)) << 4));
        }
    }
}
```

### Generate Custom Field Names

You can also specify a custom field name using the `FieldName` property of the `BitFieldAttribute`. The following example demonstrates how to define bit-packed properties with custom field names:

```csharp
internal sealed partial class NamedFieldExample
{
    [BitField(FieldName = "_another", BitsCount = 9)]
    public partial short Short1 { get; set; }

    [BitField(FieldName = "_another", BitsCount = 9)]
    public partial short Short2 { get; set; }
}
```

This will generate the following code:

```csharp
partial class NamedFieldExample
{
    private uint _another;
    public partial short Short1
    {
        get
        {
            return (short)((this._another >> 0) & ((1U << 9) - 1));
        }
        set
        {
            const uint maxValue_ = (1U << 9) - 1;
            var clamped_ = global::System.Math.Min((uint)(value), maxValue_);
            this._another = (uint)((this._another & ~(((1U << 9) - 1) << 0)) | ((clamped_ & ((1U << 9) - 1)) << 0));
        }
    }

    public partial short Short2
    {
        get
        {
            return (short)((this._another >> 9) & ((1U << 9) - 1));
        }
        set
        {
            const uint maxValue_ = (1U << 9) - 1;
            var clamped_ = global::System.Math.Min((uint)(value), maxValue_);
            this._another = (uint)((this._another & ~(((1U << 9) - 1) << 9)) | ((clamped_ & ((1U << 9) - 1)) << 9));
        }
    }

    public partial short Short3
    {
        get
        {
            const short maxValue_ = (1 << 9) - 1;
            short value_ = (short)((this._another >> 18) & ((1U << 9) - 1));
            if (value_ == maxValue_)
            {
                return ExtendedValue();
            }
            return value_;
        }
        set
        {
            const uint maxValue_ = (1U << 9) - 1;
            var clamped_ = global::System.Math.Min((uint)(value), maxValue_);
            this._another = (uint)((this._another & ~(((1U << 9) - 1) << 18)) | ((clamped_ & ((1U << 9) - 1)) << 18));
        }
    }
}
```

> **Note:** If you use the same field name for multiple properties, the generator will use a single field for all of them, and the maximum bit size per field is 64.  
> If you need to use more than 64 bits, either specify different field names in the attributes or omit the named fields so that the generator creates a new field when necessary.

### Define Bit-Packed Properties with Already Defined Fields

The `BitFieldAttribute` can also be used to generate properties that use already defined fields. The following example demonstrates this:

```csharp
public partial struct PackedStruct
{
    private long _packed;
    private byte _packed2;

    [BitField(FieldName = nameof(_packed))]
    public partial bool Bool1 { get; set; }

    [BitField(FieldName = nameof(_packed))]
    public partial bool Bool2 { get; set; }

    [BitField(BitsCount = 11, FieldName = nameof(_packed))]
    public partial short Short1 { get; set; }

    [BitField(BitsCount = 9, FieldName = nameof(_packed))]
    public partial short Short2 { get; set; }

    [BitField(BitsCount = 14, FieldName = nameof(_packed))]
    public partial int Int1 { get; set; }

    [BitField(BitsCount = 18, FieldName = nameof(_packed))]
    public partial int Int2 { get; set; }

    [BitField(BitsCount = 5, FieldName = nameof(_packed2))]
    public partial byte Hi { get; set; }

    [BitField(BitsCount = 2, FieldName = nameof(_packed2))]
    public partial byte Hi2 { get; set; }

    [BitField(FieldName = nameof(_packed2))]
    public partial bool HiBool { get; set; }
}
```

This will generate the following code:

```csharp
partial struct PackedStruct
{
    public partial bool Bool1
    {
        get
        {
            return ((this._packed >> 0) & (1L)) == 1L;
        }
        set
        {
            this._packed = (long)(value ? ((this._packed) | (((1L << 1) - 1) << 0)) : (this._packed & ~(((1L << 1) - 1) << 0)));
        }
    }

    public partial bool Bool2
    {
        get
        {
            return ((this._packed >> 1) & (1L)) == 1L;
        }
        set
        {
            this._packed = (long)(value ? ((this._packed) | (((1L << 1) - 1) << 1)) : (this._packed & ~(((1L << 1) - 1) << 1)));
        }
    }

    public partial short Short1
    {
        get
        {
            return (short)((this._packed >> 2) & ((1L << 11) - 1));
        }
        set
        {
            const long maxValue_ = (1L << 11) - 1;
            var clamped_ = global::System.Math.Min((long)(value), maxValue_);
            this._packed = (long)((this._packed & ~(((1L << 11) - 1) << 2)) | ((clamped_ & ((1L << 11) - 1)) << 2));
        }
    }

    public partial short Short2
    {
        get
        {
            return (short)((this._packed >> 13) & ((1L << 9) - 1));
        }
        set
        {
            const long maxValue_ = (1L << 9) - 1;
            var clamped_ = global::System.Math.Min((long)(value), maxValue_);
            this._packed = (long)((this._packed & ~(((1L << 9) - 1) << 13)) | ((clamped_ & ((1L << 9) - 1)) << 13));
        }
    }

    public partial int Int1
    {
        get
        {
            return (int)((this._packed >> 22) & ((1L << 14) - 1));
        }
        set
        {
            const long maxValue_ = (1L << 14) - 1;
            var clamped_ = global::System.Math.Min((long)(value), maxValue_);
            this._packed = (long)((this._packed & ~(((1L << 14) - 1) << 22)) | ((clamped_ & ((1L << 14) - 1)) << 22));
        }
    }

    public partial int Int2
    {
        get
        {
            return (int)((this._packed >> 36) & ((1L << 18) - 1));
        }
        set
        {
            const long maxValue_ = (1L << 18) - 1;
            var clamped_ = global::System.Math.Min((long)(value), maxValue_);
            this._packed = (long)((this._packed & ~(((1L << 18) - 1) << 36)) | ((clamped_ & ((1L << 18) - 1)) << 36));
        }
    }
}
```

### ExtendedBitFieldAttribute

`ExtendedBitFieldAttribute` provides functionality similar to `BitFieldAttribute`. Sometimes your data is larger than the allowed bit size; in this case you can use `ExtendedBitFieldAttribute` to store values that exceed the specified bit size.

The following example demonstrates how to define bit-packed properties with custom bit sizes using the `ExtendedBitFieldAttribute`:

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

If any property value exceeds the maximum (2^BitsCount = 2047), the getter method specified by `GetterLargeSizeValueName` will be called.

This will generate the following code:

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
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (ulong)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 0)) | ((clamped_ & ((1UL << 11) - 1)) << 0));
        }
    }

    public partial ushort SlotCount2
    {
        get
        {
            return (ushort)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ >> 11) & ((1UL << 11) - 1));
        }
        internal set
        {
            const ulong maxValue_ = (1UL << 11) - 1;
            var clamped_ = global::System.Math.Min((ulong)(value), maxValue_);
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (ulong)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 11)) | ((clamped_ & ((1UL << 11) - 1)) << 11));
        }
    }

    public partial int SlotCount3
    {
        get
        {
            const int maxValue_ = (1 << 11) - 1;
            int value_ = (int)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ >> 22) & ((1UL << 11) - 1));
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
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (ulong)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 22)) | ((clamped_ & ((1UL << 11) - 1)) << 22));
        }
    }

    public partial int SlotCount4
    {
        get
        {
            const int maxValue_ = (1 << 11) - 1;
            int value_ = (int)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ >> 33) & ((1UL << 11) - 1));
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
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (ulong)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 33)) | ((clamped_ & ((1UL << 11) - 1)) << 33));
        }
    }

    public partial int SlotCount5
    {
        get
        {
            const int maxValue_ = (1 << 11) - 1;
            int value_ = (int)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ >> 44) & ((1UL << 11) - 1));
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
            this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ = (ulong)((this._SlotCount__SlotCount2__SlotCount3__SlotCount4__SlotCount5__ & ~(((1UL << 11) - 1) << 44)) | ((clamped_ & ((1UL << 11) - 1)) << 44));
        }
    }
}
```

**How It Works:**

```csharp
var extendedBitField = new ExtendedBitFieldExample
{
    SlotCount = 15683, // The resulting value is calculated as: 123 * 6 + 2^11 = 738 + 2048 = 2786. Since the input value is greater than 2047, the extended value (e.g., 12345) will be used.
};

Assert.Equal(2786, extendedBitField.SlotCount);
```

### Define Read-Only Bit-Packed Properties

For read-only properties, you can use the `init` accessor and reference a defined readonly field using the `nameof` operator. The following example demonstrates how to define read-only bit-packed properties with custom bit sizes using the `BitFieldAttribute`:

```csharp
public partial class SimpleReadOnlyExample
{
    private readonly int _packed;

    [BitField(BitsCount = 22, FieldName = nameof(_packed))]
    public partial int Int1 { get; init; }

    [BitField(BitsCount = 9, FieldName = nameof(_packed))]
    public partial short Short1 { get; init; }

    [BitField(BitsCount = 1, FieldName = nameof(_packed))]
    public partial bool Bool1 { get; init; }
}
```

This will generate the following code:

```csharp
partial class SimpleReadOnlyExample
{
    public partial int Int1
    {
        get
        {
            return (int)((this._packed >> 0) & ((1 << 22) - 1));
        }
        init
        {
            const int maxValue_ = (1 << 22) - 1;
            var clamped_ = global::System.Math.Min((int)(value), maxValue_);
            this._packed = (int)((this._packed & ~(((1 << 22) - 1) << 0)) | ((clamped_ & ((1 << 22) - 1)) << 0));
        }
    }

    public partial short Short1
    {
        get
        {
            return (short)((this._packed >> 22) & ((1 << 9) - 1));
        }
        init
        {
            const int maxValue_ = (1 << 9) - 1;
            var clamped_ = global::System.Math.Min((int)(value), maxValue_);
            this._packed = (int)((this._packed & ~(((1 << 9) - 1) << 22)) | ((clamped_ & ((1 << 9) - 1)) << 22));
        }
    }

    public partial bool Bool1
    {
        get
        {
            return ((this._packed >> 31) & (1)) == 1;
        }
        init
        {
            this._packed = (int)(value ? ((this._packed) | (((1 << 1) - 1) << 31)) : (this._packed & ~(((1 << 1) - 1) << 31)));
        }
    }
}
```

Alternatively, you can use the `ReadOnlyBitFieldAttribute`, which provides the same functionality as `BitFieldAttribute` but for read-only properties. This attribute also generates a constructor:

```csharp
public partial class SimpleReadOnlyExample
{
    [ReadOnlyBitField(BitsCount = 22)]
    public partial int Int1 { get; init; }

    [ReadOnlyBitField(BitsCount = 9)]
    public partial short Short1 { get; init; }

    [ReadOnlyBitField(BitsCount = 1)]
    public partial bool Bool1 { get; init; }
}
```

This will generate the following code:

```csharp
partial class SimpleReadOnlyExample
{
    private SimpleReadOnlyExample(int Int1, short Short1, bool Bool1)
    {
        {
            const uint maxInt1_ = (1U << 22) - 1;
            var clampedInt1_ = global::System.Math.Min((uint)(Int1), maxInt1_);
            this._Int1__Short1__Bool1__ = (uint)((this._Int1__Short1__Bool1__ & ~(((1U << 22) - 1) << 0)) | ((clampedInt1_ & ((1U << 22) - 1)) << 0));
        }
        {
            const uint maxShort1_ = (1U << 9) - 1;
            var clampedShort1_ = global::System.Math.Min((uint)(Short1), maxShort1_);
            this._Int1__Short1__Bool1__ = (uint)((this._Int1__Short1__Bool1__ & ~(((1U << 9) - 1) << 22)) | ((clampedShort1_ & ((1U << 9) - 1)) << 22));
        }
        {
            this._Int1__Short1__Bool1__ = (uint)(Bool1 ? ((this._Int1__Short1__Bool1__) | (((1U << 1) - 1) << 31)) : (this._Int1__Short1__Bool1__ & ~(((1U << 1) - 1) << 31)));
        }
    }
}

partial class SimpleReadOnlyExample
{
    private readonly uint _Int1__Short1__Bool1__;
    public partial int Int1
    {
        get
        {
            return (int)((this._Int1__Short1__Bool1__ >> 0) & ((1U << 22) - 1));
        }
        init
        {
            const uint maxValue_ = (1U << 22) - 1;
            var clamped_ = global::System.Math.Min((uint)(value), maxValue_);
            this._Int1__Short1__Bool1__ = (uint)((this._Int1__Short1__Bool1__ & ~(((1U << 22) - 1) << 0)) | ((clamped_ & ((1U << 22) - 1)) << 0));
        }
    }

    public partial short Short1
    {
        get
        {
            return (short)((this._Int1__Short1__Bool1__ >> 22) & ((1U << 9) - 1));
        }
        init
        {
            const uint maxValue_ = (1U << 9) - 1;
            var clamped_ = global::System.Math.Min((uint)(value), maxValue_);
            this._Int1__Short1__Bool1__ = (uint)((this._Int1__Short1__Bool1__ & ~(((1U << 9) - 1) << 22)) | ((clamped_ & ((1U << 9) - 1)) << 22));
        }
    }

    public partial bool Bool1
    {
        get
        {
            return ((this._Int1__Short1__Bool1__ >> 31) & (1U)) == 1U;
        }
        init
        {
            this._Int1__Short1__Bool1__ = (uint)(value ? ((this._Int1__Short1__Bool1__) | (((1U << 1) - 1) << 31)) : (this._Int1__Short1__Bool1__ & ~(((1U << 1) - 1) << 31)));
        }
    }
}
```

> **Note:** The generated constructor is private by default, but you can change it to public using the `ConstructorAccessModifier` parameter. For example:
>
> ```csharp
> public partial class SimpleReadOnlyExample
> {
>     [ReadOnlyBitField(BitsCount = 22, ConstructorAccessModifier = AccessModifier.Public)]
>     public partial int Int1 { get; }
>
>     [ReadOnlyBitField(BitsCount = 9)]
>     public partial short Short1 { get; }
>
>     [ReadOnlyBitField(BitsCount = 1)]
>     public partial bool Bool1 { get; }
> }
> ```
>
> In this case, properties with the same `ConstructorAccessModifier` will be grouped into the same constructor.
>
> Also, if you prefer not to use the `init` accessor, you can use `ReadOnlyBitFieldAttribute` to generate read-only properties where the constructor sets values directly to the fields.

### ReadOnlyExtendedBitField

`ReadOnlyExtendedBitFieldAttribute` provides functionality similar to `ReadOnlyBitFieldAttribute`, but for extended bit field scenarios.
