using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
public partial class ExtendedBitFieldExample
{
    public const int BitsCountSlotCountProperty = 11;

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount
    {
        get; internal set;
    }

    [BitField(BitsCount = BitsCountSlotCountProperty)]
    public partial ushort SlotCount2
    {
        get; internal set;
    }

    [ExtendedBitField(BitsCount = BitsCountSlotCountProperty, GetterLargeSizeValueName = nameof(GetLargeSlotCount))]
    public partial int SlotCount3
    {
        get; internal set;
    }

    public static int GetLargeSlotCount(int a = 6)
    {
        return ((int)Math.Pow(2, 11)) + 123 * a;
    }
}
