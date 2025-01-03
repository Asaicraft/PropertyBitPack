using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal interface IFieldName
{

    /// <summary>
    /// The name of the field.
    /// Can be null if the field is unnamed.
    /// </summary>
    public string? Name
    {
        get;
    }

    /// <summary>
    /// Indicates whether the field is linked to an existing symbol.
    /// If true, both <see cref="ExistingSymbol"/> and <see cref="Name"/> are guaranteed to be non-null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExistingSymbol))]
    [MemberNotNullWhen(true, nameof(Name))]
    public bool IsSymbolExist
    {
        get;
    }

    /// <summary>
    /// The existing symbol associated with the field, if any.
    /// Can be null if no symbol is associated.
    /// </summary>
    public ISymbol? ExistingSymbol
    {
        get;
    }

}
