using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;
public static partial class BitFieldUtils
{
    public static byte GetByteValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8); 

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1); 

        return (byte)((field >> start) & mask); 
    }

    public static void SetByteValue(ref byte field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 8); 

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1); 
        var clampedValue = (byte)(value & mask); 

        field &= (byte)~(mask << start);

        field |= (byte)(clampedValue << start);
    }

    public static byte GetByteValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref sbyte field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 8);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);
        var clampedValue = (byte)(value & mask);

        field &= (sbyte)~(mask << start);
        field |= (sbyte)(clampedValue << start);
    }

    public static byte GetByteValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref short field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);
        var clampedValue = (byte)(value & mask);

        field &= (short)~(mask << start);
        field |= (short)(clampedValue << start);
    }

    public static byte GetByteValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref ushort field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);
        var clampedValue = (byte)(value & mask);

        field &= (ushort)~(mask << start);
        field |= (ushort)(clampedValue << start);
    }

    public static byte GetByteValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref int field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);
        var clampedValue = (byte)(value & mask);

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static byte GetByteValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1U << length) - 1; 

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref uint field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1U << length) - 1; 
        var clampedValue = value & mask;

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static byte GetByteValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (byte)((1 << length) - 1);

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref long field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1L << length) - 1;
        var clampedValue = value & mask; 

        field &= ~(mask << start);
        field |= (clampedValue << start); 
    }

    public static byte GetByteValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = ((1UL << length) - 1); 

        return (byte)((field >> start) & mask);
    }

    public static void SetByteValue(ref ulong field, Range range, byte value)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1UL << length) - 1; 
        var clampedValue = value & mask;

        field &= ~(mask << start); 
        field |= (clampedValue << start);
    }
}
