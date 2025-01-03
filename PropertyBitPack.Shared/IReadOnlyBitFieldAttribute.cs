namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
interface IReadOnlyBitFieldAttribute
{
    AccessModifier ConstructorAccessModifier { get; set; }
}