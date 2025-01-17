using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Generates source code for properties related to existing fields in the target type.
/// </summary>
/// <remarks>
/// This generator processes requests of type <see cref="ExistingFieldGsr"/> and creates the corresponding 
/// source code files. It iterates through the properties and generates the necessary syntax nodes, 
/// including properties and additional members, using the provided base methods.
/// </remarks>
internal sealed class ExistingFieldPropertiesGenerator: BasePropertiesSyntaxGenerator
{
    protected override void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder)
    {
        var candidateRequests = FilterCandidates<ExistingFieldGsr>(requests);

        for (var i = 0; i < candidateRequests.Length; i++)
        {
            var candidateRequest = candidateRequests[i];

            var sourceText = Generate(candidateRequest);
            var fileName = GetFileName(candidateRequest);

            requests.Remove(candidateRequest);

            immutableArrayBuilder.Add(new FileGeneratorRequest(sourceText, fileName));
        }

    }

    private SourceText Generate(ExistingFieldGsr nonExistingFieldGsr)
    {
        using var propertiesRented = ListsPool.Rent<PropertyDeclarationSyntax>();
        var properties = propertiesRented.List;

        using var additionalMembersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var additionalMembersList = additionalMembersRented.List;

        for (var i = 0; i < nonExistingFieldGsr.Properties.Length; i++)
        {
            var propertyRequest = nonExistingFieldGsr.Properties[i];

            properties.Add(GenerateProperty(nonExistingFieldGsr, propertyRequest, out var additionalMembers));

            if (!additionalMembers.IsDefaultOrEmpty)
            {
                additionalMembersList.AddRange(additionalMembers);
            }
        }

        using var membersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var members = membersRented.List;

        members.AddRange(properties);
        members.AddRange(additionalMembersList);

        var unit = GenerateCompilationUnit(nonExistingFieldGsr, members);

        return unit.NormalizeWhitespace().GetText(Encoding.UTF8);
    }
}
