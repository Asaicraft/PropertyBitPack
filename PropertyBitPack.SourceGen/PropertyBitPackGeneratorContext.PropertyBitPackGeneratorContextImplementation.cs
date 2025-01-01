using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
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

        public override ImmutableArray<IBitFieldPropertyAggregator> BitFieldPropertyAggregators
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
            ImmutableArray<IBitFieldPropertyAggregator> bitFieldPropertyAggregators,
            ImmutableArray<IPropertiesSyntaxGenerator> bitFieldPropertyGenerators
            )
        {
            AttributeParsers = attributeParsers;
            BitFieldPropertyParsers = bitFieldPropertyParsers;
            BitFieldPropertyAggregators = bitFieldPropertyAggregators;
            PropertySyntaxGenerators = bitFieldPropertyGenerators;

            BindToSelf();
        }

        internal void BindToSelf()
        {
            for (var i = 0; i < AttributeParsers.Length; i++)
            {
                var parser = AttributeParsers[i];

                if(parser is IContextBindable contextBindable)
                {
                    contextBindable.BindContext(this);
                }
            }

            for (var i = 0; i < BitFieldPropertyParsers.Length; i++)
            {
                var parser = BitFieldPropertyParsers[i];
                if (parser is IContextBindable contextBindable)
                {
                    contextBindable.BindContext(this);
                }
            }

            for (var i = 0; i < BitFieldPropertyAggregators.Length; i++)
            {
                var aggregator = BitFieldPropertyAggregators[i];
                if (aggregator is IContextBindable contextBindable)
                {
                    contextBindable.BindContext(this);
                }
            }

            for (var i = 0; i < PropertySyntaxGenerators.Length; i++)
            {
                var generator = PropertySyntaxGenerators[i];
                if (generator is IContextBindable contextBindable)
                {
                    contextBindable.BindContext(this);
                }
            }
        }
    }
}
