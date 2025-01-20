using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
using PropertyBitPack.SourceGen.BitFieldPropertyParsers;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequest;

namespace PropertyBitPack.SourceGen;
internal abstract partial class PropertyBitPackGeneratorContext
{
    public abstract ImmutableArray<IAttributeParser> AttributeParsers
    {
        get;
    }

    public abstract ImmutableArray<IBitFieldPropertyParser> BitFieldPropertyParsers
    {
        get;
    }

    public abstract ImmutableArray<IBitFieldPropertyAggregator> BitFieldPropertyAggregators
    {
        get;
    }

    public abstract ImmutableArray<IPropertiesSyntaxGenerator> PropertySyntaxGenerators
    {
        get;
    }

    public virtual bool IsCandidateAttribute(AttributeData attributeData, AttributeSyntax attributeSyntax)
    {
        for (var i = 0; i < AttributeParsers.Length; i++)
        {
            var parser = AttributeParsers[i];
            if (parser.IsCandidate(attributeData, attributeSyntax))
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool TryParseAttribute(
        AttributeData attributeData,
        AttributeSyntax attributeSyntax,
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out IAttributeParsedResult? result)
    {


        for (var i = 0; i < AttributeParsers.Length; i++)
        {
            var parser = AttributeParsers[i];
            if (parser.IsCandidate(attributeData, attributeSyntax))
            {
                if (parser.TryParse(attributeData, attributeSyntax, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
                {
                    return true;
                }
                else
                {
                    for (var j = i + 1; i > AttributeParsers.Length; j++)
                    {
                        var nextParser = AttributeParsers[j];

                        if (!nextParser.FallbackOnCandidateFailure)
                        {
                            continue;
                        }

                        if (!nextParser.IsCandidate(attributeData, attributeSyntax))
                        {
                            continue;
                        }

                        if (nextParser.TryParse(attributeData, attributeSyntax, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        result = null;


        return false;
    }

    public virtual BaseBitFieldPropertyInfo? ParseBitFieldProperty(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        for (var i = 0; i < BitFieldPropertyParsers.Length; i++)
        {
            var parser = BitFieldPropertyParsers[i];
            var result = parser.Parse(propertyDeclarationSyntax, candidateAttribute, attributeSyntax, semanticModel, diagnostics);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    public virtual ImmutableArray<GenerateSourceRequest> AggregateBitFieldProperties(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        var requests = ImmutableArrayBuilder<GenerateSourceRequest>.Rent();
        for (var i = 0; i < BitFieldPropertyAggregators.Length; i++)
        {
            var aggregator = BitFieldPropertyAggregators[i];
            var aggregatedRequests = aggregator.Aggregate(properties, diagnostics);
            requests.AddRange(aggregatedRequests.AsSpan());
        }
        return requests.ToImmutable();
    }

    public virtual ImmutableArray<FileGeneratorRequest> GeneratePropertySyntax(ILinkedList<GenerateSourceRequest> requests)
    {
        var sourceTexts = ImmutableArrayBuilder<FileGeneratorRequest>.Rent();

        for (var i = 0; i < PropertySyntaxGenerators.Length; i++)
        {
            var generator = PropertySyntaxGenerators[i];
            var generatedTexts = generator.Generate(requests);
            sourceTexts.AddRange(generatedTexts.AsSpan());
        }

        return sourceTexts.ToImmutable();
    }
}