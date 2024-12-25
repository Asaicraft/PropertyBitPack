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
public abstract class AttributeParsedResult(string? fieldName, int bitsCount)
{
    public string? FieldName
    {
        get;
    } = fieldName;

    public int? BitsCount
    {
        get;
    } = bitsCount;

}