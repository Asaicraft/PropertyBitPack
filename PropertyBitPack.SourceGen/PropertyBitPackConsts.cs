using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal static class PropertyBitPackConsts
{
    public const string RootNamespace = nameof(PropertyBitPack);
    public const string BitFieldAttributeName = $"{RootNamespace}.{nameof(BitFieldAttribute)}";
    public const string IExtendedBitFieldAttributeName = $"{RootNamespace}.{nameof(IExtendedBitFieldAttribute)}";
    public const string IReadOnlyBitFieldAttributeName = $"{RootNamespace}.{nameof(IReadOnlyBitFieldAttribute)}";

    public const string BitFieldAttributeFieldName = nameof(BitFieldAttribute.FieldName);
    public const string BitFieldAttributeBitsCount = nameof(BitFieldAttribute.BitsCount);

    public const string IExtendedBitFieldAttributeGetterLargeSizeValueName = nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName);
}
