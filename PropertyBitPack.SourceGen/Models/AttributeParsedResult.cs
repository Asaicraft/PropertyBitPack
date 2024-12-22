using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        var attributeType = ExtractType(attributeData);

        if (attributeType == BitsMappingAttributeType.Unknown)
        {
            return false;
        }

        if(attributeType == BitsMappingAttributeType.BitField)
        {
            return ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, propertyDeclarationSyntax, in diagnostics, out result);
        }

        if(attributeType == BitsMappingAttributeType.IExtendedBitField)
        {
            return ParsedExtendedBitFiledAttribute.TryParseExtendedBitFieldAttribute(attributeData, propertyDeclarationSyntax, semanticModel, in diagnostics, out result);
        }

        return false;
    }

    public static BitsMappingAttributeType ExtractType(AttributeData attributeData)
    {
        var attributeClass = attributeData.AttributeClass;

        if(attributeClass is null)
        {
            return BitsMappingAttributeType.Unknown;
        }


        var bitsMappingAttributeType = BitsMappingAttributeType.Unknown;

        if(attributeClass.ContainingNamespace.Name == nameof(PropertyBitPack))
        {
            if(attributeClass.Name == BitFieldAttribute)
            {
                bitsMappingAttributeType |= BitsMappingAttributeType.BitField;
            }

            var interfaces = attributeClass.AllInterfaces.Where(x => x.ContainingNamespace.Name == nameof(PropertyBitPack)).ToImmutableArray();

            foreach(var candidateInterface in interfaces)
            {
                if (candidateInterface.Name == IExtendedBitFieldAttribute)
                {
                    bitsMappingAttributeType |= BitsMappingAttributeType.IExtendedBitField;
                }

                if(candidateInterface.Name == IReadOnlyBitFieldAttribute)
                {
                    bitsMappingAttributeType |= BitsMappingAttributeType.IReadOnlyBitField;
                }
            }
        }


        return bitsMappingAttributeType;
    }

    public static bool Predicate(AttributeData attributeData)
    {
        var bitMappingAttributeType = ExtractType(attributeData);

        return bitMappingAttributeType != BitsMappingAttributeType.Unknown;
    }
}