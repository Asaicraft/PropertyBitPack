using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;

public class BitFieldUtilsTest
{
    [Fact]
    public void TestBoolValue()
    {
        var byteField = (byte)0b0000_0000;
        var byteFieldIndex = Random.Shared.Next(0, 8);

        BitFieldUtils.SetBoolValue(ref byteField, byteFieldIndex, true);


        Assert.True(BitFieldUtils.GetBoolValue(ref byteField, byteFieldIndex));
    }

    [Fact]
    public void TestByteValue()
    {
        byte field = 0;
        var range = 2..5; // 3 bits
        byte valueToSet = 0b101; // 5
        BitFieldUtils.SetByteValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetByteValue(ref field, range));
    }

    [Fact]
    public void TestSByteValue()
    {
        sbyte field = 0;
        var range = 1..4; // 3 bits
        sbyte valueToSet = 0b011; // 3
        BitFieldUtils.SetSByteValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetSByteValue(ref field, range));
    }

    [Fact]
    public void TestShortValue()
    {
        short field = 0;
        var range = 5..12; // 7 bits
        short valueToSet = 0b1010101; // 85
        BitFieldUtils.SetShortValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetShortValue(ref field, range));
    }

    [Fact]
    public void TestUShortValue()
    {
        ushort field = 0;
        var range = 3..10; // 7 bits
        ushort valueToSet = 0b1101010; // 106
        BitFieldUtils.SetUShortValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetUShortValue(ref field, range));
    }

    [Fact]
    public void TestIntValue()
    {
        var field = 0;
        var range = 10..20; // 10 bits
        var valueToSet = 0b1010101010; // 682
        BitFieldUtils.SetIntValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetIntValue(ref field, range));
    }

    [Fact]
    public void TestUIntValue()
    {
        uint field = 0;
        var range = 5..15; // 10 bits
        uint valueToSet = 0b1110011100; // Some 10-bit value
        BitFieldUtils.SetUIntValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetUIntValue(ref field, range));
    }

    [Fact]
    public void TestLongValue()
    {
        long field = 0;
        var range = 20..40; // 20 bits
        long valueToSet = 0b10101010101010101010; // 20-bit value
        BitFieldUtils.SetLongValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetLongValue(ref field, range));
    }

    [Fact]
    public void TestULongValue()
    {
        ulong field = 0;
        var range = 30..50; // 20 bits
        var valueToSet = 0b10101010101010101010UL; // 20-bit value
        BitFieldUtils.SetULongValue(ref field, range, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetULongValue(ref field, range));
    }

    [Fact]
    public void TestByteMultipleValues()
    {
        // Partition 8 bits: 0..3 (3 bits), 3..6 (3 bits), 6..8 (2 bits)
        byte field = 0;
        byte value1 = 0b101; // 5 (3 bits)
        byte value2 = 0b011; // 3 (3 bits)
        byte value3 = 0b10;  // 2 (2 bits)

        BitFieldUtils.SetByteValue(ref field, 0..3, value1);
        BitFieldUtils.SetByteValue(ref field, 3..6, value2);
        BitFieldUtils.SetByteValue(ref field, 6..8, value3);

        Assert.Equal(value1, BitFieldUtils.GetByteValue(ref field, 0..3));
        Assert.Equal(value2, BitFieldUtils.GetByteValue(ref field, 3..6));
        Assert.Equal(value3, BitFieldUtils.GetByteValue(ref field, 6..8));
    }

    [Fact]
    public void TestSByteMultipleValues()
    {
        // Partition 8 bits: 0..3, 3..6, 6..8
        sbyte field = 0;
        sbyte value1 = 0b101; // 5
        sbyte value2 = 0b011; // 3
        sbyte value3 = 0b10;  // 2

        BitFieldUtils.SetSByteValue(ref field, 0..3, value1);
        BitFieldUtils.SetSByteValue(ref field, 3..6, value2);
        BitFieldUtils.SetSByteValue(ref field, 6..8, value3);

        Assert.Equal(value1, BitFieldUtils.GetSByteValue(ref field, 0..3));
        Assert.Equal(value2, BitFieldUtils.GetSByteValue(ref field, 3..6));
        Assert.Equal(value3, BitFieldUtils.GetSByteValue(ref field, 6..8));
    }

    [Fact]
    public void TestShortMultipleValues()
    {
        // Partition 16 bits: 0..5 (5 bits), 5..11 (6 bits), 11..16 (5 bits)
        short field = 0;
        short value1 = 0b10101;  // 21 (max 31)
        short value2 = 0b110011; // 51 (max 63)
        short value3 = 0b10110;  // 22 (max 31)

        BitFieldUtils.SetShortValue(ref field, 0..5, value1);
        BitFieldUtils.SetShortValue(ref field, 5..11, value2);
        BitFieldUtils.SetShortValue(ref field, 11..16, value3);

        Assert.Equal(value1, BitFieldUtils.GetShortValue(ref field, 0..5));
        Assert.Equal(value2, BitFieldUtils.GetShortValue(ref field, 5..11));
        Assert.Equal(value3, BitFieldUtils.GetShortValue(ref field, 11..16));
    }

    [Fact]
    public void TestUShortMultipleValues()
    {
        // Partition 16 bits: 0..5, 5..11, 11..16
        ushort field = 0;
        ushort value1 = 0b10101;  // 21
        ushort value2 = 0b110011; // 51
        ushort value3 = 0b10110;  // 22

        BitFieldUtils.SetUShortValue(ref field, 0..5, value1);
        BitFieldUtils.SetUShortValue(ref field, 5..11, value2);
        BitFieldUtils.SetUShortValue(ref field, 11..16, value3);

        Assert.Equal(value1, BitFieldUtils.GetUShortValue(ref field, 0..5));
        Assert.Equal(value2, BitFieldUtils.GetUShortValue(ref field, 5..11));
        Assert.Equal(value3, BitFieldUtils.GetUShortValue(ref field, 11..16));
    }

    [Fact]
    public void TestIntMultipleValues()
    {
        // Partition 32 bits: 0..10 (10 bits), 10..22 (12 bits), 22..32 (10 bits)
        var field = 0;
        var value1 = 0x1AB; // 427, fits in 10 bits (max 1023)
        var value2 = 0xABC; // 2748, fits in 12 bits (max 4095)
        var value3 = 0x2CD; // 717, fits in 10 bits

        BitFieldUtils.SetIntValue(ref field, 0..10, value1);
        BitFieldUtils.SetIntValue(ref field, 10..22, value2);
        BitFieldUtils.SetIntValue(ref field, 22..32, value3);

        Assert.Equal(value1, BitFieldUtils.GetIntValue(ref field, 0..10));
        Assert.Equal(value2, BitFieldUtils.GetIntValue(ref field, 10..22));
        Assert.Equal(value3, BitFieldUtils.GetIntValue(ref field, 22..32));
    }

    [Fact]
    public void TestUIntMultipleValues()
    {
        // Partition 32 bits: 0..10, 10..22, 22..32
        uint field = 0;
        uint value1 = 0x1AB; // 427
        uint value2 = 0xABC; // 2748
        uint value3 = 0x2CD; // 717

        BitFieldUtils.SetUIntValue(ref field, 0..10, value1);
        BitFieldUtils.SetUIntValue(ref field, 10..22, value2);
        BitFieldUtils.SetUIntValue(ref field, 22..32, value3);

        Assert.Equal(value1, BitFieldUtils.GetUIntValue(ref field, 0..10));
        Assert.Equal(value2, BitFieldUtils.GetUIntValue(ref field, 10..22));
        Assert.Equal(value3, BitFieldUtils.GetUIntValue(ref field, 22..32));
    }

    [Fact]
    public void TestLongMultipleValues()
    {
        // Partition 64 bits: 0..20 (20 bits), 20..45 (25 bits), 45..64 (19 bits)
        long field = 0;
        long value1 = 123456;  // fits in 20 bits (max 1,048,575)
        long value2 = 9876543; // fits in 25 bits (max 33,554,431)
        long value3 = 345678;  // fits in 19 bits (max 524,287)

        BitFieldUtils.SetLongValue(ref field, 0..20, value1);
        BitFieldUtils.SetLongValue(ref field, 20..45, value2);
        BitFieldUtils.SetLongValue(ref field, 45..64, value3);

        Assert.Equal(value1, BitFieldUtils.GetLongValue(ref field, 0..20));
        Assert.Equal(value2, BitFieldUtils.GetLongValue(ref field, 20..45));
        Assert.Equal(value3, BitFieldUtils.GetLongValue(ref field, 45..64));
    }

    [Fact]
    public void TestULongMultipleValues()
    {
        // Partition 64 bits: 0..20, 20..45, 45..64
        ulong field = 0;
        var value1 = 123456UL;  // fits in 20 bits
        var value2 = 9876543UL; // fits in 25 bits
        var value3 = 345678UL;  // fits in 19 bits

        BitFieldUtils.SetULongValue(ref field, 0..20, value1);
        BitFieldUtils.SetULongValue(ref field, 20..45, value2);
        BitFieldUtils.SetULongValue(ref field, 45..64, value3);

        Assert.Equal(value1, BitFieldUtils.GetULongValue(ref field, 0..20));
        Assert.Equal(value2, BitFieldUtils.GetULongValue(ref field, 20..45));
        Assert.Equal(value3, BitFieldUtils.GetULongValue(ref field, 45..64));
    }

    [Fact]
    public void TestByteValue_RangeOperators_WithCaret()
    {
        byte field = 0;
        // ^3..^1: For an 8-bit field, ^3 means 8-3 = 5 and ^1 means 8-1 = 7, so length = 2.
        byte valueToSet = 0b10; // value = 2 fits in 2 bits
        BitFieldUtils.SetByteValue(ref field, ^3..^1, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetByteValue(ref field, ^3..^1));
    }

    [Fact]
    public void TestByteValue_RangeOperators_StartOnly()
    {
        byte field = 0;
        // 4.. means from index 4 to end (4 bits: indices 4-7)
        byte valueToSet = 0b1010; // 10 fits in 4 bits
        BitFieldUtils.SetByteValue(ref field, 4.., valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetByteValue(ref field, 4..));
    }

    [Fact]
    public void TestByteValue_RangeOperators_EndOnly()
    {
        byte field = 0;
        // ..3 means from index 0 to 3 (3 bits)
        byte valueToSet = 0b110; // 6 fits in 3 bits
        BitFieldUtils.SetByteValue(ref field, ..3, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetByteValue(ref field, ..3));
    }

    [Fact]
    public void TestSByteValue_RangeOperators_WithCaret()
    {
        sbyte field = 0;
        // Using ^ operators on an 8-bit sbyte.
        sbyte valueToSet = 0b010; // 2 fits in 2 bits.
        BitFieldUtils.SetSByteValue(ref field, ^3..^1, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetSByteValue(ref field, ^3..^1));
    }

    [Fact]
    public void TestShortValue_RangeOperators_WithMixedRange()
    {
        short field = 0;
        // For a 16-bit short, use 5..^3. ^3 means 16-3 = 13 so length = 13-5 = 8 bits.
        short valueToSet = 0x5A; // 90 decimal, fits in 8 bits.
        BitFieldUtils.SetShortValue(ref field, 5..^3, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetShortValue(ref field, 5..^3));
    }

    [Fact]
    public void TestUShortValue_RangeOperators_WithMixedRange()
    {
        ushort field = 0;
        // For a 16-bit ushort, use 4..^4. ^4 means 16-4 = 12 so length = 12-4 = 8 bits.
        ushort valueToSet = 0xA5; // 165 decimal.
        BitFieldUtils.SetUShortValue(ref field, 4..^4, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetUShortValue(ref field, 4..^4));
    }

    [Fact]
    public void TestIntValue_RangeOperators_WithCaret()
    {
        var field = 0;
        // For a 32-bit int, use ^12..^4: ^12 means 32-12 = 20, ^4 means 32-4 = 28 so length = 8 bits.
        var valueToSet = 0xAB; // 171 decimal.
        BitFieldUtils.SetIntValue(ref field, ^12..^4, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetIntValue(ref field, ^12..^4));
    }

    [Fact]
    public void TestUIntValue_RangeOperators_WithMixedRange()
    {
        uint field = 0;
        // For a 32-bit uint, use 10..^10: ^10 means 32-10 = 22 so length = 22-10 = 12 bits.
        uint valueToSet = 0xABC; // 2748 decimal.
        BitFieldUtils.SetUIntValue(ref field, 10..^10, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetUIntValue(ref field, 10..^10));
    }

    [Fact]
    public void TestLongValue_RangeOperators_WithMixedRange()
    {
        long field = 0;
        // For a 64-bit long, use 20..^20: ^20 means 64-20 = 44, so length = 44-20 = 24 bits.
        long valueToSet = 0xABCDE; // 703710 decimal, fits in 24 bits.
        BitFieldUtils.SetLongValue(ref field, 20..^20, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetLongValue(ref field, 20..^20));
    }

    [Fact]
    public void TestULongValue_RangeOperators_WithCaret()
    {
        ulong field = 0;
        // For a 64-bit ulong, use ..^32: from start to index 64-32 = 32 (length = 32 bits)
        ulong valueToSet = 0xDEADBEEF; // fits in 32 bits.
        BitFieldUtils.SetULongValue(ref field, ..^32, valueToSet);
        Assert.Equal(valueToSet, BitFieldUtils.GetULongValue(ref field, ..^32));
    }
}
