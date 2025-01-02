using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyParsers;
public interface IBitFieldPropertyParser
{
    public BaseBitFieldPropertyInfo? Parse(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics);
}
