namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
interface IExtendedBitFieldAttribute
{
    public string? GetterLargeSizeValueName { get; set; }
}