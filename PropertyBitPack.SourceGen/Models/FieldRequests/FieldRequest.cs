using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.FieldRequests;

/// <summary>
/// Represents a specific implementation of <see cref="IFieldRequest"/> for field-related operations.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class FieldRequest : IFieldRequest
{
    private readonly string _name;
    private readonly SpecialType _fieldType;
    private readonly bool _isExist;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldRequest"/> class with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <param name="fieldType">The type of the field as a <see cref="SpecialType"/>.</param>
    /// <param name="isExist">Indicates whether the field exists.</param>
    public FieldRequest(string name, SpecialType fieldType, bool isExist)
    {
        _name = name;
        _fieldType = fieldType;
        _isExist = isExist;
    }

    /// <inheritdoc/>
    public string Name => _name;

    /// <inheritdoc/>
    public SpecialType FieldType => _fieldType;

    /// <inheritdoc/>
    public bool IsExist => _isExist;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"IsExist:{_isExist} {_name} ({_fieldType})";
    }

    private string GetDebuggerDisplay() => ToString();
}