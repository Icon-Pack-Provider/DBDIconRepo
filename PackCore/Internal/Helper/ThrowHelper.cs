using static IconPack.Internal.Flags;

namespace IconPack.Internal.Helper;

internal static class ThrowHelper
{
    public static void APINotInitialize()
    {
        if (!IsInitialized)
            throw new APINotInitializeException();
    }
}
