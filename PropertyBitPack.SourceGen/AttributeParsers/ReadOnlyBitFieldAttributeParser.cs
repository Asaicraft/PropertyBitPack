using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.AttributeParsers;
internal sealed class ReadOnlyBitFieldAttributeParser : BaseExtendedAndReadonlyAttributeParser
{
    public override bool IsCandidate(AttributeData attributeData, AttributeSyntax attributeSyntax)
    {
        return HasInterface<IReadOnlyBitFieldAttribute>(attributeData);
    }

    public override bool TryParse(AttributeData attributeData, AttributeSyntax attributeSyntax, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out IAttributeParsedResult? result)
    {
        result = null;

        var owner = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax)?.ContainingType;

        if (!TryGetBitsCount(attributeData, out var bitsCount, propertyDeclarationSyntax, in diagnostics))
        {
            return false;
        }

        if (!TryGetFieldName(attributeData, out var fieldName, semanticModel, in diagnostics, propertyDeclarationSyntax, owner))
        {
            return false;
        }

        if (!TryGetConstructorAccessModifier(semanticModel, propertyDeclarationSyntax, attributeData, attributeSyntax, owner, in diagnostics, out var accessModifier))
        {
            accessModifier = AccessModifier.Default;
        }


        result = new ParsedReadOnlyBitFieldAttribute(attributeSyntax, attributeData, fieldName, bitsCount, accessModifier);
        return true;
    }

}
