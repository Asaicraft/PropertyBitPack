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
        ImmutableArray<GenerateSourceRequest> candidateRequests;

        using (var candidateRequestsBuilder = ImmutableArrayBuilder<GenerateSourceRequest>.Rent())
        {
            foreach (var candidateRequest in candidateRequests)
            {
                if(candidateRequest is not NonExistingFieldGsr)
                {
                    continue;
                }

                candidateRequestsBuilder.Add(candidateRequest);
            }

            candidateRequests = candidateRequestsBuilder.ToImmutable();
        }


    }
}
