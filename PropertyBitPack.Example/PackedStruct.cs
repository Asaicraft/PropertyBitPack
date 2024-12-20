using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
public partial struct PackedStruct
{
    [BitField]
    public partial bool Bool1
    {
        get; set;
    }

    [BitField]
    public partial bool Bool2
    {
        get; set;
    }

    [BitField(BitsCount = 11)]
    public partial short Short1
    {
        get; set;
    }

    [BitField(BitsCount = 9)]

    public partial short Short2
    {
        get; set;
    }

    [BitField(BitsCount = 14)]
    public partial int Int1
    {
        get; set;
    }

    [BitField(BitsCount = 18)]
    public partial int Int2
    {
        get; set;
    }

    [BitField(BitsCount = 5, FieldName = "_hi")]
    public partial byte Hi
    {
        get; set;
    }

    [BitField(BitsCount = 2, FieldName = "_hi")]
    public partial byte Hi2
    {
        get; set;
    }

    [BitField(FieldName = "_hi")]
    public partial bool HiBool
    {
        get; set;
    }
}