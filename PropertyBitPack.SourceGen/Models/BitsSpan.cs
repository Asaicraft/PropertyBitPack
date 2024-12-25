using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public readonly struct BitsSpan(FieldRequest fieldRequest, byte start, byte length)
{
    private readonly FieldRequest _fieldRequest = fieldRequest;
    private readonly byte _start = start;
    private readonly byte _length = length;

    public FieldRequest FieldRequest => _fieldRequest;
    public byte Start => _start;
    public byte Length => _length;
}
