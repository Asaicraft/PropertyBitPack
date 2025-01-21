using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Generates source code for properties and fields when the fields do not exist in the target type.
/// </summary>
/// <remarks>
/// This generator processes requests of type <see cref="NonExistingFieldGsr"/> by creating syntax nodes 
/// for fields that need to be introduced into the target type. Properties are assumed to exist and are 
/// implemented via partial class declarations. The generated source files include normalized whitespace 
/// and additional members when required.
/// </remarks>
internal sealed class NonExistingFieldPropertiesSyntaxGenerator : BasePropertiesSyntaxGenerator
{
    protected override void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> fileGeneratorRequestsBuilder)
    {
        var candidateRequests = FilterCandidates<NonExistingFieldGsr>(requests);

        ProccessCandidates(requests, candidateRequests, fileGeneratorRequestsBuilder);
    }
}
