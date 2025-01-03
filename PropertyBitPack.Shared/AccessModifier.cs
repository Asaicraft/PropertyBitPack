namespace PropertyBitPack;

#if PUBLIC_PACKAGE
public
#else
internal
#endif
enum AccessModifier
{
    Public,
    Protected,
    Internal,
    ProtectedInternal,
    Private,
    PrivateProtected,
    Default 
}