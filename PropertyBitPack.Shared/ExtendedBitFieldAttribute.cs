using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
sealed class ExtendedBitFieldAttribute : BitsMappingAttributeBase, IExtendedBitFieldAttribute
{
    public required string? GetterLargeSizeValueName
    {
        get; set;
    }
}
