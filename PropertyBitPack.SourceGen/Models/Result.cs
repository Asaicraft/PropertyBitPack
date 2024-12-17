using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public sealed record Result<T>(T? Value, ImmutableArray<Diagnostic>? Diagnostics)
{
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ImmutableArray<Diagnostic> diagnostics) => new(default, diagnostics);

    public bool IsEmpty => Value is null;
    public bool IsError => Diagnostics?.Any(x => x.Severity == DiagnosticSeverity.Error) ?? false;
}
