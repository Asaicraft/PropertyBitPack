using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public abstract class AttributeParsedResult(
    BitsMappingAttributeType attributeType,
    int? bitsCount,
    string? fieldName)
{
    public BitsMappingAttributeType AttributeType
    {
        get;
    } = attributeType;

    public int? BitsCount
    {
        get;
    } = bitsCount;

    public string? FieldName
    {
        get;
    } = fieldName;

    public static bool TryParse(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        result = null;
        var attributeType = attributeData.AttributeClass?.Name switch
        {
            BitFieldAttribute => BitsMappingAttributeType.BitField,
            ExtendedBitFieldAttribute => BitsMappingAttributeType.ExtendedBitField,
            _ => BitsMappingAttributeType.Unknown
        };

        if (attributeType == BitsMappingAttributeType.Unknown)
        {
            return false;
        }

        if(attributeType == BitsMappingAttributeType.BitField)
        {
            return ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, propertyDeclarationSyntax, in diagnostics, out result);
        }

        if(attributeType == BitsMappingAttributeType.ExtendedBitField)
        {
            return ParsedExtendedBitFiledAttribute.TryParseExtendedBitFieldAttribute(attributeData, propertyDeclarationSyntax, semanticModel, in diagnostics, out result);
        }

        return false;
    }

    
}