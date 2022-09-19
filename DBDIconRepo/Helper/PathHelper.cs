using System.IO;

namespace DBDIconRepo.Helper;

public static class PathHelper
{
    public static string NameOnly(this string original) => Path.GetFileNameWithoutExtension(original);
    public static string NameOnly(this FileInfo original) => Path.GetFileNameWithoutExtension(original.FullName);
}