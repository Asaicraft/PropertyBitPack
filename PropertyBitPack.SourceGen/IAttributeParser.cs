using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace PropertyBitPack.SourceGen;
public interface IAttributeParser
{
    /// <summary>
    /// Handle if the previous parser failed to parse the attribute.
    /// </summary>
    public bool HandleIfTryParseFalse
    {
        get;
    }

    public bool IsCandidate(AttributeData attributeData);

    public bool TryParse(
        AttributeData attributeData,
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out AttributeParsedResult? result);
}
