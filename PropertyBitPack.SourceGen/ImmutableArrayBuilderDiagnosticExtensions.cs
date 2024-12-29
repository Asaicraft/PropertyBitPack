using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class ImmutableArrayBuilderDiagnosticExtensions
{
    public static bool HasError(this in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return diagnostics.WrittenSpan.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    public static bool HasWarning(this in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return diagnostics.WrittenSpan.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning);
    }

    public static bool HasInfo(this in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return diagnostics.WrittenSpan.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Info);
    }

    public static bool HasHidden(this in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return diagnostics.WrittenSpan.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Hidden);
    }

    public static bool HasNone(this in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return diagnostics.WrittenSpan.IsEmpty;
    }

    public static bool TryAdd(this in ImmutableArrayBuilder<Diagnostic> diagnostics, Diagnostic diagnostic)
    {
        if(diagnostics.IsDefault)
        {
            return false;
        }

        diagnostics.Add(diagnostic);
        return true;
    }
}
