using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Generates source code for properties related to existing fields in the target type.
/// </summary>
/// <remarks>
/// This generator processes requests of type <see cref="ExistingFieldGsr"/> and creates the corresponding 
/// source code files. It iterates through the properties and generates the necessary syntax nodes, 
/// including properties and additional members, using the provided base methods.
/// </remarks>
internal sealed class ExistingFieldPropertiesGenerator: BasePropertiesSyntaxGenerator
{
    protected override void GenerateCore(ILinkedList<IGenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder)
    {
        var candidateRequests = FilterCandidates<ExistingFieldGsr>(requests);

        ProccessCandidates(requests, candidateRequests, immutableArrayBuilder);
    }
}
