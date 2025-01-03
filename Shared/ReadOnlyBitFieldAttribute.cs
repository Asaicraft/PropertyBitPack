using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack;
public sealed class ReadOnlyBitFieldAttribute : BitsMappingAttributeBase, IReadOnlyBitFieldAttribute
{
    public AccessModifier ConstructorAccessModifier
    {
        get; set;
    } = AccessModifier.Private;
}
