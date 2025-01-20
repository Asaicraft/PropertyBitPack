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
internal abstract class AttributeParsedResult(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, byte? bitsCount) : IAttributeParsedResult
{
    public AttributeSyntax AttributeSyntax
    {
        get;
    } = attributeSyntax;

    public AttributeData AttributeData
    {
        get;
    } = attributeData;

    public IFieldName? FieldName
    {
        get;
    } = fieldName;


    public byte? BitsCount
    {
        get;
    } = bitsCount;
}