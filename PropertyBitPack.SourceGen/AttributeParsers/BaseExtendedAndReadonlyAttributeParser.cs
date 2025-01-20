using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.AttributeParsers;
internal abstract class BaseExtendedAndReadonlyAttributeParser : BaseAttributeParser
{
    private static readonly ImmutableArray<Type> _validGetterLargeValueSymbolTypes = [typeof(IFieldSymbol), typeof(IPropertySymbol), typeof(IMethodSymbol)];

    protected virtual ReadOnlySpan<Type> ValidGetterLargeValueSymbolTypes => _validGetterLargeValueSymbolTypes.AsSpan();

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
    protected virtual bool TryGetterLargeSizeValueSymbol(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax, AttributeData attributeData, AttributeSyntax attributeSyntax, INamedTypeSymbol owner, ref readonly ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out ISymbol? getterLargeSizeValueSymbol)
    {
        getterLargeSizeValueSymbol = null;

        if (!HasInterface<IExtendedBitFieldAttribute>(attributeData))
        {
            // Return early if the attribute does not implement IExtendedBitFieldAttribute
            // and we have nothing to parse and no diagnostics to report.
            return false;
        }

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

        var validSymbolsTypes = ValidGetterLargeValueSymbolTypes;

        getterLargeSizeValueSymbol = ExtractSymbolFromNameOf(owner, nameOf, validSymbolsTypes);

        // Maybe we should use Unsafe.As<IPropertySymbol?>(getterLargeSizeValueSymbol) instead of casting?
        var propertySymbol = (IPropertySymbol?)semanticModel.GetDeclaredSymbol(propertySyntax);


        return ValidateGetterLargeSizeValueAndReport(getterLargeSizeValueSymbol, propertySymbol, propertySyntax, argument, in diagnostics);
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
    protected virtual bool ValidateGetterLargeSizeValueAndReport(ISymbol? getterLargeValueSymbol, IPropertySymbol? propertySymbol, PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeArgumentSyntax getterLargeValueSyntax, ref readonly ImmutableArrayBuilder<Diagnostic> diagnostics)
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
        else if (getterLargeValueSymbol is IFieldSymbol fieldSymbol)
        {
            if (!SymbolEqualityComparer.Default.Equals(fieldSymbol.Type, propertySymbol.Type))
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

    protected virtual bool TryGetConstructorAccessModifier(SemanticModel semanticModel, PropertyDeclarationSyntax propertySyntax, AttributeData attributeData, AttributeSyntax attributeSyntax, INamedTypeSymbol owner, ref readonly ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AccessModifier? accessModifier)
    {
        const string ConstructorAccessModifier = nameof(ReadOnlyBitFieldAttribute.ConstructorAccessModifier);

        // Default value for the access modifier.
        // If parsing fails, we should return null.
        accessModifier = ReadOnlyBitFieldAttribute.DefaultConstructorAccessModifier;

        if (!HasInterface<IReadOnlyBitFieldAttribute>(attributeData))
        {
            // Return early if the attribute does not implement IReadOnlyBitFieldAttribute.
            // We have nothing to parse and no diagnostics to report.
            // Also, we should return null instead of the default value.
            accessModifier = null;
            return false;
        }

        var argumentSyntax = GetAttributeArgument(attributeSyntax, ConstructorAccessModifier);

        if (argumentSyntax is null)
        {
            // If the argument is missing, it means the default value should be used.
            // This is not an error.
            return true;
        }

        if (!TryGetConstantValue<AccessModifier>(attributeData, ConstructorAccessModifier, out var candidateAccessModifier))
        {
            // How is it possible to have a null value here?
            // AttributeArgumentSyntax was found, but not in the attribute data.
            ThrowHelper.ThrowUnreachableException("AttributeArgumentSyntax found, but not in data.");
        }

        accessModifier = candidateAccessModifier;
        return true;
    }
}
