using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.SourceGen;
public readonly struct BitsSpan(int start, int length, int fieldBitsCount)
{
    public readonly int Start { get; } = start;
    public readonly int Length { get; } = length;
    public readonly int FieldBitsCount { get; } = fieldBitsCount;

    public readonly int End => Start + Length;

    public override string ToString()
    {
        return $"Start: {Start}, Length: {Length}";
    }

}
