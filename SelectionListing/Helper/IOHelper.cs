using SelectionListing.Internal;
using System.Text;
using static SelectionListing.Internal.Flags;
using static SelectionListing.Internal.Terms;

namespace SelectionListing.Helper;

internal static class IOHelper
{
    public static DirectoryInfo GetCacheCatagorizeDirectory()
    {
        if (!Directory.Exists(WorkingDirectory))
            Directory.CreateDirectory(WorkingDirectory);
        string dir = Path.Combine(WorkingDirectory, CloneDirectoryName);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return new DirectoryInfo(dir);
    }

    public static string GetCacheCatagorizeRepoURL()
    {
        string path = Path.Join(WorkingDirectory, "addon_repo.txt");
        if (!File.Exists(path))
        {
            File.WriteAllText(path, CloneURL);
            return CloneURL;
        }
        string url = File.ReadAllText(path);
        try
        {
            Uri validate = new(url);
            return url;
        }
        catch //Empty file, or invalid URL
        {
            return CloneURL;
        }
    }

    public static DirectoryInfo GetCacheDotGitDirectory()
    {
        var dir = GetCacheCatagorizeDirectory();
        string append = Path.Combine(dir.FullName, CloneDotGitDirectory);
        return new(append);
    }

    public static FileInfo GetLastFetchGitFile()
    {
        var file = GetCacheDotGitDirectory();
        string append = Path.Join(file.FullName, LastFetchFilename);
        return new(append);
    }

    public static string? GetDisplayFile(string currentDirectory)
    {
        currentDirectory = Path.Join(currentDirectory, DisplayFile);
        if (File.Exists(currentDirectory))
            return File.ReadAllText(currentDirectory, Encoding.UTF8);
        return null;
    }

    public static bool IsThisListingFolder(string currentDirectory)
    {
        currentDirectory = Path.Join(currentDirectory, ListingFile);
        return File.Exists(currentDirectory);
    }
}
