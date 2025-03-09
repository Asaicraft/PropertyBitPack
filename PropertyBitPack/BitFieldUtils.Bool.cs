using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

public static partial class BitFieldUtils
{
    public static bool GetBoolValue(ref readonly byte field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 7)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 7.");
        }

        return (field & (1 << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref byte field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 7)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 7.");
        }

        if (value)
        {
            field = (byte)(field | (1 << bitIndex));
        }
        else
        {
            field = (byte)(field & ~(1 << bitIndex));
        }
    }


    public static bool GetBoolValue(ref readonly sbyte field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 7)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 7.");
        }

        return (field & (1 << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref sbyte field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 7)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 7.");
        }

        var mask = (byte)(1 << bitIndex); 

        if (value)
        {
            field = (sbyte)(field | (sbyte)mask); 
        }
        else
        {
            field = (sbyte)(field & (sbyte)~mask);
        }
    }

    public static bool GetBoolValue(ref readonly short field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 15)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 15.");
        }

        return (field & (1 << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref short field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 15)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 15.");
        }

        if (value)
        {
            field |= (short)(1 << bitIndex);
        }
        else
        {
            field &= (short)~(1 << bitIndex);
        }
    }

    public static bool GetBoolValue(ref readonly ushort field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 15)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 15.");
        }

        return (field & (1 << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref ushort field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 15)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 15.");
        }

        if (value)
        {
            field |= (ushort)(1 << bitIndex);
        }
        else
        {
            field &= (ushort)~(1 << bitIndex);
        }
    }

    public static bool GetBoolValue(ref readonly int field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 31)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 31.");
        }

        return (field & (1 << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref int field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 31)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 31.");
        }

        if (value)
        {
            field |= (1 << bitIndex);
        }
        else
        {
            field &= ~(1 << bitIndex);
        }
    }

    public static bool GetBoolValue(ref readonly uint field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 31)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 31.");
        }

        return (field & (1U << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref uint field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 31)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 31.");
        }

        if (value)
        {
            field |= (1U << bitIndex);
        }
        else
        {
            field &= ~(1U << bitIndex);
        }
    }

    public static bool GetBoolValue(ref readonly long field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 63)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 63.");
        }

        return (field & (1L << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref long field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 63)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 63.");
        }

        if (value)
        {
            field |= (1L << bitIndex);
        }
        else
        {
            field &= ~(1L << bitIndex);
        }
    }

    public static bool GetBoolValue(ref readonly ulong field, int bitIndex)
    {
        if (bitIndex < 0 || bitIndex > 63)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 63.");
        }

        return (field & (1UL << bitIndex)) != 0;
    }

    public static void SetBoolValue(ref ulong field, int bitIndex, bool value)
    {
        if (bitIndex < 0 || bitIndex > 63)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitIndex), bitIndex, "The bit index must be between 0 and 63.");
        }

        if (value)
        {
            field |= (1UL << bitIndex);
        }
        else
        {
            field &= ~(1UL << bitIndex);
        }
    }
}
