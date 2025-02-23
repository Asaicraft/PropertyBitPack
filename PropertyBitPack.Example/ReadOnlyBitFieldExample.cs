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

    [ReadOnlyBitField(BitsCount = 15, FieldName = "_bitField")]
    public partial int AdditionalData2
    {
        get;
    }

    [ReadOnlyBitField(BitsCount = 1, FieldName = "_bitField")]
    public partial int AdditionalData3
    {
        get;
    }
}
