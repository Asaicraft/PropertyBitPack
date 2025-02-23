using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;
public sealed partial class ReadOnlyBitFieldExample
{
    private readonly byte _existing;

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Internal)]
    public partial bool Flag1 
    { 
        get; 
    }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial bool Flag2
    {
        get;
    }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial bool Flag3
    {
        get;
    }

    [ReadOnlyBitField(ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData
    {
        get;
    }

    [ReadOnlyBitField(BitsCount = 15, FieldName = "_bitField", ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData2
    {
        get;
    }

    [ReadOnlyBitField(BitsCount = 1, FieldName = "_bitField", ConstructorAccessModifier = AccessModifier.Public)]
    public partial int AdditionalData3
    {
        get;
    }

    [ReadOnlyBitField(BitsCount = 4, FieldName = nameof(_existing), ConstructorAccessModifier = AccessModifier.Public)]
    public partial byte AdditionalData4
    {
        get;
    }

    [ReadOnlyExtendedBitField(BitsCount = 4, FieldName = nameof(_existing), GetterLargeSizeValueName = nameof(MaxByteValue), ConstructorAccessModifier = AccessModifier.Public)]
    public partial byte AdditionalData5
    {
        get;
    }

    public static byte MaxByteValue() => 255;
}
