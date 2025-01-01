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

        builder.AttributeParsers.AddFirst(new ParsedBitFieldAttributeParser());
        
        _context = builder.Build();
    }

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

                    return new object();
                }
            )
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(propertiesWithAttributes, static (context, properties) =>
        {
            var diagnosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();


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
