using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.BitFieldPropertyParsers;
using PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
using System.Collections.Immutable;

namespace PropertyBitPack.SourceGen;
public abstract partial class PropertyBitPackGeneratorContext
{
    internal sealed class PropertyBitPackGeneratorContextImplementation : PropertyBitPackGeneratorContext
    {
        public override ImmutableArray<IAttributeParser> AttributeParsers
        {
            get;
        }
        public override ImmutableArray<IBitFieldPropertyParser> BitFieldPropertyParsers
        {
            get;
        }
        public override ImmutableArray<IPropertiesSyntaxGenerator> PropertySyntaxGenerators
        {
            get;
        }
        public PropertyBitPackGeneratorContextImplementation(
            ImmutableArray<IAttributeParser> attributeParsers,
            ImmutableArray<IBitFieldPropertyParser> bitFieldPropertyParsers,
            ImmutableArray<IPropertiesSyntaxGenerator> bitFieldPropertyGenerators)
        {
            AttributeParsers = attributeParsers;
            BitFieldPropertyParsers = bitFieldPropertyParsers;
            PropertySyntaxGenerators = bitFieldPropertyGenerators;
        }
    }
}
