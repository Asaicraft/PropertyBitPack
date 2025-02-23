using PropertyBitPack.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;

public class ExtendedBitFieldTests
{
    [Fact]
    public void TestExtendedBitField()
    {
        var extendedBitField = new ExtendedBitFieldExample
        {
            SlotCount = 156, // 11 bits max value is 2047
            SlotCount2 = 981, // 11 bits max value is 2047
            SlotCount3 = 15683, // The resulting value will be calculated as: 123 * 6 + 2^11 = 123 * 6 + 2048 = 738 + 2048 = 2786. Because the value is greater than 2047, we will use extended value 12345
            SlotCount4 = 2012, // 11 bits max value is 2047
            SlotCount5 = 313 // 11 bits max value is 2047
        };

        Assert.Equal(156, extendedBitField.SlotCount);
        Assert.Equal(981, extendedBitField.SlotCount2);
        Assert.Equal(2786, extendedBitField.SlotCount3);
        Assert.Equal(2012, extendedBitField.SlotCount4);
        Assert.Equal(313, extendedBitField.SlotCount5);
    }

    [Fact]
    public void VerifyExtendedBitFieldInternalField()
    {
        var extendedBitField = typeof(ExtendedBitFieldExample);

        var fields = extendedBitField.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.Single(fields);

        var field = fields[0];

        Assert.Equal(typeof(ulong), field.FieldType);
    }
}