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
internal abstract class AttributeParsedResult(AttributeSyntax attributeSyntax, AttributeData attributeData, IFieldName? fieldName, int? bitsCount)
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


    public int? BitsCount
    {
        get;
    } = bitsCount;

    public AttributeArgumentSyntax? BitsCountArgument()
    {
        return AttributeSyntax.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "bitsCount");
    }
}