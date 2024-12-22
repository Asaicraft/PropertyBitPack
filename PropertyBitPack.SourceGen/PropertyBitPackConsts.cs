using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class PropertyBitPackConsts
{
    public const string BitFieldAttribute = nameof(BitFieldAttribute);
    public const string IExtendedBitFieldAttribute = nameof(IExtendedBitFieldAttribute);
    public const string IReadOnlyBitFieldAttribute = nameof(IReadOnlyBitFieldAttribute);

    public static readonly string[] CandidateAttributes = [BitFieldAttribute, IExtendedBitFieldAttribute];

    public const string BitFieldAttributeFieldName = "FieldName";
    public const string BitFieldAttributeBitsCount = "BitsCount";

    public const string IExtendedBitFieldAttributeGetterLargeSizeValueName = "GetterLargeSizeValueName";
}
