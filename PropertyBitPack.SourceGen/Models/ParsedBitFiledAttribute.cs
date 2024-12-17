using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedBitFiledAttribute: AttributeParsedResult
{
    private ParsedBitFiledAttribute(
        int? bitsCount,
        string? fieldName)
        : base(BitsMappingAttributeType.BitField, bitsCount, fieldName)
    {
    }

    public static bool TryParseBitFieldAttribute(AttributeData attributeData, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        var bitsCount = attributeData.NamedArguments.FirstOrDefault(static arg => arg.Key == BitFieldAttributeBitsCount).Value.Value as int?;
        var fieldName = attributeData.NamedArguments.FirstOrDefault(static arg => arg.Key == BitFieldAttributeFieldName).Value.Value as string;

        result = new ParsedBitFiledAttribute(bitsCount, fieldName);

        return true;
    }
}
