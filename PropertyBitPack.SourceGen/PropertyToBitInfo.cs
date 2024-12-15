using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen;
public sealed class PropertyToBitInfo
{
    public required PropertyDeclarationSyntax PropertyDeclaration
    {
        get; init;
    }

    public required bool IsInit
    {
        get; init;
    }

    /// <summary>
    /// May be property(<see cref="IPropertySymbol"/>) or method(<see cref="IMethodSymbol"/>)
    /// </summary>
    public required ISymbol? GetterLargeSizeValueSymbol
    {
        get; init;
    }

    public required SyntaxTokenList SetterOrInitModifiers
    {
        get; init;
    }

    public required IPropertySymbol PropertySymbol
    {
        get; init;
    }

    public required int? BitsCount
    {
        get; init;
    }

    public required string? FieldName
    {
        get; init;
    }

    public required ITypeSymbol PropertyType
    {
        get; init;
    }

    public required ITypeSymbol Owner
    {
        get; init;
    }

    public required bool IsSmallBits
    {
        get; init;
    }

    public required string? GetterLargeSizeValueName
    {
        get; init;
    }

    public required ImmutableArray<Diagnostic> Diagnostics
    {
        get; init;
    }

    public bool IsError => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
