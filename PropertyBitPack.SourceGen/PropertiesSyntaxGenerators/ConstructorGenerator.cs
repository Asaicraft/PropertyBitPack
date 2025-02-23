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
            BitwiseSyntaxHelpers.GetAccessModifers(constructorGsr.ConstructorRequest.ConstructorAccessModifier),
            Identifier(owner.Name),
            GenerateParameterListSyntax(constructorRequest),
            null,
            GenerateBlockSyntax(constructorGsr, constructorRequest)
        );
    }

    /// <summary>
    /// Generates a constructor block that initializes bit-field properties based on constructor parameters.
    /// </summary>
    /// <param name="constructorGsr">The <see cref="ConstructorGsr"/> containing property information.</param>
    /// <param name="constructorRequest">The <see cref="IConstructorRequest"/> containing parameter details.</param>
    /// <returns>A <see cref="BlockSyntax"/> representing the constructor body.</returns>
    private BlockSyntax GenerateBlockSyntax(ConstructorGsr constructorGsr, IConstructorRequest constructorRequest)
    {
        // Here we will accumulate all expressions inside the constructor
        using var statementsRented = ListsPool.Rent<StatementSyntax>();
        var statements = statementsRented.List;

        // We need to find the corresponding BitFieldPropertyInfoRequest for each parameter.
        // Let's assume a 1:1 match by name: if a parameter has the name "flag1",
        // then the corresponding property (BitFieldPropertyInfoRequest) has propertySymbol.Name = "flag1".
        //
        // If you have more complex matching rules, replace this search logic with your own.

        for (var i = 0; i < constructorRequest.ParamRequests.Length; i++)
        {
            var paramRequest = constructorRequest.ParamRequests[i];
            var paramName = paramRequest.Name;

            // Find the property whose propertySymbol.Name matches the parameter name
            BitFieldPropertyInfoRequest? propertyInfo = null;
            for (var propertyIndex = 0; propertyIndex < constructorGsr.Properties.Length; propertyIndex++)
            {
                var property = constructorGsr.Properties[propertyIndex];
                if (property.PropertySymbol.Name == paramName)
                {
                    propertyInfo = property;
                    break;
                }
            }

            if (propertyInfo is null)
            {
                continue;
            }

            var propertyGenerator = FindPropertySyntaxGenerator(constructorGsr, propertyInfo);

            if (propertyGenerator is not BasePropertySyntaxGenerator basePropertySyntaxGenerator)
            {
                continue;
            }

            // Extract all necessary information from propertyInfo
            var fieldType = propertyInfo.BitsSpan.FieldRequest.FieldType;
            var fieldName = propertyInfo.BitsSpan.FieldRequest.Name;
            var start = propertyInfo.BitsSpan.Start;
            var length = propertyInfo.BitsSpan.Length;

            var setterBlock = basePropertySyntaxGenerator.SetterBlockSyntax(
                propertyInfo,
                valueVariableName: paramName,
                maxValueVariableName: $"max{paramName}_",
                clampedValueVariableName: $"clamped{paramName}_"
            );

            statements.Add(setterBlock);
        }

        // Finally, return the completed Block(...)
        return Block(statements);
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
