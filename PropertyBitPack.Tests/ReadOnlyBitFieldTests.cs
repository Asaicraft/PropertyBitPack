using PropertyBitPack.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;
public class ReadOnlyBitFieldTests
{
    [Fact]
    public void TestReadOnlyBitField()
    {
        // AdditionalData4 is 4 bits, max value is 15, try to set 123
        // AdditionalData5 is 4 bits, max value is 15, try to set 77, will called GetterLargeSizeValueName, return 255
        // Flag2 is 1 bit, max value is 1, try to set true
        // Flag3 is 1 bit, max value is 1, try to set false
        // AdditionalData is 32 bits, max value is int.MaxValue, try to set 23512
        // AdditionalData2 is 15 bits, max value is 32767, try to set 3232
        // AdditionalData3 is 1 bit, max value is 1, try to set 333, return 1
        var readOnlyBitField = new ReadOnlyBitFieldExample(123, 77, true, false, 23512, 3232, 333);

        Assert.Equal(23512, readOnlyBitField.AdditionalData);
        Assert.Equal(3232, readOnlyBitField.AdditionalData2);
        Assert.Equal(1, readOnlyBitField.AdditionalData3);
        Assert.Equal(15, readOnlyBitField.AdditionalData4);
        Assert.Equal(255, readOnlyBitField.AdditionalData5);

        Assert.True(readOnlyBitField.Flag2);
        Assert.False(readOnlyBitField.Flag3);

        // Flag1 is 1 bit, max value is 1, try to set true
        readOnlyBitField = new(true);

        Assert.True(readOnlyBitField.Flag1);
    }
}
