using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal readonly struct BitsSpan(IFieldRequest fieldRequest, byte start, byte length)
{
    private readonly IFieldRequest _fieldRequest = fieldRequest;
    private readonly byte _start = start;
    private readonly byte _length = length;

    public IFieldRequest FieldRequest => _fieldRequest;
    public byte Start => _start;
    public byte Length => _length;

    public override string ToString()
    {
        return $"{_fieldRequest.Name} ({_fieldRequest.FieldType}) [{_start}, {_start + _length})";
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
