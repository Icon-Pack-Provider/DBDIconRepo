namespace SelectionListing.Internal;

internal static class Flags
{
    public static bool IsInitialized { get; set; } = false;
    public static string WorkingDirectory { get; set; } = "";
    public static string CloneURL { get; set; } = "https://github.com/Icon-Pack-Provider/IconRepoAddons.git";
}
