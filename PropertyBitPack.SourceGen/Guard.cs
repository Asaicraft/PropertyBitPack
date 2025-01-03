using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;

/// <summary>
/// Provides utility methods for validating syntax nodes and ensuring correctness in source generation.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Determines whether the specified <see cref="SyntaxNode"/> contains any errors.
    /// </summary>
    /// <param name="node">The syntax node to check for errors.</param>
    /// <returns>
    /// <see langword="true"/> if the syntax node contains at least one diagnostic with a severity of <see cref="DiagnosticSeverity.Error"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsErrors(SyntaxNode node)
        => node
            .GetDiagnostics()
            .Any(d => d.Severity == DiagnosticSeverity.Error);


    public static bool IsSupportedTypeForBitsCount(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Byte => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            _ => false
        };
    }

}