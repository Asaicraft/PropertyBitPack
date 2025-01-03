using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.AttributeParsers;
public abstract class BaseAttributeParser : IAttributeParser, IContextBindable
{
    protected static readonly SymbolDisplayFormat SymbolDisplayFormatNameAndContainingTypesAndNamespaces = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private PropertyBitPackGeneratorContext? _context;

    public PropertyBitPackGeneratorContext Context
    {
        get
        {
            Debug.Assert(_context is not null);

            return _context!;
        }
    }

    public virtual bool FallbackOnCandidateFailure => false;

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        _context = context;
    }

    public abstract bool IsCandidate(AttributeData attributeData);

    public abstract bool TryParse(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result);

    /// <summary>
    /// Checks if the specified attribute class implements a given interface.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="interfaceFullName">The full name of the interface to check for.</param>
    /// <returns>True if the attribute class implements the specified interface; otherwise, false.</returns>
    protected static bool HasInterface(AttributeData attributeData, string interfaceFullName)
    {
        // Ensure the attribute class is not null.
        if (attributeData.AttributeClass is null)
        {
            return false;
        }

        // Retrieve all interfaces implemented by the attribute class.
        var interfaces = attributeData.AttributeClass.AllInterfaces;

        // Iterate through the interfaces and check for a match.
        for (var i = 0; i < interfaces.Length; i++)
        {
            var currentInterfaceName = GetSymbolFullName(interfaces[i]);

            if (currentInterfaceName == interfaceFullName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve the field name from the provided attribute data.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="fieldName">The retrieved FieldName object if successfully determined; otherwise, null.</param>
    /// <param name="semanticModel">An optional semantic model for resolving symbols.</param>
    /// <param name="diagnostics">An optional diagnostics builder for reporting issues.</param>
    /// <returns>True if the field name is successfully determined; false if the retrieval fails due to invalid data.</returns>
    protected static bool TryGetFieldName(AttributeData attributeData, out IFieldName? fieldName, SemanticModel? semanticModel = null, in ImmutableArrayBuilder<Diagnostic> diagnostics = default, PropertyDeclarationSyntax? propertyDeclarationSyntax = null, ITypeSymbol? owner = null)
    {
        fieldName = null;
        var syntax = attributeData.ApplicationSyntaxReference?.GetSyntax();

        // If the property declaration syntax is not provided, attempt to retrieve it.
        propertyDeclarationSyntax ??= GetPropertyDeclaration(attributeData);

        // If there is no syntax reference, attempt to extract the field name from named arguments.
        if (syntax is null)
        {
            if (attributeData.NamedArguments.FirstOrDefault(static arg => arg.Key == BitFieldAttributeFieldName).Value.Value is string validFieldName)
            {
                fieldName = new FieldName(validFieldName);
                return true;
            }
            return false;
        }

        // If the semantic model is not provided, we cannot proceed.
        if (semanticModel is null)
        {
            return false;
        }

        // Retrieve the specific attribute argument corresponding to the field name.
        var fieldNameArgument = GetAttributeArgument(attributeData, BitFieldAttributeFieldName);

        if (fieldNameArgument is null)
        {
            return false;
        }

        // Get the operation for the field name argument.
        var fieldNameOperation = semanticModel.GetOperation(fieldNameArgument.Expression);

        // Ensure the operation is a NameOf operation.
        if (fieldNameOperation is not INameOfOperation nameOfOperation)
        {
            return false;
        }

        // Extract the owner type of the field using the NameOf operation.
        owner ??= ExtractOwnerFromNameOfOperation(nameOfOperation);

        if (owner is null)
        {
            return false;
        }

        // Extract the field symbol from the NameOf operation and the owner.
        var fieldSymbol = ExtractSymbolFromNameOf<IFieldSymbol>(owner, nameOfOperation);

        if (fieldSymbol is not null)
        {
            fieldName = new FieldName(fieldSymbol);
            return true;
        }
        else
        {
            diagnostics.TryAdd(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidReferenceInFieldName, syntax.GetLocation(), propertyDeclarationSyntax?.Identifier.Text));
            return false;
        }
    }

    /// <summary>
    /// Attempts to retrieve the bits count from the provided attribute data.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="bitsCount">The retrieved bits count if successfully determined; otherwise, null.</param>
    /// <param name="diagnostics">An optional diagnostics builder for reporting issues.</param>
    /// <returns>True if the bits count is successfully determined; false if the retrieval fails due to invalid data.</returns>
    protected static bool TryGetBitsCount(AttributeData attributeData, out int? bitsCount, PropertyDeclarationSyntax? propertyDeclarationSyntax = null, in ImmutableArrayBuilder<Diagnostic> diagnostics = default)
    {
        bitsCount = null;

        // If the property declaration syntax is not provided, attempt to retrieve it.
        propertyDeclarationSyntax ??= GetPropertyDeclaration(attributeData);

        if (GetConstantValue(attributeData, BitFieldAttributeBitsCount) is not int validBitsCount)
        {
            ThrowHelper.ThrowUnreachableException($"{BitFieldAttribute}.{BitFieldAttributeBitsCount} must be of type int");
            return false;
        }

        if (validBitsCount < 1)
        {
            var bitsCountArgument = GetAttributeArgument(attributeData, BitFieldAttributeBitsCount);

            diagnostics.TryAdd(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidBitsCount, bitsCountArgument?.GetLocation(), propertyDeclarationSyntax?.Identifier.Text));
            return false;
        }

        if(validBitsCount >= 64)
        {
            var bitsCountArgument = GetAttributeArgument(attributeData, BitFieldAttributeBitsCount);

            diagnostics.TryAdd(Diagnostic.Create(PropertyBitPackDiagnostics.InvalidBitsCount, bitsCountArgument?.GetLocation(), propertyDeclarationSyntax?.Identifier.Text));
            return false;
        }

        bitsCount = validBitsCount;
        return true;
    }


    /// <summary>
    /// Retrieves the constant value associated with a named argument in the specified attribute data.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="argumentName">The name of the argument to retrieve.</param>
    /// <returns>The constant value of the argument, or null if not found.</returns>
    protected static object? GetConstantValue(AttributeData attributeData, string argumentName)
    {
        return attributeData.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Value;
    }

    /// <summary>
    /// Attempts to retrieve the constant value of a specific type associated with a named argument in the attribute data.
    /// </summary>
    /// <typeparam name="T">The expected type of the constant value.</typeparam>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="argumentName">The name of the argument to retrieve.</param>
    /// <param name="value">The retrieved constant value, or null if the retrieval fails.</param>
    /// <returns>True if the constant value was successfully retrieved and matches the expected type; otherwise, false.</returns>
    protected static bool TryGetConstantValue<T>(AttributeData attributeData, string argumentName, [NotNullWhen(true)] out T? value) where T : class
    {
        var constantValue = GetConstantValue(attributeData, argumentName);
        if (constantValue is T result)
        {
            value = result;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }


    #region Extraction Methods

    /// <summary>
    /// Extracts the owning type from a NameOf operation if it belongs to an attribute applied to a property.
    /// </summary>
    /// <param name="nameOfOperation">The NameOf operation to analyze.</param>
    /// <returns>The owning type symbol, or null if it cannot be determined.</returns>
    protected static ITypeSymbol? ExtractOwnerFromNameOfOperation(INameOfOperation nameOfOperation)
    {
        var syntax = nameOfOperation.Syntax;
        var semanticModel = nameOfOperation.SemanticModel;

        if (semanticModel is null)
        {
            return null;
        }

        if (syntax is not { Parent.Parent.Parent: AttributeListSyntax attributeListSyntax })
        {
            return null;
        }

        if (attributeListSyntax.Parent is not PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            return null;
        }

        var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);

        return propertySymbol?.ContainingType;
    }

    /// <summary>
    /// Extracts a symbol from a NameOf operation using optional type filters.
    /// </summary>
    /// <param name="nameOfOperation">The NameOf operation to analyze.</param>
    /// <param name="symbolTypes">An optional span of types to filter the extracted symbols.</param>
    /// <returns>The matching symbol, or null if not found.</returns>
    protected static ISymbol? ExtractSymbolFromNameOf(INameOfOperation nameOfOperation, ReadOnlySpan<Type> symbolTypes = default)
    {
        var owner = ExtractOwnerFromNameOfOperation(nameOfOperation);

        return owner is not null ? ExtractSymbolFromNameOf(owner, nameOfOperation, symbolTypes) : null;
    }

    /// <summary>
    /// Extracts a symbol from a NameOf operation within a specified owner and optional type filters.
    /// </summary>
    /// <param name="owner">The owning type symbol.</param>
    /// <param name="nameOfOperation">The NameOf operation to analyze.</param>
    /// <param name="symbolTypes">An optional span of types to filter the extracted symbols.</param>
    /// <returns>The matching symbol, or null if not found.</returns>
    protected static ISymbol? ExtractSymbolFromNameOf(ITypeSymbol owner, INameOfOperation nameOfOperation, ReadOnlySpan<Type> symbolTypes = default)
    {
        if (nameOfOperation.ConstantValue.Value is not string symbolName)
        {
            return null;
        }

        var candidates = owner.GetMembers(symbolName);

        if (candidates.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < candidates.Length; i++)
        {
            var symbol = candidates[i];

            if (symbolTypes.IsEmpty)
            {
                return symbol;
            }
            else
            {
                foreach (var symbolType in symbolTypes)
                {
                    if (symbolType.IsAssignableFrom(symbol.GetType()))
                    {
                        return symbol;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts all symbols from a NameOf operation within a specified owner and optional type filters.
    /// </summary>
    /// <param name="owner">The owning type symbol.</param>
    /// <param name="nameOfOperation">The NameOf operation to analyze.</param>
    /// <param name="symbolTypes">An optional span of types to filter the extracted symbols.</param>
    /// <returns>An immutable array of matching symbols.</returns>
    protected static ImmutableArray<ISymbol> ExtractsSymbolFromNameOf(ITypeSymbol owner, INameOfOperation nameOfOperation, ReadOnlySpan<Type> symbolTypes = default)
    {
        if (nameOfOperation.ConstantValue.Value is not string symbolName)
        {
            return [];
        }

        var candidates = owner.GetMembers(symbolName);

        if (candidates.Length == 0)
        {
            return [];
        }

        using var symbols = ImmutableArrayBuilder<ISymbol>.Rent(candidates.Length);

        for (var i = 0; i < candidates.Length; i++)
        {
            var symbol = candidates[i];

            if (symbolTypes.IsEmpty)
            {
                symbols.Add(candidates[i]);
            }
            else
            {
                foreach (var symbolType in symbolTypes)
                {
                    if (symbolType.IsAssignableFrom(symbol.GetType()))
                    {
                        symbols.Add(symbol);
                    }
                }
            }
        }

        return symbols.ToImmutable();
    }

    /// <summary>
    /// Extracts a specific symbol of type T from a NameOf operation within a specified owner.
    /// </summary>
    /// <typeparam name="T">The expected type of the symbol.</typeparam>
    /// <param name="owner">The owning type symbol.</param>
    /// <param name="nameOfOperation">The NameOf operation to analyze.</param>
    /// <returns>The matching symbol of type T, or default if not found.</returns>
    protected static T? ExtractSymbolFromNameOf<T>(ITypeSymbol owner, INameOfOperation nameOfOperation) where T : ISymbol
    {
        if (nameOfOperation.ConstantValue.Value is not string symbolName)
        {
            return default;
        }

        var candidates = owner.GetMembers(symbolName);

        if (candidates.Length == 0)
        {
            return default;
        }

        for (var i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] is T symbol)
            {
                return symbol;
            }
        }
        return default;
    }

    protected static AttributeSyntax ExtractAttributeSyntax(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeData attributeData)
    {
        return (attributeData.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax) ?? propertyDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).First(x => x.Name.ToString() == attributeData.AttributeClass?.Name);
    }

    #endregion


    /// <summary>
    /// Retrieves the full name of the attribute's class, including namespaces and containing types.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <returns>The full name of the attribute class, or null if it cannot be determined.</returns>
    protected static string? GetAttributeFullName(AttributeData attributeData)
    {
        return attributeData.AttributeClass?.ToDisplayString(SymbolDisplayFormatNameAndContainingTypesAndNamespaces);
    }

    /// <summary>
    /// Retrieves the full name of a symbol, including namespaces and containing types.
    /// </summary>
    /// <param name="symbol">The symbol to analyze.</param>
    /// <returns>The full name of the symbol.</returns>
    protected static string GetSymbolFullName(ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormatNameAndContainingTypesAndNamespaces);
    }

    /// <summary>
    /// Retrieves a specific attribute argument by its name from the provided attribute syntax.
    /// </summary>
    /// <param name="attributeSyntax">The syntax representation of the attribute.</param>
    /// <param name="argumentName">The name of the argument to retrieve.</param>
    /// <returns>The matching attribute argument syntax, or null if not found.</returns>
    protected static AttributeArgumentSyntax? GetAttributeArgument(AttributeSyntax attributeSyntax, string argumentName)
    {
        return attributeSyntax.ArgumentList?.Arguments.FirstOrDefault(x => x.NameEquals?.Name.Identifier.Text == argumentName);
    }

    /// <summary>
    /// Retrieves a specific attribute argument by its name from the provided attribute data.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="argumentName">The name of the argument to retrieve.</param>
    /// <returns>The matching attribute argument syntax, or null if not found.</returns>
    protected static AttributeArgumentSyntax? GetAttributeArgument(AttributeData attributeData, string argumentName)
    {
        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        return GetAttributeArgument(attributeSyntax, argumentName);
    }

    /// <summary>
    /// Retrieves the property declaration syntax associated with the specified attribute data.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <returns>The property declaration syntax, or null if it cannot be determined.</returns>
    protected static PropertyDeclarationSyntax? GetPropertyDeclaration(AttributeData attributeData)
    {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().FirstAncestorOrSelf<PropertyDeclarationSyntax>();
    }

    /// <summary>
    /// Checks if the specified attribute matches the given full name.
    /// </summary>
    /// <param name="attributeData">The attribute data to analyze.</param>
    /// <param name="attributeFullName">The full name of the attribute to compare against.</param>
    /// <returns>True if the attribute's full name matches the provided name; otherwise, false.</returns>
    protected static bool MatchesAttributeFullName(AttributeData attributeData, string attributeFullName)
    {
        return GetAttributeFullName(attributeData) == attributeFullName;
    }
}