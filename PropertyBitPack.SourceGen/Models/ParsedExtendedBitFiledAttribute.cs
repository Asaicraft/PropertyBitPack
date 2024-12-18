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
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MissingGetterLargeSizeValue, attributeSyntax.GetLocation(), propertyDeclarationSyntax));
            return false;
        }

        var getterLargeSizeValueOperation = semanticModel.GetOperation(getterLargeSizeValueNameSyntax.Expression);

        if (getterLargeSizeValueOperation is not INameOfOperation nameOfOperation)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MandatoryOfNameofInGetterLargeSizeValueName, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax));
            return false;
        }

        var nameofArgument = nameOfOperation.Argument;

        if (nameofArgument is not IPropertyReferenceOperation ||
            nameofArgument is not IMethodReferenceOperation)
        {
            diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax));
            return false;
        }

        if (nameofArgument is IPropertyReferenceOperation propertyReference)
        {
            var propertySymbol = propertyReference.Property;

            if (!targetPropertySymbol.Type.Equals(propertySymbol.Type, SymbolEqualityComparer.Default))
            {
                diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidTypeForPropertyReference, getterLargeSizeValueNameSyntax.GetLocation(), propertyDeclarationSyntax, targetPropertySymbol.Type.ToDisplayString()));
                return false;
            }

            if (ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, out var parsed))
            {
                result = new ParsedExtendedBitFiledAttribute(parsed, propertySymbol);
                return true;
            }
            else
            {
                return false;
            }
        }

        if(nameofArgument is IMethodReferenceOperation methodReference)
        {
            var methodSymbol = methodReference.Method;

            if(!methodSymbol.ReturnType.Equals(targetPropertySymbol.Type, SymbolEqualityComparer.Default))
            {
                diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidReturnTypeForMethodReference, getterLargeSizeValueNameSyntax.GetLocation(), methodSymbol.Name, targetPropertySymbol.Type.ToDisplayString()));
                return false;
            }

            var isOnlyDefaultParameters = true;
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (!parameter.HasExplicitDefaultValue)
                {
                    isOnlyDefaultParameters = false;
                    break;
                }
            }

            if(!isOnlyDefaultParameters)
            {
                diagnostics.Add(Diagnostic.Create(PropertyBitPackDiagnostics.MethodWithParametersNotAllowed, getterLargeSizeValueNameSyntax.GetLocation(), methodSymbol.Name));
                return false;
            }

            if (ParsedBitFiledAttribute.TryParseBitFieldAttribute(attributeData, out var parsed))
            {
                result = new ParsedExtendedBitFiledAttribute(parsed, methodSymbol);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

}
