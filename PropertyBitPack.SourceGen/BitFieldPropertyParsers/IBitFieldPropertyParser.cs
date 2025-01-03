using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyParsers;
internal interface IBitFieldPropertyParser
{

    public bool IsCandidate(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        SemanticModel semanticModel);

    public BaseBitFieldPropertyInfo? Parse(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics);
}
