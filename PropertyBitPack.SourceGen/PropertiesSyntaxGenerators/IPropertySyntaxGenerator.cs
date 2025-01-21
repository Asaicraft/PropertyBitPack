using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Interface for generating property declarations based on bit field information and source requests.
/// </summary>
internal interface IPropertySyntaxGenerator
{
    /// <summary>
    /// Generates a property declaration based on the provided source request and bit field property information.
    /// </summary>
    /// <param name="sourceRequest">The source request containing field and property mapping information.</param>
    /// <param name="bitFieldPropertyInfoRequest">The bit field property information request.</param>
    /// <param name="additionalMember">
    /// Outputs additional member declarations (e.g., helper methods or fields) required for the property.
    /// </param>
    /// <returns>
    /// A <see cref="PropertyDeclarationSyntax"/> representing the generated property, or <c>null</c> if generation is not applicable.
    /// </returns>
    public PropertyDeclarationSyntax? GenerateProperty(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember);
}
