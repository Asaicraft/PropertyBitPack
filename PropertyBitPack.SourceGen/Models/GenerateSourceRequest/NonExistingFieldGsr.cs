using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
internal abstract class NonExistingFieldGsr : GenerateSourceRequest
{
    public abstract ImmutableArray<NonExistingFieldRequest> NonExistingFieldRequests { get; }

    public sealed override ImmutableArray<FieldRequest> Fields
    {
        get
        {
            var nonExistingFieldRequests = NonExistingFieldRequests;
            var fields = Unsafe.As<ImmutableArray<NonExistingFieldRequest>, ImmutableArray<FieldRequest>>(ref nonExistingFieldRequests);

            return fields;
        }
    }
}
