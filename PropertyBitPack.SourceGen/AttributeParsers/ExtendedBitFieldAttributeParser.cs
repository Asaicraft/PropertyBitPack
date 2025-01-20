using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Text;

namespace PropertyBitPack.SourceGen.AttributeParsers;

/// <summary>
/// Parses attributes annotated with <see cref="ExtendedBitFieldAttribute"/>.
/// </summary>
/// <remarks>
/// This parser validates and extracts relevant information from properties annotated with 
/// <see cref="ExtendedBitFieldAttribute"/>. It ensures proper diagnostics are reported for missing 
/// or invalid configurations, such as type mismatches or missing large size value references.
/// </remarks>
internal sealed class ExtendedBitFieldAttributeParser : BaseAttributeParser
{
    private static readonly Type[] _validSymbolsTypes = [typeof(IFieldSymbol), typeof(IPropertySymbol), typeof(IMethodSymbol)];

    /// <summary>
    /// Determines if the specified attribute is a candidate for parsing.
    /// </summary>
    /// <param name="attributeData">The attribute data to evaluate.</param>
    /// <param name="attributeSyntax">The syntax representation of the attribute.</param>
    /// <returns><c>true</c> if the attribute implements <see cref="IExtendedBitFieldAttribute"/>; otherwise, <c>false</c>.</returns>
    public override bool IsCandidate(AttributeData attributeData, AttributeSyntax attributeSyntax)
    {
        return HasInterface<IExtendedBitFieldAttribute>(attributeData);
    }

    /// <summary>
    /// Attempts to parse the given attribute and populate the result with its data.
    /// </summary>
    /// <param name="attributeData">The attribute data to parse.</param>
    /// <param name="attributeSyntax">The syntax representation of the attribute.</param>
    /// <param name="propertyDeclarationSyntax">The syntax representation of the property being parsed.</param>
    /// <param name="semanticModel">The semantic model used for analysis.</param>
    /// <param name="diagnostics">
    /// A collection of <see cref="Diagnostic"/> instances to which any parsing errors are added.
    /// </param>
    /// <param name="result">The parsed result, or <c>null</c> if parsing fails.</param>
    /// <returns>
    /// <c>true</c> if the attribute was successfully parsed; otherwise, <c>false</c>.
    /// </returns>
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

        if (!TryGetterLargeSizeValueSymbol(semanticModel, propertyDeclarationSyntax, attributeData, attributeSyntax, owner, in diagnostics, out var getterLargeSizeValueSymbol))
        {
            return false;
        }

        result = new ParsedExtendedBitFiledAttribute(attributeSyntax, attributeData, fieldName, bitsCount, getterLargeSizeValueSymbol);

        return true;
    }

    /// <summary>
    /// Attempts to resolve the symbol for the large size value referenced in the attribute.
    /// </summary>
    /// <param name="semanticModel">The semantic model used for analysis.</param>
    /// <param name="propertySyntax">The property being analyzed.</param>
    /// <param name="attributeData">The attribute data being parsed.</param>
    /// <param name="attributeSyntax">The syntax representation of the attribute.</param>
    /// <param name="owner">The owner type of the property.</param>
    /// <param name="diagnostics">
    /// A collection of <see cref="Diagnostic"/> instances to which any parsing errors are added.
    /// </param>
    /// <param name="getterLargeSizeValueSymbol">The resolved symbol for the large size value.</param>
    /// <returns>
    /// <c>true</c> if the large size value symbol was successfully resolved; otherwise, <c>false</c>.
    /// </returns>
    private bool TryGetterLargeSizeValueSymbol(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax, AttributeData attributeData, AttributeSyntax attributeSyntax, INamedTypeSymbol owner, ref readonly  ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out ISymbol? getterLargeSizeValueSymbol)
    {
        getterLargeSizeValueSymbol = null;

        var argument = GetAttributeArgument(attributeSyntax, nameof(IExtendedBitFieldAttribute.GetterLargeSizeValueName));

        // for diagnostics
        var propertyName = propertySyntax.Identifier.Text;

        if (argument is null)
        {
            var diagnostic = Diagnostic.Create(
                PropertyBitPackDiagnostics.MissingGetterLargeSizeValue,
                attributeSyntax.GetLocation(),
                propertySyntax.Identifier.Text
            );

            diagnostics.Add(diagnostic);
            return false;
        }

        var nameOfOperation = semanticModel.GetOperation(argument.Expression);

        if (nameOfOperation is not INameOfOperation nameOf)
        {
            var diagnostic = Diagnostic.Create(
                PropertyBitPackDiagnostics.MandatoryOfNameofInGetterLargeSizeValueName,
                argument.GetLocation(),
                propertyName
            );

            diagnostics.Add(diagnostic);

            return false;
        }

        var validSymbolsTypes = _validSymbolsTypes.AsSpan();

        getterLargeSizeValueSymbol = ExtractSymbolFromNameOf(owner, nameOf, validSymbolsTypes);

        // Maybe we should use Unsafe.As<IPropertySymbol?>(getterLargeSizeValueSymbol) instead of casting?
        var propertySymbol = (IPropertySymbol?)semanticModel.GetDeclaredSymbol(propertySyntax);


        return ValidateAndReport(getterLargeSizeValueSymbol, propertySymbol, propertySyntax, argument, in diagnostics);
    }

    /// <summary>
    /// Validates the resolved large size value symbol against the property type and reports diagnostics if invalid.
    /// </summary>
    /// <param name="getterLargeValueSymbol">The symbol for the large size value.</param>
    /// <param name="propertySymbol">The property symbol being validated.</param>
    /// <param name="propertyDeclarationSyntax">The syntax representation of the property.</param>
    /// <param name="getterLargeValueSyntax">The syntax representation of the large size value reference.</param>
    /// <param name="diagnostics">
    /// A collection of <see cref="Diagnostic"/> instances to which any validation errors are added.
    /// </param>
    /// <returns>
    /// <c>true</c> if validation succeeds; otherwise, <c>false</c>.
    /// </returns>
    private bool ValidateAndReport(ISymbol? getterLargeValueSymbol, IPropertySymbol? propertySymbol, PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeArgumentSyntax getterLargeValueSyntax, ref readonly ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        if (getterLargeValueSymbol is null)
        {
            return false;
        }

        if (propertySymbol is null)
        {
            // This should never happen, but just in case
            return false;
        }

        var getterLargeValueLocation = getterLargeValueSyntax.GetLocation();

        if (getterLargeValueSymbol is IPropertySymbol property)
        {
            if (!SymbolEqualityComparer.Default.Equals(property.Type, propertySymbol.Type))
            {
                var diagnostic = Diagnostic.Create(
                    PropertyBitPackDiagnostics.InvalidTypeForPropertyReference,
                    getterLargeValueLocation,
                    property.Name,
                    propertySymbol.Type
                );

                diagnostics.Add(diagnostic);
                
                return false;
            }
        }
        else if (getterLargeValueSymbol is IMethodSymbol method)
        {
            if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, propertySymbol.Type))
            {
                var diagnostic = Diagnostic.Create(
                    PropertyBitPackDiagnostics.InvalidReturnTypeForMethodReference,
                    getterLargeValueLocation,
                    method.Name,
                    propertySymbol.Type,
                    method.ReturnType
                );
                diagnostics.Add(diagnostic);
                return false;
            }
        }
        else if(getterLargeValueSymbol is IFieldSymbol fieldSymbol)
        {
            if(!SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, propertySymbol.Type))
            {
                // I know this is a bit confusing, but sometimes we use a property to reference a field
                // thats why we report it as a property not a field (because we doesn't have DiagnosticDescriptor for fields)

                // TODO: Add a new DiagnosticDescriptor for fields
                var diagnostic = Diagnostic.Create(
                    PropertyBitPackDiagnostics.InvalidTypeForPropertyReference,
                    getterLargeValueLocation,
                    fieldSymbol.Name,
                    propertySymbol.Type
                );
                diagnostics.Add(diagnostic);
                return false;
            }
        }
        else
        {
            var diagnostic = Diagnostic.Create(
                PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName,
                propertySymbol.Locations[0],
                propertySymbol.Name
            );
            diagnostics.Add(diagnostic);
            return false;
        }

        return true;

    }
}
