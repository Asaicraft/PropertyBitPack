using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public readonly record struct Result<T>( T? Value, ImmutableArray<Diagnostic>? Diagnostics)
{
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ImmutableArray<Diagnostic> diagnostics) => new(default, diagnostics);

    [MemberNotNullWhen(false, nameof(Value))]
    public readonly bool IsEmpty => Value is null;
    public readonly bool IsError => Diagnostics?.Any(x => x.Severity == DiagnosticSeverity.Error) ?? false;
}
