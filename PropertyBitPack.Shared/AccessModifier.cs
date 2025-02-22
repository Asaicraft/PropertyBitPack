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

internal static class AccessModifiers
{
    public static AccessModifier Invalid = (AccessModifier)(-1);
}