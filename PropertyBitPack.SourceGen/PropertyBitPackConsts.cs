using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class PropertyBitPackConsts
{
    public const string RootNamespace = nameof(PropertyBitPack);
    public const string BitFieldAttribute = $"{RootNamespace}.{nameof(BitFieldAttribute)}";
    public const string IExtendedBitFieldAttribute = $"{RootNamespace}.{nameof(IExtendedBitFieldAttribute)}";
    public const string IReadOnlyBitFieldAttribute = $"{RootNamespace}.{nameof(IReadOnlyBitFieldAttribute)}";

    public const string BitFieldAttributeFieldName = "FieldName";
    public const string BitFieldAttributeBitsCount = "BitsCount";

    public const string IExtendedBitFieldAttributeGetterLargeSizeValueName = "GetterLargeSizeValueName";
}
