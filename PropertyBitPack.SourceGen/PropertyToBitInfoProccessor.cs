using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            Debug.Assert(false, "The provided syntax node is not a property declaration.");
            return null;
        }

        using var diaognosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
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

        var attribute = attributeData.FirstOrDefault(static attr => AttributeParsedResult.Predicate(attr));

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


        if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        if (!AttributeParsedResult.TryParse(attribute, propertyDeclaration, semanticModel, in diaognosticsBuilder, out var attributeParsedResult))
        {
            return new(null, diaognosticsBuilder.ToImmutable());
        }

        var isInit = propertyDeclaration.AccessorList?.Accessors.Any(static accessor => accessor.IsKind(SyntaxKind.InitAccessorDeclaration)) ?? false;
        var setterOrInitModifiers = propertyDeclaration.AccessorList?.Accessors
            .Where(static accessor =>
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration) ||
                accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            .Select(static accessor => accessor.Modifiers).FirstOrDefault() ?? default;

        var propertyToBitInfo = new PropertyToBitInfo(
            attributeParsedResult.AttributeType,
            attributeParsedResult is ParsedExtendedBitFiledAttribute extendedBitFiledAttribute
                ? extendedBitFiledAttribute.SymbolGetterLargeSizeValue
                : null,
            isInit,
            setterOrInitModifiers,
            propertySymbol,
            attributeParsedResult.BitsCount ?? GetDefaultBitsCount(propertySymbol.Type),
            attributeParsedResult.FieldName,
            propertySymbol.Type,
            owner
        );

        return new(propertyToBitInfo, diaognosticsBuilder.ToImmutable());
    }


    public static int GetDefaultBitsCount(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Byte => 8,
            SpecialType.System_SByte => 8,
            SpecialType.System_UInt16 => 16,
            SpecialType.System_Int16 => 16,
            SpecialType.System_UInt32 => 32,
            SpecialType.System_Int32 => 32,
            SpecialType.System_UInt64 => 64,
            SpecialType.System_Int64 => 64,
            _ => ThrowHelper.ThrowNotSupportedException<int>($"The type '{type.Name}' is not supported.")
        };

    }
}
