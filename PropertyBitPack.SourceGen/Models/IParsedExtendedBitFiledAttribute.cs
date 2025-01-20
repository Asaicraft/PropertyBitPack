using Microsoft.CodeAnalysis;

namespace PropertyBitPack.SourceGen.Models;

/// <summary>
/// Represents the parsed data of an extended bit field attribute.
/// </summary>
/// <remarks>
/// This interface provides access to the properties parsed from attributes 
/// that describe extended bit fields.
/// </remarks>
internal interface IParsedExtendedBitFiledAttribute : IAttributeParsedResult
{
    /// <summary>
    /// Gets the symbol representing the large size value associated with the attribute.
    /// </summary>
    /// <value>
    /// The <see cref="ISymbol"/> referencing the large size value used in the attribute.
    /// </value>
    public ISymbol SymbolGetterLargeSizeValue { get; }
}