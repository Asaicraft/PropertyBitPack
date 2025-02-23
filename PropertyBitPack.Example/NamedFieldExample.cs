using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
internal sealed partial class NamedFieldExample
{
    [BitField(FieldName = "_another", BitsCount = 9)]
    public partial short Short1
    {
        get; set;
    }

    [BitField(FieldName = "_another", BitsCount = 9)]
    public partial short Short2
    {
        get; set;
    }

    [ExtendedBitField(FieldName = "_another", BitsCount = 9, GetterLargeSizeValueName = nameof(ExtendedValue))]
    public partial short Short3
    {
        get; set;
    }

    private static short ExtendedValue() => 12345;
}
