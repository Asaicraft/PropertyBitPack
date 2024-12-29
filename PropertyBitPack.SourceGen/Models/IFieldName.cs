﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public interface IFieldName
{
    public string Name
    {
        get;
    }


    /// <summary>
    /// If for BitsMappingAttributeBase.FieldName used nameof
    /// </summary>
    public bool IsSymbolExist
    {
        get;
    }

    public ISymbol? ExistingSymbol
    {
        get;
    }
}