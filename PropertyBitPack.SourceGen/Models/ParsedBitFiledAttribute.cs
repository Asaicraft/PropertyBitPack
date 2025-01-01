using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.Models;
public sealed class ParsedBitFiledAttribute : AttributeParsedResult
{
    public ParsedBitFiledAttribute(IFieldName? fieldName, int? bitsCount): base(fieldName, bitsCount)
    {
    }
}
