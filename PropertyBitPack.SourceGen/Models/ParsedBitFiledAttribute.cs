using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedBitFiledAttribute : AttributeParsedResult
{
    private ParsedBitFiledAttribute(
        int? bitsCount,
        string? fieldName)
        : base(BitsMappingAttributeType.BitField, bitsCount, fieldName)
    {
    }

    public static bool TryParseBitFieldAttribute(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        var bitsCount = attributeData.NamedArguments.FirstOrDefault(static arg => arg.Key == BitFieldAttributeBitsCount).Value.Value as int?;
        var fieldName = attributeData.NamedArguments.FirstOrDefault(static arg => arg.Key == BitFieldAttributeFieldName).Value.Value as string;

        if (bitsCount is int bitsCountValue)
        {
            if (bitsCountValue < 1)
            {
                diagnosticsBuilder.Add(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidBitsCount, propertyDeclarationSyntax.GetLocation()));
                result = null;
                return false;
            }
        }

        result = new ParsedBitFiledAttribute(bitsCount, fieldName);

        return true;
    }
}
