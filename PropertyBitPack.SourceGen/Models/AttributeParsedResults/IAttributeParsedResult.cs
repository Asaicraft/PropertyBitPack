using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PropertyBitPack.SourceGen.Models.AttributeParsedResults;

/// <summary>
/// Represents the result of parsing an attribute.
/// </summary>
/// <remarks>
/// This interface standardizes the properties that store data parsed from attributes, 
/// including information about the attribute's syntax, associated data, and additional metadata.
/// </remarks>
internal interface IAttributeParsedResult
{
    /// <summary>
    /// Gets the <see cref="AttributeData"/> representing the parsed attribute metadata.
    /// </summary>
    public AttributeData AttributeData { get; }

    /// <summary>
    /// Gets the <see cref="AttributeSyntax"/> representing the syntax node of the parsed attribute.
    /// </summary>
    public AttributeSyntax AttributeSyntax { get; }

    /// <summary>
    /// Gets the number of bits specified by the parsed attribute, if applicable.
    /// </summary>
    /// <value>
    /// The number of bits as a nullable <see cref="byte"/>, or <c>null</c> if not specified.
    /// </value>
    public byte? BitsCount { get; }

    /// <summary>
    /// Gets the field name specified by the parsed attribute, if applicable.
    /// </summary>
    /// <value>
    /// The <see cref="IFieldName"/> representing the field name, or <c>null</c> if not specified.
    /// </value>
    public IFieldName? FieldName { get; }
}