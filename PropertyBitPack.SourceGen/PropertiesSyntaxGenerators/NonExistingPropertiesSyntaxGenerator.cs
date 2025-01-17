using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;
internal sealed class NonExistingPropertiesSyntaxGenerator : BasePropertiesSyntaxGenerator
{
    protected override void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder)
    {
        ImmutableArray<NonExistingFieldGsr> candidateRequests;

        using (var candidateRequestsBuilder = ImmutableArrayBuilder<NonExistingFieldGsr>.Rent())
        {
            foreach (var candidateRequest in requests)
            {
                if(candidateRequest is NonExistingFieldGsr nonExistingFieldGsr)
                {
                    candidateRequestsBuilder.Add(nonExistingFieldGsr);
                    continue;
                }
            }

            candidateRequests = candidateRequestsBuilder.ToImmutable();
        }

        for (var i = 0; i < candidateRequests.Length; i++)
        {
            var candidateRequest = candidateRequests[i];

            var sourceText = Generate(candidateRequest);
            var fileName = GetFileName(candidateRequest);

            requests.Remove(candidateRequest);

            immutableArrayBuilder.Add(new FileGeneratorRequest(sourceText, fileName));
        }

    }

    private SourceText Generate(NonExistingFieldGsr nonExistingFieldGsr)
    {
        using var fieldsRented = ListsPool.Rent<FieldDeclarationSyntax>();
        var fields = fieldsRented.List;

        foreach (var field in nonExistingFieldGsr.NonExistingFieldRequests)
        {
            fields.Add(GenerateField(nonExistingFieldGsr, field));
        }

        using var propertiesRented = ListsPool.Rent<PropertyDeclarationSyntax>();
        var properties = propertiesRented.List;

        using var additionalMembersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var additionalMembersList = additionalMembersRented.List;

        for (var i = 0; i < nonExistingFieldGsr.Properties.Length; i++)
        {
            var propertyRequest = nonExistingFieldGsr.Properties[i];

            properties.Add(GenerateProperty(nonExistingFieldGsr, propertyRequest, out var additionalMembers));

            additionalMembersList.AddRange(additionalMembers);
        }

        using var membersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var members = membersRented.List;

        members.AddRange(fields);
        members.AddRange(properties);
        members.AddRange(additionalMembersList);

        var unit = GenerateCompilationUnit(nonExistingFieldGsr, members);

        return unit.NormalizeWhitespace().GetText();
    }
}
