using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
public abstract class AttributeParsedResult
{

    public AttributeParsedResult(string? fieldName, int bitsCount)
    {
        FieldName = fieldName;
        BitsCount = bitsCount;
    }

    public string? FieldName
    {
        get;
    }

    public int? BitsCount
    {
        get;
    }

}