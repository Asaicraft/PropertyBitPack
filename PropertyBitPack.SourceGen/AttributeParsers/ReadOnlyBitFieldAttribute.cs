using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static PropertyBitPack.SourceGen.PropertyBitPackConsts;

namespace PropertyBitPack.SourceGen.AttributeParsers;
internal sealed class ReadOnlyBitFieldAttributeParser : BaseAttributeParser
{
    public override bool IsCandidate(AttributeData attributeData)
    {
        return HasInterface<IReadOnlyBitFieldAttribute>(attributeData);
    }

    public override bool TryParse(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
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

        if(!TryGetConstantValue<AccessModifier>(attributeData, nameof(IReadOnlyBitFieldAttribute.ConstructorAccessModifier), out var accessModifier))
        {
            accessModifier = AccessModifier.Default;
        }


        result = new ParsedReadOnlyBitFieldAttribute(fieldName, bitsCount, accessModifier);
        return true;
    }

}
