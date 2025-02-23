using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.ConstructorRequests;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
internal sealed class ConstructorGenerator : BasePropertiesSyntaxGenerator
{
    /// <inheritdoc/>
    protected override void GenerateCore(ILinkedList<IGenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder)
    {
        var candidateRequests = FilterCandidates<ConstructorGsr>(requests);

        for (var i = 0; i < candidateRequests.Length; i++)
        {
            var candidate = candidateRequests[i];

            ProccessCandidate(requests, candidate, immutableArrayBuilder);

            requests.Remove(candidate);
        }
    }

    private void ProccessCandidate(ILinkedList<IGenerateSourceRequest> requests, ConstructorGsr constructorGsr, in ImmutableArrayBuilder<FileGeneratorRequest> fileRequestBuilder)
    {
        var constructorDecloration = GenerateConstructorDecloration(constructorGsr);

        var compilationUnit = GenerateCompilationUnit(constructorGsr, [constructorDecloration]);

        var sourceText = compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8);

        fileRequestBuilder.Add(new FileGeneratorRequest(sourceText, GetFileName(constructorGsr)));
    }

    private ConstructorDeclarationSyntax GenerateConstructorDecloration(ConstructorGsr constructorGsr)
    {
        var owner = constructorGsr.Properties[0].Owner;
        var constructorRequest = constructorGsr.ConstructorRequest;

        return ConstructorDeclaration(
            List<AttributeListSyntax>(),
            TokenList(GetAccessModifer(constructorGsr.ConstructorRequest.ConstructorAccessModifier)),
            Identifier(owner.Name),
            GenerateParameterListSyntax(constructorRequest),
            null,
            GenerateBlockSyntax(constructorGsr, constructorRequest)
        );
    }

    private BlockSyntax GenerateBlockSyntax(ConstructorGsr constructorGsr, IConstructorRequest constructorRequest)
    {
        using var rentedStatements = ListsPool.Rent<StatementSyntax>();
        var statements = rentedStatements.List;

        throw new NotImplementedException();
    }

    private SyntaxToken GetAccessModifer(AccessModifier accessModifier)
    {
        return accessModifier switch
        {
            AccessModifier.Public => Token(SyntaxKind.PublicKeyword),
            AccessModifier.Protected => Token(SyntaxKind.ProtectedKeyword),
            AccessModifier.Internal => Token(SyntaxKind.InternalKeyword),
            AccessModifier.Private => Token(SyntaxKind.PrivateKeyword),
            _ => throw new ArgumentOutOfRangeException(nameof(accessModifier))
        };
    }

    private ParameterListSyntax GenerateParameterListSyntax(IConstructorRequest constructorRequest)
    {
        using var rentedParameters = ListsPool.Rent<ParameterSyntax>();
        var parameters = rentedParameters.List;

        for (var i = 0; i < constructorRequest.ParamRequests.Length; i++)
        {
            var paramterRequest = constructorRequest.ParamRequests[i];

            parameters.Add(
                Parameter(
                    List<AttributeListSyntax>(),
                    TokenList(),
                    BitwiseSyntaxHelpers.ToSignedVariantSyntax(paramterRequest.SpecialType),
                    Identifier(paramterRequest.Name),
                    null
                )
            );
        }

        return ParameterList(SeparatedList(parameters));
    }

}
