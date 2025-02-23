using PropertyBitPack.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;

public class RecordStructTests
{
    [Fact]
    public void TestRecordStruct()
    {
        var record = new RecordStructExample
        {
            Property1 = 4, // 4 bits max value is 15
            Property2 = 11111 // 11 bits max value is 2047
        };
        Assert.Equal(4, record.Property1);
        Assert.Equal(2047, record.Property2);
    }
}
