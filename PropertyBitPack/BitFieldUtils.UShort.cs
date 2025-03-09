using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public static partial class BitFieldUtils
{
    public static ushort GetUShortValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref byte field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (byte)((1 << length) - 1);
        var clampedValue = (ushort)(value & mask);
        field &= (byte)~(mask << start);
        field |= (byte)(clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (ushort)((1 << length) - 1);
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref sbyte field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (sbyte)((1 << length) - 1);
        var clampedValue = (ushort)(value & (ushort)mask);
        field &= (sbyte)~(mask << start);
        field |= (sbyte)(clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref short field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (short)((1U << length) - 1);
        var clampedValue = (ushort)(value & mask);
        field &= (short)~(mask << start);
        field |= (short)(clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref ushort field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        var clampedValue = (ushort)(value & mask);
        field &= (ushort)~(mask << start);
        field |= (ushort)(clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref int field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1 << length) - 1;
        var clampedValue = (ushort)(value & mask);
        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1U << length) - 1;
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref uint field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1U << length) - 1;
        var clampedValue = (ushort)(value & mask);
        field &= ~(mask << start);
        field |= (uint)(clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1L << length) - 1;
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref long field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1L << length) - 1;
        var clampedValue = (ushort)(value & mask);
        field &= ~(mask << start);
        field |= ((long)clampedValue << start);
    }

    public static ushort GetUShortValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1UL << length) - 1;
        return (ushort)((field >> start) & mask);
    }

    public static void SetUShortValue(ref ulong field, Range range, ushort value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (1UL << length) - 1;
        var clampedValue = (ushort)(value & mask);
        field &= ~(mask << start);
        field |= ((ulong)clampedValue << start);
    }
}
