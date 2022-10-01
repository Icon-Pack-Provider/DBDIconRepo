using static SelectionListing.Internal.Flags;

namespace SelectionListing.Internal.Helper;

internal static class ThrowHelper
{
    public static void APINotInitialize()
    {
        if (!IsInitialized)
            throw new APINotInitializeException();
    }
}
