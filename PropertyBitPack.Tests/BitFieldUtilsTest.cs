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
}
