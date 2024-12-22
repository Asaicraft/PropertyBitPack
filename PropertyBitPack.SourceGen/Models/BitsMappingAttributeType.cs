using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public enum BitsMappingAttributeType
{
    Unknown = 0,
    BitField = 1 << 0,
    IExtendedBitField = 1 << 1,
    IReadOnlyBitField = 1 << 2,

    ExtendedReadOnlyBitField = IExtendedBitField | IReadOnlyBitField
}
