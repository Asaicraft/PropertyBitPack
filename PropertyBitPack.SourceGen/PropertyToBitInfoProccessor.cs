using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen;
public static class PropertyToBitInfoProccessor
{
    public static Result<PropertyToBitInfo>? Process(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if(context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            Debug.Assert(false, "The provided syntax node is not a property declaration.");
            return null;
        }

        using var diaognosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

        var semanticModel = context.SemanticModel;
        var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);

        if (propertySymbol is null)
        {
            Debug.Assert(false, "Failed to retrieve the symbol for the property declaration.");
            return null;
        }

        var owner = propertySymbol.ContainingType;
        var attributeData = propertySymbol.GetAttributes();

        if (attributeData.Length == 0)
        {
            Debug.Assert(false, "The property declaration does not have any attributes.");
            return null;
        }

        var attribute = attributeData.FirstOrDefault(static attr => CandidateAttributes.Contains(attr.AttributeClass?.Name));

        // after enumerating mb we should check cancellationToken
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if (attribute is null)
        {
            // It's normal for a property to not have any bit-mapping attributes.
            // Do not log an error or warning.
            return null;
        }

        var attributeType = attribute.AttributeClass?.Name switch
        {
            BitFieldAttribute => BitsMappingAttributeType.BitField,
            ExtendedBitFieldAttribute => BitsMappingAttributeType.ExtendedBitField,
            _ => BitsMappingAttributeType.Unknown
        };

        if (attributeType == BitsMappingAttributeType.Unknown)
        {
            // The attribute type is unknown.
            return null;
        }


        if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        if(!AttributeParsedResult.TryParse(attribute, semanticModel, in diaognosticsBuilder, out var attributeParsedResult))
        {
            return Result<PropertyToBitInfo>.Failure(diaognosticsBuilder.ToImmutable());
        }
    }

}
