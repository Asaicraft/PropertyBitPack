using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;


[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class SimpleGenerateSourceRequest(ImmutableArray<FieldRequest> fields, ImmutableArray<BitFieldPropertyInfoRequest> properties) : GenerateSourceRequest
{
    public override ImmutableArray<FieldRequest> Fields { get; } = fields;
    public override ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; } = properties;

    private string GetDebuggerDisplay()
    {
        return $"Fields: {Fields.Length}, Properties: {Properties.Length}";
    }
}
