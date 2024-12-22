using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;
public sealed class ReadOnlyExtendedBitField : BitsMappingAttributeBase, IReadOnlyBitFieldAttribute, IExtendedBitFieldAttribute
{
    public AccessModifier ConstructorAccessModifier
    {
        get; set;
    }
    public required string? GetterLargeSizeValueName
    {
        get; set;
    }
}
