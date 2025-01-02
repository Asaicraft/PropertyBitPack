using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PropertyBitPack.SourceGen.Models;

internal readonly record struct PropertyAttributePair(PropertyDeclarationSyntax PropertyDeclaration, AttributeData AttributeData)
{
    public static implicit operator (PropertyDeclarationSyntax, AttributeData)(PropertyAttributePair value)
    {
        return (value.PropertyDeclaration, value.AttributeData);
    }

    public static implicit operator PropertyAttributePair((PropertyDeclarationSyntax, AttributeData) value)
    {
        return new PropertyAttributePair(value.Item1, value.Item2);
    }
}