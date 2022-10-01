
//Moving
using IconInfo;
using IconInfo.Internal;
using SortAllIconsIntoFolder;

string path = $"{Environment.CurrentDirectory}\\Sorting";
Console.WriteLine("Sort at specific directory?:");
tryagain:
path = Console.ReadLine();
if (string.IsNullOrEmpty(path))
{
    goto tryagain;
}
path = InternalInfo.PathCheck(path);

tryotherpath:
foreach (var file in Directory.GetFiles(path))
{
    FileInfo fif = new(file);
    var nameOnly = Path.GetFileNameWithoutExtension(file);
    if (fif.Extension != ".png")
        continue;

    //Try putting on every list
    //Is it addon?
    try
    {
        IBasic? iconInfo = default;
        IFolder? isFolder = default;
        if (Info.Perks.ContainsKey(nameOnly))
            iconInfo = Info.Perks[nameOnly];
        else if (Info.Powers.ContainsKey(nameOnly))
            iconInfo = Info.Powers[nameOnly];
        else if (Info.Items.ContainsKey(nameOnly))
            iconInfo = Info.Items[nameOnly];
        else if (Info.Offerings.ContainsKey(nameOnly))
            iconInfo = Info.Offerings[nameOnly];
        else if (Info.DailyRituals.ContainsKey(nameOnly))
            iconInfo = Info.DailyRituals[nameOnly];
        else if (Info.StatusEffects.ContainsKey(nameOnly))
            iconInfo = Info.StatusEffects[nameOnly];
        else if (Info.Portraits.ContainsKey(nameOnly))
            iconInfo = Info.Portraits[nameOnly];
        else
        {
            iconInfo = Info.GetAddon(nameOnly,
                fif.Directory.Name == "ItemAddons" ? null : fif.Directory.Name);
        }

        if (iconInfo is IFolder)
            isFolder = (IFolder)iconInfo;
        InternalInfo.MoveToItsFolder(file, path, isFolder ?? null, iconInfo);
    }
    catch
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error moving {file}; don't know where to move its to.");
        Console.ForegroundColor = prev;
        continue;
    }
}

Console.WriteLine("Move all icons finished!");
Console.WriteLine("Want to organize other folder? [(Directory)/Y/N]");
var retry = Console.ReadLine();
var ispath = InternalInfo.PathCheck(retry);
if (Directory.Exists(ispath))
{
    path = ispath;
    goto tryotherpath;
}

if (retry.ToString().ToLower()[0] == 'y')
{
    Console.Clear();
    Console.WriteLine("Sort at specific directory?:");
    goto tryagain;
}