using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal static class IAttributeParsedResultExtensions
{
    public static AttributeArgumentSyntax? BitsCountArgument(this IAttributeParsedResult attributeParsedResult)
    {
        return attributeParsedResult.AttributeSyntax.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == nameof(attributeParsedResult.BitsCount));
    }

    public static AttributeArgumentSyntax? FieldNameArgument(this IAttributeParsedResult attributeParsedResult)
    {
        return attributeParsedResult.AttributeSyntax?.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == nameof(attributeParsedResult.FieldName));
    }
}
