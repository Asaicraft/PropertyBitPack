using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
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
internal sealed class ExtendedBitFieldAttributeParser : BaseExtendedAndReadonlyAttributeParser
{
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
    public override bool TryParse(AttributeData attributeData, AttributeSyntax attributeSyntax, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out IAttributeParsedResult? result)
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

    
}
