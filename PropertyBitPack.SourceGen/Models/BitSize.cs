using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal enum BitSize : byte
{
    Invalid = 0,
    Bool = 1,
    Byte = 8,
    UInt16 = 16,
    Int16 = 16,
    UInt32 = 32,
    Int32 = 32,
    UInt64 = 64,
    Int64 = 64
}
