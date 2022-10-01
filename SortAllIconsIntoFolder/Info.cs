using IconInfo.Internal;
using System.Text;

namespace SortAllIconsIntoFolder;

internal class InternalInfo
{
    public static List<char> Invalids => Path.GetInvalidPathChars().ToList();
    public static Dictionary<Type, string> TypeToFolder => new()
            {
                { typeof(IconInfo.Icon.Addon), "ItemAddons" },
                { typeof(IconInfo.Icon.DailyRitual), "DailyRituals" },
                { typeof(IconInfo.Icon.Portrait), "CharPortraits" },
                { typeof(IconInfo.Icon.Emblem), "Emblems" },
                { typeof(IconInfo.Icon.Offering), "Favors" },
                { typeof(IconInfo.Icon.Item), "items" },
                { typeof(IconInfo.Icon.Perk), "Perks" },
                { typeof(IconInfo.Icon.Power), "Powers" },
                { typeof(IconInfo.Icon.StatusEffect), "StatusEffects" }
            };

    public static void MoveToItsFolder<T>(string source, string root, IFolder? folder = null, T? info = default)
    {
        var subFolder = folder is null ? string.Empty : folder.Folder;

        StringBuilder pathBuilder = new();

        pathBuilder.Append(root);
        pathBuilder.Append('\\');

        if (!TypeToFolder.ContainsKey(info.GetType()))
            return;
        pathBuilder.Append(TypeToFolder[info.GetType()]);
        pathBuilder.Append('\\');

        if (folder is not null)
        {
            pathBuilder.Append(folder.Folder);
            pathBuilder.Append('\\');
        }

        pathBuilder.Append(((IBasic)info).File);
        pathBuilder.Append(".png");

        Console.WriteLine($"Moving {source} to {pathBuilder}");
        var origin = File.ReadAllBytes(source);
        FileInfo fif = new(pathBuilder.ToString());
        if (!fif.Directory.Exists)
            fif.Directory.Create();

        File.WriteAllBytes(pathBuilder.ToString(), origin);
        File.Delete(source);
    }

    internal static string PathCheck(string path)
    {
        if (Invalids.Any(invalid => path.Contains(invalid)))
            Invalids.ForEach((c) => { path = path.Replace(c.ToString(), ""); });
        if (path.StartsWith('"'))
            path = path.Trim('"');
        return path;
    }
}