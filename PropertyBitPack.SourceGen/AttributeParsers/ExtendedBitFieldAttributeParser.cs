using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.AttributeParsers;
internal sealed class ExtendedBitFieldAttributeParser : BaseAttributeParser
{
    private static readonly Type[] _validSymbolsTypes = [typeof(IFieldSymbol), typeof(IPropertySymbol), typeof(IMethodSymbol)];

    public override bool IsCandidate(AttributeData attributeData, AttributeSyntax attributeSyntax)
    {
        return HasInterface<IExtendedBitFieldAttribute>(attributeData);
    }

    public override bool TryParse(AttributeData attributeData, AttributeSyntax attributeSyntax, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        result = null;

        var owner = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax)?.ContainingType;

        if(owner is null)
        {
            return false;
        }

        if (!TryGetBitsCount(attributeData, out var bitsCount, propertyDeclarationSyntax, in diagnostics))
        {
            return false;
        }

        if (!TryGetFieldName(attributeData, out var fieldName, semanticModel, in diagnostics, propertyDeclarationSyntax, owner))
        {
            return false;
        }

        if (!TryGetterLargeSizeValueSymbol(semanticModel, attributeData, attributeSyntax, owner, out var getterLargeSizeValueSymbol))
        {
            return false;
        }

        result = new ParsedExtendedBitFiledAttribute(attributeSyntax, attributeData, fieldName, bitsCount, getterLargeSizeValueSymbol);

        return true;
    }

    private bool TryGetterLargeSizeValueSymbol(SemanticModel semanticModel, AttributeData attributeData, AttributeSyntax attributeSyntax, INamedTypeSymbol owner, [NotNullWhen(true)] out ISymbol? getterLargeSizeValueSymbol)
    {
        getterLargeSizeValueSymbol = null;

        var argument = GetAttributeArgument(attributeSyntax, nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName));

        if (argument is null)
        {
            return false;
        }

        var nameOfOperation = semanticModel.GetOperation(argument.Expression);

        if (nameOfOperation is not INameOfOperation nameOf)
        {
            return false;
        }

        var validSymbolsTypes = _validSymbolsTypes.AsSpan();

        getterLargeSizeValueSymbol = ExtractSymbolFromNameOf(owner, nameOf, validSymbolsTypes);

        return getterLargeSizeValueSymbol is not null;
    }
}
