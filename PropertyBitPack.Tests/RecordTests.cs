using PropertyBitPack.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;

public class RecordTests
{
    [Fact]
    public void TestRecord()
    {
        var record = new RecordExample
        {
            Property1 = 150, // 4 bits max value is 15
            Property2 = 111 // 11 bits max value is 2047
        };
        Assert.Equal(15, record.Property1);
        Assert.Equal(111, record.Property2);
    }
}
