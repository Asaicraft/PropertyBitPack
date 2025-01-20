namespace PropertyBitPack.SourceGen.Models;

/// <summary>
/// Represents the parsed data of a read-only bit field attribute.
/// </summary>
/// <remarks>
/// This interface provides access to the properties parsed from attributes 
/// that describe read-only bit fields.
/// </remarks>
internal interface IParsedReadOnlyBitFieldAttribute : IAttributeParsedResult
{
    /// <summary>
    /// Gets the access modifier for the constructor of the read-only bit field.
    /// </summary>
    /// <value>
    /// The <see cref="AccessModifier"/> specifying the visibility of the constructor.
    /// </value>
    public AccessModifier ConstructorAccessModifier { get; }
}