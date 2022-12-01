using DBDIconRepo.Model;
using SelectionListing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBDIconRepo.Helper;

public static class BackgroundRandomizer
{
    private static List<string>? availableBackgrounds = null;
    public static List<string>? AvailableBackgrounds
    {
        get
        {
            if (availableBackgrounds is null)
                availableBackgrounds = List();
            return availableBackgrounds;
        }
        set
        {
            availableBackgrounds = value;
        }
    }

    private static List<string> List()
    {
        string path = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory,
            Terms.CloneDirectoryName, "Background");
        DirectoryInfo dir = new DirectoryInfo(path);
        if (!dir.Exists)
        {
            return new();
        }
        return new(Directory.GetFiles(path));
    }

    public static string Get()
    {
        if (AvailableBackgrounds is null)
            AvailableBackgrounds = List();
        //Check setting first
        BackgroundOption option = (BackgroundOption)SettingManager.Instance.BackgroundMode;
        switch (option)
        {
            case BackgroundOption.Random:
                //Daily seed
                int seed = (int)(DateTime.Today.Ticks % int.MaxValue);
                Random random = new(seed);
                return AvailableBackgrounds[random.Next(0, AvailableBackgrounds.Count)];
            case BackgroundOption.Lock:
                return SettingManager.Instance.LockedBackgroundPath;
            default:
            case BackgroundOption.None:
                return string.Empty;
        }
    }
}
