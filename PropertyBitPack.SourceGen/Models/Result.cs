using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value, null);
    public static Result<T> Failure<T>(ImmutableArray<Diagnostic> diagnostics) => new(default, diagnostics);
    public static Result<T> Null<T>() => new(default, null);
}

public readonly record struct Result<T>( T? Value, ImmutableArray<Diagnostic>? Diagnostics)
{
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(ImmutableArray<Diagnostic> diagnostics) => new(default, diagnostics);

    [MemberNotNullWhen(false, nameof(Value))]
    public readonly bool IsEmpty => Value is null;

    [MemberNotNullWhen(true, nameof(Diagnostics))]
    public readonly bool IsError => Diagnostics?.Any(x => x.Severity == DiagnosticSeverity.Error) ?? false;
}
