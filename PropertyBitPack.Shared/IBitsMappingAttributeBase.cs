namespace PropertyBitPack;

internal interface IBitsMappingAttributeBase
{
    byte BitsCount { get; set; }
    string? FieldName { get; set; }
}