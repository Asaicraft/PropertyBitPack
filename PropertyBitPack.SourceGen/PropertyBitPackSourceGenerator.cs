﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using PropertyBitPack.SourceGen.Models;
using System.Collections.Immutable;
using PropertyBitPack.SourceGen.Collections;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using PropertyBitPack.SourceGen.AttributeParsers;

namespace PropertyBitPack.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class PropertyBitPackSourceGenerator : IIncrementalGenerator
{
    private static readonly PropertyBitPackGeneratorContext _context;

    static PropertyBitPackSourceGenerator()
    {
        var builder = PropertyBitPackGeneratorContextBuilder.Create();

        builder.AttributeParsers.Add(new ReadOnlyBitFieldAttributeParser());
        builder.AttributeParsers.Add(new ParsedBitFieldAttributeParser());

        _context = builder.Build();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertiesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsCandidateProperty(syntaxNode),
                transform: static Result<BaseBitFieldPropertyInfo>? (context, cancellationToken) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

                    var semanticModel = context.SemanticModel;

                    using var diagnosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

                    var attributeLists = propertyDeclaration.AttributeLists;


                    using var attributeDatas = ImmutableArrayBuilder<AttributeData>.Rent();


                    if (semanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
                    {
                        return null;
                    }

                    var attributes = propertySymbol.GetAttributes();

                    var candidates = GetCandidadates(attributes);

                    if (candidates.Length == 0)
                    {
                        return null;
                    }

                    var attributeData = candidates[0];

                    if (candidates.Length != 1)
                    {
                        diagnosticsBuilder.Add(
                            Diagnostic.Create(
                                PropertyBitPackDiagnostics.AttributeConflict,
                                propertyDeclaration.GetLocation(),
                                string.Join(
                                    ", ",
                                    candidates.Select(x => x.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                                )
                            )
                        );

                        return Result.Failure<BaseBitFieldPropertyInfo>(diagnosticsBuilder.ToImmutable());
                    }

                    var bitFieldPropertyInfo = _context.ParseBitFieldProperty(propertyDeclaration, semanticModel, in diagnosticsBuilder);
                    var diagnostics = diagnosticsBuilder.ToImmutable();

                    ImmutableArray<Diagnostic>? nullableDiagnostics = diagnostics.IsDefaultOrEmpty ? null : diagnostics;

                    return new(bitFieldPropertyInfo, nullableDiagnostics);



                    static ImmutableArray<AttributeData> GetCandidadates(ImmutableArray<AttributeData> attributeDatas)
                    {
                        using var candidates = ImmutableArrayBuilder<AttributeData>.Rent();

                        for (var i = 0; i < attributeDatas.Length; i++)
                        {
                            var attributeData = attributeDatas[i];

                            if (_context.IsCandidateAttribute(attributeData))
                            {
                                candidates.Add(attributeData);
                            }
                        }

                        return candidates.ToImmutable();
                    }
                }
            )
            .Where(x => x is not null)
            .Select((x, _) => (Result<BaseBitFieldPropertyInfo>)x!)
            .Collect();
        
        context.RegisterSourceOutput(propertiesWithAttributes, static (context, results) =>
        {
            using var diagnosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

            var bitFieldPropertyInfos = ValidateAndAccumulateProperties(results, in diagnosticsBuilder);
            var bitFieldPropertyInfoList = new SimpleLinkedList<BaseBitFieldPropertyInfo>(bitFieldPropertyInfos);

            var aggregatedBitFieldProperties = _context.AggregateBitFieldProperties(bitFieldPropertyInfoList);

            var generateSourceRequests = new SimpleLinkedList<GenerateSourceRequest>(aggregatedBitFieldProperties);

            var generatedPropertySyntax = _context.GeneratePropertySyntax(generateSourceRequests);

            foreach (var generatedProperty in generatedPropertySyntax)
            {
                context.AddSource(generatedProperty.FileName, generatedProperty.SourceText);
            }

        showDiagnostics:
            if (diagnosticsBuilder.Count > 0)
            {
                var diagnostics = diagnosticsBuilder.ToImmutable();

                foreach (var diagnostic in diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                    Debug.WriteLine(diagnostic);
                }
            }
            return;

            // This method validates and accumulates property-attribute pairs from the given candidates.
            // Parameters:
            // - ImmutableArray<Result<PropertyAttributePair>> candidates: A collection of candidates to validate.
            // - ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder: A builder for accumulating diagnostics during validation.
            // Returns:
            // - ImmutableArray<PropertyAttributePair>: An immutable array containing valid property-attribute pairs.
            static ImmutableArray<BaseBitFieldPropertyInfo> ValidateAndAccumulateProperties(ImmutableArray<Result<BaseBitFieldPropertyInfo>> candidates, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
            {
                // Create a temporary builder for storing valid property-attribute pairs.
                using var properties = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent();

                // Iterate through all the candidates.
                for (var i = 0; i < candidates.Length; i++)
                {
                    var candidate = candidates[i];

                    // If the candidate has errors, add the associated diagnostics to the diagnosticsBuilder.
                    if (candidate.IsError)
                    {
                        diagnosticsBuilder.AddRange(candidate.Diagnostics.Value.AsSpan());
                        continue;
                    }

                    // Skip candidates where the property declaration is not a valid PropertyDeclarationSyntax.
                    if (candidate.Value is not BaseBitFieldPropertyInfo bitFieldPropertyInfo)
                    {
                        continue;
                    }

                    // Add the valid candidate to the properties builder.
                    properties.Add(candidate.Value);
                }

                // Convert the builder to an immutable array and return the result.
                return properties.ToImmutable();
            }
        });
    }

    private static bool IsCandidateProperty(SyntaxNode syntaxNode)
    {
        if (Guard.ContainsErrors(syntaxNode))
        {
            return false;
        }

        return syntaxNode is PropertyDeclarationSyntax propertyDeclaration
            && propertyDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
            && propertyDeclaration.AttributeLists.Count > 0
            && propertyDeclaration.AttributeLists.Any(static attrbiuteList => IsCandidateAttribute(attrbiuteList));

        static bool IsCandidateAttribute(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any();
        }
    }
}
