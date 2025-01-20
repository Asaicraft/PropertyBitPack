using Microsoft.CodeAnalysis;

namespace PropertyBitPack.SourceGen.Models.FieldRequests;

/// <summary>
/// Represents a request for a field, providing information about its type, existence, and name.
/// </summary>
internal interface IFieldRequest
{
    /// <summary>
    /// Gets the type of the field as a <see cref="SpecialType"/>.
    /// </summary>
    public SpecialType FieldType { get; }

    // <summary>
    /// Gets a value indicating whether the field exists.
    /// </summary>
    public bool IsExist { get; }

    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public string Name { get; }
}