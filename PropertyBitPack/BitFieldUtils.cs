using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;
public static partial class BitFieldUtils
{
    private static (int start, int length) GetRangeBounds(Range range, int maxBits)
    {
        var start = range.Start.IsFromEnd ? maxBits - range.Start.Value : range.Start.Value;
        var end = range.End.IsFromEnd ? maxBits - range.End.Value : range.End.Value;

        if (start < 0 || end > maxBits || start >= end)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(range), range, "Invalid bit range.");
        }

        return (start, end - start);
    }
}
