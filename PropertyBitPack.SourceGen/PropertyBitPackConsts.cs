using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class PropertyBitPackConsts
{
    public const string BitFieldAttribute = nameof(BitFieldAttribute);
    public const string ExtendedBitFieldAttribute = nameof(ExtendedBitFieldAttribute);

    public static readonly string[] CandidateAttributes = [BitFieldAttribute, ExtendedBitFieldAttribute];

    public const string BitFieldAttributeFieldName = "FieldName";
    public const string BitFieldAttributeBitsCount = "BitsCount";

    public const string ExtendedBitFieldAttributeGetterLargeSizeValueName = "GetterLargeSizeValueName";
}
