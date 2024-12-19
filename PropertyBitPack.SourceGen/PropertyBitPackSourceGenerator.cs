using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using PropertyBitPack.SourceGen.Models;
using System.Collections.Immutable;
using PropertyBitPack.SourceGen.Collections;

namespace PropertyBitPack.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class PropertyBitPackSourceGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertiesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsCandidateProperty(syntaxNode),
                transform: static (context, cancellationToken) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return default;
                    }

                    return PropertyToBitInfoProccessor.Process(context, cancellationToken);
                }
            )
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(propertiesWithAttributes, static (context, properties) =>
        {
            ImmutableArray<PropertyToBitInfo> filteredResults = default;

            using (var diagnosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent())
            {
                filteredResults = FilterAndCollectDiagnostics(properties, diagnosticsBuilder);

                if (diagnosticsBuilder.Count > 0)
                {
                    var diagnostics = diagnosticsBuilder.ToImmutable();

                    foreach (var diagnostic in diagnostics)
                    {
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            if (filteredResults.IsEmpty)
            {
                return;
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

    private static ImmutableArray<PropertyToBitInfo> FilterAndCollectDiagnostics(ImmutableArray<Result<PropertyToBitInfo>?> results, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
    {
        using var validatedResults = ImmutableArrayBuilder<PropertyToBitInfo>.Rent(results.Length / 2);

        foreach (var result in results)
        {
            if (result is not Result<PropertyToBitInfo> notNullResult)
            {
                continue;
            }

            if (notNullResult.Diagnostics is ImmutableArray<Diagnostic> diagnostics)
            {
                diagnosticsBuilder.AddRange(diagnostics.AsSpan());
            }

            if (notNullResult.IsError)
            {
                continue;
            }

            if (notNullResult.IsEmpty)
            {
                continue;
            }

            validatedResults.Add(notNullResult.Value);
        }

        return validatedResults.ToImmutable();
    }

}
