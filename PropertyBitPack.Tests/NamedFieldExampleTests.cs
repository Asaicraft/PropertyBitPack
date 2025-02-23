using PropertyBitPack.Example;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Tests;
public class NamedFieldExampleTests
{
    [Fact]
    public void TestNamedFieldExample()
    {
        var namedFieldExample = new NamedFieldExample
        {
            Short1 = 376, // 9 bits max value is 511
            Short2 = 1000, // 9 bits max value is 511
            Short3 = 9999 // 9 bits max value is 511, but we use extended value 12345
        };

        Assert.Equal(376, namedFieldExample.Short1);
        Assert.Equal(511, namedFieldExample.Short2);
        Assert.Equal(12345, namedFieldExample.Short3);

        namedFieldExample.Short3 = 123;

        Assert.Equal(123, namedFieldExample.Short3);
    }

    [Fact]
    public void TestFieldExistence()
    {
        var nameType = typeof(NamedFieldExample);

        var field = nameType.GetField("_another", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(field);
    }
}
