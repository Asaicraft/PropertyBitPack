// See https://aka.ms/new-console-template for more information
using PropertyBitPack;
using PropertyBitPack.Example;
using System.Drawing;

Console.WriteLine("Hello, World!");

Console.WriteLine($"SimpleStruct size: {System.Runtime.InteropServices.Marshal.SizeOf<SimpleStruct>()}");
Console.WriteLine($"PackedStruct size: {System.Runtime.InteropServices.Marshal.SizeOf<PackedStruct>()}");

var simpleStruct = new SimpleStruct
{
    Bool1 = true,
    Bool2 = false,
    Short1 = 4313,
    Short2 = 341,
    Int1 = 12345,
    Int2 = 11123456,
    Hi = 22,
    Hi2 = 11,
    HiBool = true
};

var packedStruct = new PackedStruct
{
    Bool1 = simpleStruct.Bool1,
    Bool2 = simpleStruct.Bool2,
    // max value for 11 bits is 2047
    Short1 = simpleStruct.Short1,
    // max value for 9 bits is 511
    Short2 = simpleStruct.Short2,
    // max value for 14 bits is 16383
    Int1 = simpleStruct.Int1,
    // max value for 18 bits is 262143
    Int2 = simpleStruct.Int2,
    // max value for 5 bits is 31
    Hi = simpleStruct.Hi,
    // max value for 2 bits is 3
    Hi2 = simpleStruct.Hi2,
    HiBool = simpleStruct.HiBool
};

const int nameWidth = 7;
const int valueWidth = 9;
const int bitFieldWidth = 3;

Console.WriteLine();

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Bool1),-nameWidth}: {simpleStruct.Bool1,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Bool1),-nameWidth}: {packedStruct.Bool1,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 1");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Bool2),-nameWidth}: {simpleStruct.Bool2,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Bool2),-nameWidth}: {packedStruct.Bool2,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 1");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Short1),-nameWidth}: {simpleStruct.Short1,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Short1),-nameWidth}: {packedStruct.Short1,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 11");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Short2),-nameWidth}: {simpleStruct.Short2,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Short2),-nameWidth}: {packedStruct.Short2,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 9");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Int1),-nameWidth}: {simpleStruct.Int1,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Int1),-nameWidth}: {packedStruct.Int1,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 14");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Int2),-nameWidth}: {simpleStruct.Int2,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Int2),-nameWidth}: {packedStruct.Int2,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 18");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Hi),-nameWidth}: {simpleStruct.Hi,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Hi),-nameWidth}: {packedStruct.Hi,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 5");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.Hi2),-nameWidth}: {simpleStruct.Hi2,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.Hi2),-nameWidth}: {packedStruct.Hi2,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 2");

Console.WriteLine($"{nameof(simpleStruct)}.{nameof(simpleStruct.HiBool),-nameWidth}: {simpleStruct.HiBool,-valueWidth} " +
                  $"{nameof(packedStruct)}.{nameof(packedStruct.HiBool),-nameWidth}: {packedStruct.HiBool,-valueWidth} " +
                  $"{nameof(packedStruct)}{nameof(BitFieldAttribute.BitsCount),-bitFieldWidth}: 1");