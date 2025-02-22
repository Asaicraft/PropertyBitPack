using PropertyBitPack.SourceGen.Models.FieldRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal abstract class GenerateSourceRequest: IGenerateSourceRequest
{
    public abstract ImmutableArray<IFieldRequest> Fields { get; }

    public abstract ImmutableArray<BitFieldPropertyInfoRequest> Properties { get; }

    private string GetDebuggerDisplay()
    {
        var totalBits = Properties.Sum(static x => x.BitsSpan.Length);
        var totalCapacity = Fields.Sum(static x => BitCountHelper.GetBitsCountForSpecialType(x.FieldType));
        var capacities = string.Join(
            ", ",
            Fields.Select(static x => BitCountHelper.GetBitsCountForSpecialType(x.FieldType))
        );


        return $"Fields: {Fields.Length}, Properties: {Properties.Length}, Total Bits: {totalBits}, Total Capacity: {totalCapacity}, Capacities: [{capacities}]";
    }


    [Conditional("DEBUG")]
    public virtual void FullDebugWriteLine()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(GetDebuggerDisplay());

        stringBuilder.AppendLine("Fields:");
        stringBuilder.AppendLine();
        foreach (var field in Fields)
        {
            stringBuilder.Append("\t");
            stringBuilder.AppendLine(field.ToString());
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Properties:");
        stringBuilder.AppendLine();

        foreach (var property in Properties)
        {
            stringBuilder.Append("\t");
            stringBuilder.AppendLine(property.ToString());
        }

        Debug.WriteLine(stringBuilder.ToString());
    }
}
