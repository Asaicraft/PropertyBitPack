using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedExtendedBitFiledAttribute : AttributeParsedResult
{
    private ParsedExtendedBitFiledAttribute(
        AttributeParsedResult attributeParsedResult,
        ISymbol symbolGetterLargeSizeValue)
        : this(attributeParsedResult.BitsCount, attributeParsedResult.FieldName, symbolGetterLargeSizeValue)
    {
    }

    private ParsedExtendedBitFiledAttribute(
        int? bitsCount,
        string? fieldName,
        ISymbol symbolGetterLargeSizeValue)
        : base(BitsMappingAttributeType.ExtendedBitField, bitsCount, fieldName)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }


    public ISymbol SymbolGetterLargeSizeValue
    {
        get;
    }

    public static bool TryParseExtendedBitFieldAttribute(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        result = null;

        if (semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol targetPropertySymbol)
        {
            ThrowHelper.ThrowUnreachableException();
            return false;
        }

        var owner = targetPropertySymbol.ContainingType;

        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            return false;
        }

        // It's impossible to have an ExtendedBitField attribute without an argument list.
        if (attributeSyntax.ArgumentList is null)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MissingGetterLargeSizeValue, attributeSyntax.GetLocation(), propertyDeclarationSyntax));
            return false;
        }

        var getterLargeSizeValueNameSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault(x => x.NameEquals?.Name.Identifier.Text == ExtendedBitFieldAttributeGetterLargeSizeValueName);

        // It's impossible to have an ExtendedBitField attribute without a getterLargeSizeValue argument. Because it's property is required.
        if (getterLargeSizeValueNameSyntax is null)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MissingGetterLargeSizeValue, attributeSyntax.GetLocation(), propertyDeclarationSyntax.Identifier));
            return false;
        }

        var getterLargeSizeValueOperation = semanticModel.GetOperation(getterLargeSizeValueNameSyntax.Expression);

        if (getterLargeSizeValueOperation is not INameOfOperation nameOfOperation)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MandatoryOfNameofInGetterLargeSizeValueName, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax.Identifier));
            return false;
        }

        var nameofArgument = nameOfOperation.Argument;


        var nameofArgumentSymbol = SemanticModelHelper.GetCandidateGetterLargeSizeValueNameSymbol(owner, nameOfOperation, targetPropertySymbol, propertyDeclarationSyntax, getterLargeSizeValueNameSyntax, attributeData, in diagnostics);

        if(nameofArgumentSymbol is null)
        {
            return false;
        }

        if (ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, propertyDeclarationSyntax, in diagnostics, out var parsed))
        {
            result = new ParsedExtendedBitFiledAttribute(parsed, nameofArgumentSymbol);
            return true;
        }

        return false;
    }

}
