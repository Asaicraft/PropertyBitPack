using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public static partial class BitFieldUtils
{
    public static long GetLongValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (1L << length) - 1;
        return (field >> start) & mask;
    }

    public static void SetLongValue(ref byte field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (byte)((1 << length) - 1);
        var clampedValue = value & mask;
        field &= (byte)~(mask << start);
        field |= (byte)(clampedValue << start);
    }

    public static long GetLongValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (1L << length) - 1;
        return (field >> start) & mask;
    }

    public static void SetLongValue(ref sbyte field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (sbyte)((1 << length) - 1);
        var clampedValue = value & mask;
        field &= (sbyte)~(mask << start);
        field |= (sbyte)(clampedValue << start);
    }

    public static long GetLongValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = length == 16 ? ~0L : ((1L << length) - 1);
        return (field >> start) & mask;
    }

    public static void SetLongValue(ref short field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (short)((1U << length) - 1);
        var clampedValue = value & mask;
        field &= (short)~(mask << start);
        field |= (short)(clampedValue << start);
    }

    public static long GetLongValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        return (long)((field >> start) & mask);
    }

    public static void SetLongValue(ref ushort field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (ushort)((1U << length) - 1);
        var clampedValue = value & mask;
        field &= (ushort)~(mask << start);
        field |= (ushort)(clampedValue << start);
    }

    public static long GetLongValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (length == 32) ? ~0L : ((1L << length) - 1);
        return (field >> start) & mask;
    }

    public static void SetLongValue(ref int field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1 << length) - 1;
        var clampedValue = (int)(value & mask);
        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static long GetLongValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1UL << length) - 1;
        return (long)((field >> start) & mask);
    }

    public static void SetLongValue(ref uint field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1U << length) - 1;
        var clampedValue = value & mask;
        field &= ~(mask << start);
        field |= (uint)(clampedValue << start);
    }

    public static long GetLongValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0L : ((1L << length) - 1);
        return (field >> start) & mask;
    }

    public static void SetLongValue(ref long field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0L : ((1L << length) - 1);
        var clampedValue = value & mask;
        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static long GetLongValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        return (long)((field >> start) & mask);
    }

    public static void SetLongValue(ref ulong field, Range range, long value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = (ulong)(value & (long)mask);
        field &= ~(mask << start);
        field |= (clampedValue << start);
    }
}
