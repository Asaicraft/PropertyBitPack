using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        return HasInterface(attributeData, IReadOnlyBitFieldAttribute);
    }

    public override bool TryParse(AttributeData attributeData, PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics, [NotNullWhen(true)] out AttributeParsedResult? result)
    {
        throw new NotImplementedException();
    }
}
