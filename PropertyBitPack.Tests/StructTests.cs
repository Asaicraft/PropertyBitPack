using PropertyBitPack.Example;
using System.Runtime.InteropServices;

namespace PropertyBitPack.Tests;

public class StructTests
{
    public static PackedStruct Create()
    {
        return new PackedStruct
        {
            Bool1 = true,
            Bool2 = false,
            Short1 = 4313,  // max value for 11 bits is 2047
            Short2 = 341,   // max value for 9 bits is 511
            Int1 = 12345,   // max value for 14 bits is 16383
            Int2 = 11123456, // max value for 18 bits is 262143
            Hi = 22,        // max value for 5 bits is 31
            Hi2 = 11,       // max value for 2 bits is 3
            HiBool = true
        };

    }

    [Fact]
    public void PackedStructPropertiesTest()
    {
        var packedStruct = Create();

        Assert.True(packedStruct.Bool1);
        Assert.False(packedStruct.Bool2);
        Assert.Equal(2047, packedStruct.Short1);
        Assert.Equal(341, packedStruct.Short2);
        Assert.Equal(12345, packedStruct.Int1);
        Assert.Equal(262143, packedStruct.Int2);
        Assert.Equal(22, packedStruct.Hi);
        Assert.Equal(3, packedStruct.Hi2);
        Assert.True(packedStruct.HiBool);
    }

    [Fact]
    public void StructSizeComparison()
    {
        var simpleStructSize = Marshal.SizeOf<SimpleStruct>();
        var packedStructSize = Marshal.SizeOf<PackedStruct>();

        Assert.True(simpleStructSize > packedStructSize);
    }
}
