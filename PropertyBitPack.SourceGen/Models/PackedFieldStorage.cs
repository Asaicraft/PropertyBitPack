using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

/// <summary>
/// Represents a storage unit for bit-packed properties within a single field.
/// This class aggregates information about the field name, its type, and the properties stored in it.
/// </summary>
/// <param name="FieldName">
/// The name of the backing field used to store the bit-packed data for the associated properties.
/// </param>
/// <param name="TypeSyntax">
/// The syntax node representing the type of the backing field (e.g., <c>uint</c>, <c>ulong</c>).
/// </param>
/// <param name="TypeBitsCount">
/// The total number of bits available in the backing field type (e.g., 32 for <c>uint</c>, 64 for <c>ulong</c>).
/// </param>
/// <param name="StoredBitsCount">
/// The total number of bits actually used by the properties in this field.
/// </param>
/// <param name="PropertiesWhichDataStored">
/// A collection of properties whose data is stored within this backing field.
/// Each property has its own bit allocation, which is part of the total bits in the field.
/// </param>
/// <param name="Owner">
/// The type symbol representing the class or struct that owns these properties.
/// </param>
public sealed record PackedFieldStorage(
    string FieldName,
    TypeSyntax TypeSyntax,
    int TypeBitsCount,
    int StoredBitsCount,
    ImmutableArray<PropertyToBitInfo> PropertiesWhichDataStored,
    ITypeSymbol Owner
);