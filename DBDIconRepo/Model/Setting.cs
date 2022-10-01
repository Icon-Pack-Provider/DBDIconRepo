using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Strings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Model;

public partial class Setting : ObservableObject
{
    [ObservableProperty]
    string dBDInstallationPath = "";

    [ObservableProperty]
    bool useUncuratedContent = true;
    //TODO:Eventually load content only from specific repo pointed to other packs if this set to true (which is by default: true)

    [ObservableProperty]
    ObservableCollection<IconInfo.Icon.Perk> perkPreviewSelection = new()
    {
        IconInfo.Icons.Perks.Spine_Chill,
        IconInfo.Icons.Perks.Adrenaline,
        IconInfo.Icons.Perks.Sloppy_Butcher,
        IconInfo.Icons.Perks.Lightborn
    };

    [ObservableProperty]
    FilterOptions filterOptions = FilterOptions.CompletePack;

    [ObservableProperty]
    SortOptions sortBy = SortOptions.Name;

    [ObservableProperty]
    bool sortAscending;

    [ObservableProperty]
    bool alwaysClonePackRepo = false;

    [ObservableProperty]
    double downloadIfSelectMoreThanMeThreshold = 0.2d;

    //Banner res
    //1280x300
    [ObservableProperty]
    int bannerDecodeWidth = 256;

    [ObservableProperty]
    int bannerDecodeHeight = 60;

    //Perk res | Portrait res
    //256x256 | 512x512
    [ObservableProperty]
    int iconPreviewDecodeWidth = 64;

    [ObservableProperty]
    int iconPreviewDecodeHeight = 64;

    [ObservableProperty]
    string gitHubLoginToken = "";
#if DEBUG
    [ObservableProperty]
    string cacheAndDisplayDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Terms.AppDataFolder}Dev");
#else
    [ObservableProperty]
    string cacheAndDisplayDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Terms.AppDataFolder);
#endif
}

public static class SettingManager
{
    private static Setting? _instance = null;
    public static Setting Instance
    {
        get
        {
            _instance ??= LoadSettings();
            return _instance;
        }
    }

    private const string SettingFilename = "settings.json";

    private static FileInfo GetSettingFile()
    {
        StringBuilder pathBuilder = new();
        pathBuilder.Append(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        pathBuilder.Append('\\');
#if DEBUG
        pathBuilder.Append(Terms.AppDataFolder);
        pathBuilder.Append("Dev");
        pathBuilder.Append('\\');
#else
        pathBuilder.Append(Terms.AppDataFolder);
        pathBuilder.Append('\\');
#endif
        pathBuilder.Append(SettingFilename);
        return new FileInfo(pathBuilder.ToString());
    }

    public static void SaveSettings()
    {
        var setfile = GetSettingFile();
        if (setfile.Exists)
            setfile.Delete();

        string setting = JsonSerializer.Serialize(Instance, new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = false
        });

        //Write to file
        using StreamWriter writer = new(File.Create(setfile.FullName), Encoding.UTF8);
        writer.Write(setting);
    }

    public static Setting? LoadSettings()
    {
        var setfile = GetSettingFile();
        if (!File.Exists(setfile.FullName))
            return new();

        using StreamReader reader = File.OpenText(setfile.FullName);
        var setting = JsonSerializer.Deserialize<Setting>(reader.ReadToEnd());
        return setting;
    }

    public static void DeleteSettings()
    {
        var setfile = GetSettingFile();
        if (setfile.Exists)
            setfile.Delete();
        _instance = new();
    }
}

public enum SortOptions
{
    Name,
    Author,
    LastUpdate
}

public class FilterOptions : ObservableObject
{
    public static FilterOptions CompletePack
    {
        get
        {
            return new FilterOptions()
            {
                HasPerks = true,
                HasAddons = true,
                HasItems = true,
                HasOfferings = true,
                HasPortraits = true,
                HasPowers = true,
                HasStatus = true
            };
        }
    }

    bool _perks;
    public bool HasPerks
    {
        get => _perks;
        set
        {
            if (SetProperty(ref _perks, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasPerks), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _portraits;
    public bool HasPortraits
    {
        get => _portraits;
        set
        {
            if (SetProperty(ref _portraits, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasPortraits), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _powers;
    public bool HasPowers
    {
        get => _powers;
        set
        {
            if (SetProperty(ref _powers, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasPowers), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _item;
    public bool HasItems
    {
        get => _item;
        set
        {
            if (SetProperty(ref _item, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasItems), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _status;
    public bool HasStatus
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasStatus), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _offerings;
    public bool HasOfferings
    {
        get => _offerings;
        set
        {
            if (SetProperty(ref _offerings, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasOfferings), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }

    bool _addons;
    public bool HasAddons
    {
        get => _addons;
        set
        {
            if (SetProperty(ref _addons, value))
            {
                Messenger.Default.Send(new FilterOptionChangedMessage(nameof(HasAddons), this), MessageToken.FILTEROPTIONSCHANGETOKEN);
            }
        }
    }
}