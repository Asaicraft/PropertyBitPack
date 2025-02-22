using PropertyBitPack.SourceGen.Models.FieldRequests;

namespace PropertyBitPack.SourceGen.Models;

internal record struct FieldsPropertiesPair(List<IFieldRequest> Fields, List<BitFieldPropertyInfoRequest> Properties)
{
    public static implicit operator (List<IFieldRequest>, List<BitFieldPropertyInfoRequest>)(FieldsPropertiesPair value)
    {
        return (value.Fields, value.Properties);
    }

    public static implicit operator FieldsPropertiesPair((List<IFieldRequest>, List<BitFieldPropertyInfoRequest>) value)
    {
        return new FieldsPropertiesPair(value.Item1, value.Item2);
    }
}