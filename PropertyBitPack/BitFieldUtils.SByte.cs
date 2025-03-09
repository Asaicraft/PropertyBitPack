using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public static partial class BitFieldUtils
{
    public static sbyte GetSByteValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref byte field, Range range, sbyte value)
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

    public static sbyte GetSByteValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref sbyte field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 8);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);
        var clampedValue = (sbyte)(value & mask);

        field &= (sbyte)~(mask << start);
        field |= (sbyte)(clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref short field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (short)((1 << length) - 1);
        var clampedValue = (short)(value & mask);

        field &= (short)~(mask << start);
        field |= (short)(clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref ushort field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 16);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (ushort)((1 << length) - 1);
        var clampedValue = (ushort)(value & mask);

        field &= (ushort)~(mask << start);
        field |= (ushort)(clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1 << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref int field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1 << length) - 1;
        var clampedValue = (value & mask);

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1U << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref uint field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 32);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1U << length) - 1;
        var clampedValue = (uint)(value & mask);

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (sbyte)((1L << length) - 1);

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref long field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = ((1L << length) - 1);
        var clampedValue = (value & mask);

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static sbyte GetSByteValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1UL << length) - 1;

        return (sbyte)((field >> start) & mask);
    }

    public static void SetSByteValue(ref ulong field, Range range, sbyte value)
    {
        (var start, var length) = GetRangeBounds(range, 64);

        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }

        var mask = (1UL << length) - 1;
        var clampedValue = ((ulong)value & mask);

        field &= ~(mask << start);
        field |= (clampedValue << start);
    }
}
