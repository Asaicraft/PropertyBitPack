using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
using PropertyBitPack.SourceGen.BitFieldPropertyParsers;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public abstract class PropertyBitPackGeneratorContextBuilder
{
    public abstract ILinkedList<IAttributeParser> AttributeParsers
    {
        get;
    }

    public abstract ILinkedList<IBitFieldPropertyParser> BitFieldPropertyParsers
    {
        get;
    }

    public abstract ILinkedList<IBitFieldPropertyAggregator> BitFieldPropertyAggregators
    {
        get;
    }

    public abstract ILinkedList<IPropertiesSyntaxGenerator> PropertiesSyntaxGenerators
    {
        get;
    }


    public virtual PropertyBitPackGeneratorContext Build()
    {
        using var attributeParsersBuilder = ImmutableArrayBuilder<IAttributeParser>.Rent();
        using var bitFieldPropertyParsersBuilder = ImmutableArrayBuilder<IBitFieldPropertyParser>.Rent();
        using var bitFieldPropertyAggregatorsBuilder = ImmutableArrayBuilder<IBitFieldPropertyAggregator>.Rent();
        using var propertiesSyntaxGeneratorsBuilder = ImmutableArrayBuilder<IPropertiesSyntaxGenerator>.Rent();

        foreach (var attributeParser in AttributeParsers)
        {
            attributeParsersBuilder.Add(attributeParser);
        }

        foreach (var bitFieldPropertyParser in BitFieldPropertyParsers)
        {
            bitFieldPropertyParsersBuilder.Add(bitFieldPropertyParser);
        }

        foreach (var bitFieldPropertyAggregator in BitFieldPropertyAggregators)
        {
            bitFieldPropertyAggregatorsBuilder.Add(bitFieldPropertyAggregator);
        }

        foreach (var bitFieldPropertyGenerator in PropertiesSyntaxGenerators)
        {
            propertiesSyntaxGeneratorsBuilder.Add(bitFieldPropertyGenerator);
        }

        return new PropertyBitPackGeneratorContext.PropertyBitPackGeneratorContextImplementation(
            attributeParsersBuilder.ToImmutable(),
            bitFieldPropertyParsersBuilder.ToImmutable(),
            propertiesSyntaxGeneratorsBuilder.ToImmutable());
    }

    private sealed class PropertyBitPackGeneratorContextBuilderImplementation : PropertyBitPackGeneratorContextBuilder
    {
        public override ILinkedList<IAttributeParser> AttributeParsers
        {
            get;
        }

        public override ILinkedList<IBitFieldPropertyParser> BitFieldPropertyParsers
        {
            get;
        }

        public override ILinkedList<IBitFieldPropertyAggregator> BitFieldPropertyAggregators
        {
            get;
        }

        public override ILinkedList<IPropertiesSyntaxGenerator> PropertiesSyntaxGenerators
        {
            get;
        }

        public PropertyBitPackGeneratorContextBuilderImplementation()
        {
            AttributeParsers = new SimpleLinkedList<IAttributeParser>();
            BitFieldPropertyParsers = new SimpleLinkedList<IBitFieldPropertyParser>();
            BitFieldPropertyAggregators = new SimpleLinkedList<IBitFieldPropertyAggregator>();
            PropertiesSyntaxGenerators = new SimpleLinkedList<IPropertiesSyntaxGenerator>();
        }
    }
}
