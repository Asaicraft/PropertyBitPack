using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;
public static partial class BitFieldUtils
{
    public static ulong GetULongValue(ref readonly byte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (length == 8) ? ~0UL : ((1UL << length) - 1);
        return (((ulong)field) >> start) & mask;
    }

    public static void SetULongValue(ref byte field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (length == 8) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (byte)(((ulong)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly sbyte field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (length == 8) ? ~0UL : ((1UL << length) - 1);
        // Treat sbyte as unsigned 8-bit value.
        return (((ulong)(byte)field) >> start) & mask;
    }

    public static void SetULongValue(ref sbyte field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 8);
        if (length <= 0 || length > 8)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 8 bits.");
        }
        var mask = (length == 8) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        // Update sbyte by treating it as unsigned.
        field = (sbyte)(((ulong)(byte)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly short field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (length == 16) ? ~0UL : ((1UL << length) - 1);
        // Treat short as unsigned 16-bit.
        return (((ulong)(ushort)field) >> start) & mask;
    }

    public static void SetULongValue(ref short field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (length == 16) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (short)(((ulong)(ushort)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly ushort field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (length == 16) ? ~0UL : ((1UL << length) - 1);
        return (((ulong)field) >> start) & mask;
    }

    public static void SetULongValue(ref ushort field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 16);
        if (length <= 0 || length > 16)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 16 bits.");
        }
        var mask = (length == 16) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (ushort)(((ulong)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly int field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (length == 32) ? ~0UL : ((1UL << length) - 1);
        // Treat int as unsigned 32-bit.
        return (((ulong)(uint)field) >> start) & mask;
    }

    public static void SetULongValue(ref int field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (length == 32) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (int)(((ulong)(uint)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly uint field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (length == 32) ? ~0UL : ((1UL << length) - 1);
        return (((ulong)field) >> start) & mask;
    }

    public static void SetULongValue(ref uint field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 32);
        if (length <= 0 || length > 32)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 32 bits.");
        }
        var mask = (length == 32) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (uint)(((ulong)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly long field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        return (((ulong)field) >> start) & mask;
    }

    public static void SetULongValue(ref long field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (long)(((ulong)field & ~(mask << start)) | (clampedValue << start));
    }

    public static ulong GetULongValue(ref readonly ulong field, Range range)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        return (field >> start) & mask;
    }

    public static void SetULongValue(ref ulong field, Range range, ulong value)
    {
        (var start, var length) = GetRangeBounds(range, 64);
        if (length <= 0 || length > 64)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "The bit range must be between 1 and 64 bits.");
        }
        var mask = (length == 64) ? ~0UL : ((1UL << length) - 1);
        var clampedValue = value & mask;
        field = (field & ~(mask << start)) | (clampedValue << start);
    }
}