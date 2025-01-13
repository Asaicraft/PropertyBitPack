using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class FieldName(string? name) : IFieldName
{
    private readonly string? _name = name;
    private readonly IFieldSymbol? _fieldSymbol;

    public FieldName(IFieldSymbol fieldSymbol) : this(fieldSymbol.Name)
    {
        _fieldSymbol = fieldSymbol;
    }

    public string? Name => _name;
    public bool IsSymbolExist => _fieldSymbol != null;
    public IFieldSymbol? ExistingSymbol => _fieldSymbol;

    private string GetDebuggerDisplay()
    {
        return IsSymbolExist ? $"{Name} (Symbol:{ExistingSymbol?.Kind.ToString() ?? "null"})" : Name ?? "<unnamed>";
    }
}
