using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
internal interface IGenerateSourceRequest
{
    public ImmutableArray<IFieldRequest> Fields { get; }

    public ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; }
}
