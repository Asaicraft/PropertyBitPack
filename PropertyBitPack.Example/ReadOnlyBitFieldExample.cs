using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
public sealed partial class ReadOnlyBitFieldExample
{
    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Internal)]
    public partial bool Flag1 
    { 
        get; 
    }

    [ReadOnlyBitField]
    public partial bool Flag2
    {
        get;
    }

    [ReadOnlyBitField]
    public partial bool Flag3
    {
        get;
    }

    [ReadOnlyBitField]
    public partial int AdditionalData
    {
        get;
    }
}
