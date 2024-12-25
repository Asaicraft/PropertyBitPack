using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
public interface IPropertiesSyntaxGenerator
{
    /// <summary>
    /// Remove properties in <paramref name="properties"/> which aggregated
    /// </summary>
    public ImmutableArray<SourceText> Generate(in ImmutableArrayBuilder<GenerateSourceRequest> request);

}
