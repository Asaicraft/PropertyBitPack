using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
public partial struct PackedStruct
{
    private long _packed;
    private byte _packed2;

    [BitField(FieldName = nameof(_packed))]
    public partial bool Bool1
    {
        get; set;
    }

    [BitField(FieldName = nameof(_packed))]
    public partial bool Bool2
    {
        get; set;
    }

    [BitField(BitsCount = 11, FieldName = nameof(_packed))]
    public partial short Short1
    {
        get; set;
    }

    [BitField(BitsCount = 9, FieldName = nameof(_packed))]

    public partial short Short2
    {
        get; set;
    }

    [BitField(BitsCount = 14, FieldName = nameof(_packed))]
    public partial int Int1
    {
        get; set;
    }

    [BitField(BitsCount = 18, FieldName = nameof(_packed))]
    public partial int Int2
    {
        get; set;
    }

    [BitField(BitsCount = 5, FieldName = nameof(_packed2))]
    public partial byte Hi
    {
        get; set;
    }

    [BitField(BitsCount = 2, FieldName = nameof(_packed2))]
    public partial byte Hi2
    {
        get; set;
    }

    [BitField(FieldName = nameof(_packed2))]
    public partial bool HiBool
    {
        get; set;
    }
}