using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal interface IFieldName
{

    public string Name
    {
        get;
    }

    /// <summary>
    /// If for BitsMappingAttributeBase.FieldName used nameof
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExistingSymbol))]
    public bool IsSymbolExist
    {
        get;
    }


    public ISymbol? ExistingSymbol
    {
        get;
    }

}
