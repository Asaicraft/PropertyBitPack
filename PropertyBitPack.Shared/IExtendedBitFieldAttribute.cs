namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
interface IExtendedBitFieldAttribute : IBitFieldAttribute
{
    public string GetterLargeSizeValueName { get; set; }
}