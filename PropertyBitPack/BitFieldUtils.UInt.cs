using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public static partial class BitFieldUtils
{
    public static uint GetUIntValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (uint)((1U << length) - 1);
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref byte field, Range range, uint value)
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

    public static uint GetUIntValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (uint)((1 << length) - 1);
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref sbyte field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (sbyte)((1 << length) - 1);
        var clampedValue = (uint)(value & (uint)mask);
        field &= (sbyte)~(mask << start);
        field |= (sbyte)(clampedValue << start);
    }

    public static uint GetUIntValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (uint)((1U << length) - 1);
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref short field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (short)((1U << length) - 1);
        var clampedValue = (uint)(value & mask);
        field &= (short)~(mask << start);
        field |= (short)(clampedValue << start);
    }

    public static uint GetUIntValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (uint)((1U << length) - 1);
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref ushort field, Range range, uint value)
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

    public static uint GetUIntValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (uint)((1U << length) - 1);
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref int field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1 << length) - 1;
        var clampedValue = value & (uint)mask;
        field &= ~(mask << start);
        field |= (int)(clampedValue << start);
    }

    public static uint GetUIntValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1U << length) - 1;
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref uint field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1U << length) - 1;
        var clampedValue = value & mask;
        field &= ~(mask << start);
        field |= (clampedValue << start);
    }

    public static uint GetUIntValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1L << length) - 1;
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref long field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1L << length) - 1;
        var clampedValue = value & (uint)mask;
        field &= ~(mask << start);
        field |= ((long)clampedValue << start);
    }

    public static uint GetUIntValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1UL << length) - 1;
        return (uint)((field >> start) & mask);
    }

    public static void SetUIntValue(ref ulong field, Range range, uint value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (1UL << length) - 1;
        var clampedValue = value & (uint)mask;
        field &= ~(mask << start);
        field |= ((ulong)clampedValue << start);
    }
}