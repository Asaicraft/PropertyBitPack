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
        : base(BitsMappingAttributeType.ExtendedBitField, attributeParsedResult.BitsCount, attributeParsedResult.FieldName)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
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
        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            result = null;
            return false;
        }

        if (attributeSyntax.Parent?.Parent is not PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            ThrowHelper.ThrowUnreachableException();
            result = null;
            return false;
        }

        // It's impossible to have an ExtendedBitField attribute without an argument list.
        if (attributeSyntax.ArgumentList is null)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MissingGetterLargeSizeValue, attributeSyntax.GetLocation(), propertyDeclarationSyntax));
            result = null;
            return false;
        }

        var getterLargeSizeValueNameSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault(x => x.NameEquals?.Name.Identifier.Text == ExtendedBitFieldAttributeGetterLargeSizeValueName);

        // It's impossible to have an ExtendedBitField attribute without a getterLargeSizeValue argument. Because it's property is required.
        if (getterLargeSizeValueNameSyntax is null)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MissingGetterLargeSizeValue, attributeSyntax.GetLocation(), propertyDeclarationSyntax));
            result = null;
            return false;
        }

        var getterLargeSizeValueOperation = semanticModel.GetOperation(getterLargeSizeValueNameSyntax.Expression);

        if (getterLargeSizeValueOperation is not INameOfOperation nameOfOperation)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MandatoryOfNameofInGetterLargeSizeValueName, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax));
            result = null;
            return false;
        }

        var nameofArgument = nameOfOperation.Argument;

        if (nameofArgument is not IPropertyReferenceOperation ||
            nameofArgument is not IMethodReferenceOperation)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax));
            result = null;
            return false;
        }

        if (nameofArgument is IPropertyReferenceOperation propertyReference)
        {
            var propertySymbol = propertyReference.Property;



            if (ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, out var parsed))
            {
                result = new ParsedExtendedBitFiledAttribute(parsed, propertySymbol);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        if (nameofArgument is IMethod)
    }
}
