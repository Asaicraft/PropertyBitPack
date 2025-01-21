using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
internal sealed class ReadOnlyUnnamedFieldGsr(NonExistingFieldRequest fieldRequest, ImmutableArray<BitFieldPropertyInfoRequest> properties) : UnnamedFieldGsr(fieldRequest, properties)
{
}
