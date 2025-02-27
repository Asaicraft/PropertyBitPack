using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.Example;

public sealed partial class ReadOnlyExtendedBitFieldExample
{
    private readonly int _existingField;

    [ReadOnlyExtendedBitField(BitsCount = 5, GetterLargeSizeValueName = nameof(GetMaxValue))]
    public partial int Data1 { get; }

    [ReadOnlyExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(GetMaxValue), ConstructorAccessModifier = AccessModifier.Public)]
    public partial int Data2 { get; }

    [ReadOnlyExtendedBitField(BitsCount = 6, FieldName = nameof(_existingField), GetterLargeSizeValueName = nameof(GetMaxValue), ConstructorAccessModifier = AccessModifier.Public)]
    public partial int Data3 { get; }

    public static int GetMaxValue() => 1024; // External getter for overflow values
}