namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
interface IReadOnlyBitFieldAttribute : IBitFieldAttribute
{
    AccessModifier ConstructorAccessModifier { get; set; }
}