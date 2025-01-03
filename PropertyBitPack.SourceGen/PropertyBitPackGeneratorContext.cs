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

namespace PropertyBitPack.SourceGen;
public abstract partial class PropertyBitPackGeneratorContext
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

    public virtual bool IsCandidateAttribute(AttributeData attributeData)
    {
        for (var i = 0; i < AttributeParsers.Length; i++)
        {
            var parser = AttributeParsers[i];
            if (parser.IsCandidate(attributeData))
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool TryParseAttribute(
        AttributeData attributeData,
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out AttributeParsedResult? result)
    {


        for(var i = 0; i < AttributeParsers.Length; i++)
        {
            var parser = AttributeParsers[i];
            if (parser.IsCandidate(attributeData))
            {
                if (parser.TryParse(attributeData, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
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

                        if(!nextParser.IsCandidate(attributeData))
                        {
                            continue;
                        }

                        if (nextParser.TryParse(attributeData, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
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
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        for (var i = 0; i < BitFieldPropertyParsers.Length; i++)
        {
            var parser = BitFieldPropertyParsers[i];
            var result = parser.Parse(propertyDeclarationSyntax, candidateAttribute, semanticModel, diagnostics);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    public virtual ImmutableArray<GenerateSourceRequest> AggregateBitFieldProperties(
        ILinkedList<BaseBitFieldPropertyInfo> properties)
    {
        var requests = ImmutableArrayBuilder<GenerateSourceRequest>.Rent();
        for (var i = 0; i < BitFieldPropertyAggregators.Length; i++)
        {
            var aggregator = BitFieldPropertyAggregators[i];
            var aggregatedRequests = aggregator.Aggregate(properties);
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