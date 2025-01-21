using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
internal sealed class ReadOnlyExistingFieldGsr(IFieldSymbol fieldSymbol, ImmutableArray<BitFieldPropertyInfoRequest> bitFieldPropertyInfoRequests) : ExistingFieldGsr(fieldSymbol, bitFieldPropertyInfoRequests)
{

}
