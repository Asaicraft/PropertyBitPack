using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
internal interface IPropertySyntaxGenerator
{
    public PropertyDeclarationSyntax? GenerateProperty(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember);
}
