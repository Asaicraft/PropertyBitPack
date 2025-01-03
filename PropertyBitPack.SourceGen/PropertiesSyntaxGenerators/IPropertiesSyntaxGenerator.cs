using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
internal interface IPropertiesSyntaxGenerator
{
    /// <summary>
    /// Remove properties in <paramref name="request"/> which aggregated
    /// </summary>
    public ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<GenerateSourceRequest> request);

}
