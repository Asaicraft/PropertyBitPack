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
sealed class ReadOnlyBitFieldAttribute : BitsMappingAttributeBase, IReadOnlyBitFieldAttribute
{
    public AccessModifier ConstructorAccessModifier
    {
        get; set;
    } = AccessModifier.Private;
}
