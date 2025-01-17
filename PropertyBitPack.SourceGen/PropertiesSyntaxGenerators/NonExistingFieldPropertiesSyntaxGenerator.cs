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

/// <summary>
/// Generates source code for properties and fields when the fields do not exist in the target type.
/// </summary>
/// <remarks>
/// This generator processes requests of type <see cref="NonExistingFieldGsr"/> by creating syntax nodes 
/// for fields that need to be introduced into the target type. Properties are assumed to exist and are 
/// implemented via partial class declarations. The generated source files include normalized whitespace 
/// and additional members when required.
/// </remarks>
internal sealed class NonExistingFieldPropertiesSyntaxGenerator : BasePropertiesSyntaxGenerator
{
    protected override void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder)
    {
        ImmutableArray<NonExistingFieldGsr> candidateRequests;

        using (var candidateRequestsBuilder = ImmutableArrayBuilder<NonExistingFieldGsr>.Rent())
        {
            foreach (var candidateRequest in requests)
            {
                if (candidateRequest is NonExistingFieldGsr nonExistingFieldGsr)
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

            if (!additionalMembers.IsDefaultOrEmpty)
            {
                additionalMembersList.AddRange(additionalMembers);
            }
        }

        using var membersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var members = membersRented.List;

        members.AddRange(fields);
        members.AddRange(properties);
        members.AddRange(additionalMembersList);

        var unit = GenerateCompilationUnit(nonExistingFieldGsr, members);

        return unit.NormalizeWhitespace().GetText(Encoding.UTF8);
    }
}
