using Microsoft.CodeAnalysis;
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
                transform: static Result<PropertyDeclarationSyntax>? (context, cancellationToken) =>
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

                        return Result.Failure<PropertyDeclarationSyntax>(diagnosticsBuilder.ToImmutable());
                    }

                    return Result.Success(propertyDeclaration);



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
            .Select((x, _) => (Result<PropertyDeclarationSyntax>)x!)
            .Collect();

        context.RegisterSourceOutput(propertiesWithAttributes, static (context, results) =>
        {
            using var diagnosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();
            var properties = ValidateAndReport(results, in diagnosticsBuilder);


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

            static ImmutableArray<PropertyDeclarationSyntax> ValidateAndReport(ImmutableArray<Result<PropertyDeclarationSyntax>> candidates, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
            {
                using var properties = ImmutableArrayBuilder<PropertyDeclarationSyntax>.Rent();

                for (var i = 0; i < candidates.Length; i++)
                {
                    var candidate = candidates[i];

                    if (candidate.IsError)
                    {
                        diagnosticsBuilder.AddRange(candidate.Diagnostics.Value.AsSpan());
                        continue;
                    }

                    if (candidate.Value is not PropertyDeclarationSyntax propertyDeclaration)
                    {
                        continue;
                    }

                    properties.Add(candidate.Value);
                }

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
