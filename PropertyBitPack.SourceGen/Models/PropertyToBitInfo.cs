using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

/// <summary>
/// Represents detailed information about a property annotated with a bit-mapping attribute.
/// This record is used during the source generation process to collect and store
/// all necessary metadata for generating bit-packed code for the property.
/// </summary>
/// <param name="AttributeType">The type of the attribute applied to the property (e.g., BitField or ExtendedBitField).</param>
/// <param name="GetterLargeSizeValueSymbol">
/// A symbol representing the method or property to retrieve large values for ExtendedBitField attributes.
/// Null for simple BitField attributes.
/// </param>
/// <param name="IsInit">
/// Is init or setter property. If true, the property is an init-only property.
/// </param>
/// <param name="SetterOrInitModifiers">
/// A list of modifiers for the setter or initializer of the property, if applicable (e.g., readonly, private, etc.).
/// </param>
/// <param name="PropertySymbol">The symbol representing the target property in the source code.</param>
/// <param name="BitsCount">The number of bits allocated to this property in the bit-packed field.</param>
/// <param name="FieldName">The name of the backing field used to store the bit-packed value.</param>
/// <param name="PropertyType">The type of the property (e.g., int, bool).</param>
/// <param name="Owner">The type symbol representing the class or struct that owns this property.</param>
public sealed record PropertyToBitInfo(
    BitsMappingAttributeType AttributeType,
    ISymbol? GetterLargeSizeValueSymbol,
    bool IsInit,
    SyntaxTokenList SetterOrInitModifiers,
    IPropertySymbol PropertySymbol,
    int BitsCount,
    string? FieldName,
    ITypeSymbol PropertyType,
    ITypeSymbol Owner
);