using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
internal sealed class PropertySyntaxGenerator(PropertyBitPackGeneratorContext propertyBitPackGeneratorContext) : BasePropertySyntaxGenerator(propertyBitPackGeneratorContext)
{
    protected override bool IsCandidate(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        return true;
    }
}
