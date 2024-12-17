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
public abstract class AttributeParsedResult
{

    protected AttributeParsedResult(
        BitsMappingAttributeType attributeType,
        int? bitsCount,
        string? fieldName)
    {
        AttributeType = attributeType;
        BitsCount = bitsCount;
        FieldName = fieldName;
    }

    public BitsMappingAttributeType AttributeType
    {
        get;
    }

    public int? BitsCount
    {
        get;
    }

    public string? FieldName
    {
        get;
    }

    public static bool TryParse(AttributeData attributeData, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        var attributeType = attributeData.AttributeClass?.Name switch
        {
            BitFieldAttribute => BitsMappingAttributeType.BitField,
            ExtendedBitFieldAttribute => BitsMappingAttributeType.ExtendedBitField,
            _ => BitsMappingAttributeType.Unknown
        };

        if (attributeType == BitsMappingAttributeType.Unknown)
        {
            result = null;
            return false;
        }

        if(attributeType == BitsMappingAttributeType.BitField)
        {
            return ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, out result);
        }

        if(attributeType == BitsMappingAttributeType.ExtendedBitField)
        {
            return ParsedExtendedBitFiledAttribute.TryParseExtendedBitFieldAttribute(attributeData, semanticModel, in diagnostics, out result);
        }
    }

    
}