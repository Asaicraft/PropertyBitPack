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
/// Defines the contract for generating syntax for properties based on source generation requests.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for processing source generation requests
/// and producing file generator requests, excluding aggregated properties.
/// </remarks>
internal interface IPropertiesSyntaxGenerator
{
    /// <summary>
    /// Generates file generator requests based on the provided source generation requests,
    /// excluding aggregated properties.
    /// </summary>
    /// <param name="requests">The list of source generation requests to process.</param>
    /// <returns>
    /// An <see cref="ImmutableArray{T}"/> of <see cref="FileGeneratorRequest"/> objects representing the generated requests.
    /// </returns>
    public ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<GenerateSourceRequest> requests);
}
