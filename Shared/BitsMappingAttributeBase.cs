using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

[AttributeUsage(AttributeTargets.Property)]
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BitsMappingAttributeBase: Attribute
{
    public int BitsCount
    {
        get; set;
    }

    public string? FieldName
    {
        get; set;
    }
}
