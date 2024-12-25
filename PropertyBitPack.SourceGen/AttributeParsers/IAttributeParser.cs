using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.AttributeParsers;
/// <summary>
/// Defines the contract for attribute parsers used by the property bit pack generator.
/// Each parser can check whether it is a candidate for a given attribute and
/// attempt to parse that attribute into a model (e.g., <see cref="AttributeParsedResult"/>).
/// </summary>
public interface IAttributeParser
{
    /// <summary>
    /// Gets a value indicating whether this parser should be attempted
    /// if a previous candidate parser failed to parse the same attribute.
    /// Typically used to provide fallback or alternative parsing logic.
    /// </summary>
    public bool FallbackOnCandidateFailure { get; }

    /// <summary>
    /// Determines if the current parser recognizes and can handle the specified attribute.
    /// </summary>
    /// <param name="attributeData">The attribute data to evaluate.</param>
    /// <returns>
    /// <c>true</c> if this parser is a valid candidate for <paramref name="attributeData"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsCandidate(AttributeData attributeData);

    /// <summary>
    /// Attempts to parse the specified attribute and produce a model representing its data.
    /// If the parsing is successful, the result is assigned to <paramref name="result"/>.
    /// </summary>
    /// <param name="attributeData">The attribute data to parse.</param>
    /// <param name="propertyDeclarationSyntax">The property syntax node associated with the attribute.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> for analyzing the attribute.</param>
    /// <param name="diagnostics">
    /// A builder for collecting <see cref="Diagnostic"/> instances
    /// encountered during parsing.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the parsed attribute result if parsing succeeded; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the attribute was successfully parsed; otherwise, <c>false</c>.
    /// </returns>
    public bool TryParse(
        AttributeData attributeData,
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out AttributeParsedResult? result);
}