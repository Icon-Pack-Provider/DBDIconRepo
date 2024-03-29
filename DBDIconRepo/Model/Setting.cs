﻿using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Strings;
using DBDIconRepo.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
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
    bool sortAscending = true;

    [ObservableProperty]
    bool alwaysClonePackRepo = false;

    [ObservableProperty]
    double downloadIfSelectMoreThanMeThreshold = 0.2d;

    [ObservableProperty]
    IconsResolution resolution = new();

    [ObservableProperty]
    //True: Install everything, False: Open pack install window
    bool installEverythingInPack = false;

    [ObservableProperty]
    string gitUsername = "";

    [ObservableProperty]
    bool showDefaultPack;

    [ObservableProperty]
    bool showDevTestPack;

    [ObservableProperty]
    /* Only use on InstallPack page
     * If turn on it will set icons load async method as Render (Which froze the app, but take less time to load icon)
     * If turn off it will set icons load async method as Background (Which didn't freeze the app, but take quite sometime to load all icons)
    */
    bool sacrificingAppResponsiveness = false;

    [ObservableProperty]
    bool dismissedTheFavoritePageHeaderPrompt = false;

    [ObservableProperty]
    int packViewMode = (int)PackView.Grid;

    [ObservableProperty]
    int backgroundMode = (int)BackgroundOption.Random;

    [ObservableProperty]
    string lockedBackgroundPath = string.Empty;

    [ObservableProperty]
    string appRepoURL = "https://github.com/Icon-Pack-Provider/DBDIconRepo";

    [ObservableProperty]
    bool latestBeta = false;

    [ObservableProperty]
    bool saveHistory = true;

#if DEBUG
    [ObservableProperty]
    string cacheAndDisplayDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"{Terms.AppDataFolder}Dev");
#else
    [ObservableProperty]
    string cacheAndDisplayDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Terms.AppDataFolder);
#endif

    [ObservableProperty]
    string oauthStateName = string.Empty;

    [ObservableProperty]
    bool landedOnLandingPageBefore = false;
}

public enum BackgroundOption
{
    None,
    Random,
    Lock
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
        var file = new FileInfo(pathBuilder.ToString());
        if (!file.Directory.Exists)
            file.Directory.Create();
        return file;
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

public partial class IconsResolution : ObservableObject
{
    [ObservableProperty]
    private double bannerScale = 0.5;

    [ObservableProperty]
    private double addonScale = 0.5;

    [ObservableProperty]
    private double dailyRitualScale = 0.5;

    [ObservableProperty]
    private double emblemScale = 0.5;

    [ObservableProperty]
    private double itemScale = 0.5;

    [ObservableProperty]
    private double offeringScale = 0.5;

    [ObservableProperty]
    private double perkScale = 0.5;

    [ObservableProperty]
    private double portraitScale = 0.5;

    [ObservableProperty]
    private double powerScale = 0.5;

    [ObservableProperty]
    private double statusEffectScale = 0.5;

    public IconsResolution()
    {
        BannerScale =
        AddonScale =
        DailyRitualScale =
        EmblemScale =
        ItemScale =
        OfferingScale =
        PerkScale =
        PowerScale =
        PortraitScale =
        StatusEffectScale = 0.5;
        PropertyChanged += SaveConfigs;
    }

    [JsonIgnore]
    private Debouncer _debounce = new();
    private void SaveConfigs(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _debounce.Debounce(500, () =>
        {
            SettingManager.SaveSettings();
        });
    }

    public void SetAll(double all)
    {
        BannerScale =
        AddonScale =
        DailyRitualScale =
        EmblemScale =
        ItemScale =
        OfferingScale =
        PerkScale =
        PowerScale =
        PortraitScale =
        StatusEffectScale = all;
    }
}

public static class IconResolutionScale
{
    /// <summary>
    /// 1280x300
    /// </summary>
    public static Size Banner => new(1280 * SettingManager.Instance.Resolution.BannerScale,
                300 * SettingManager.Instance.Resolution.BannerScale);

    /// <summary>
    /// 512x512
    /// </summary>
    public static Size Portrait => new(512 * SettingManager.Instance.Resolution.PortraitScale,
        512 * SettingManager.Instance.Resolution.PortraitScale);

    public static Size Addon => new(256 * SettingManager.Instance.Resolution.AddonScale,
        256 * SettingManager.Instance.Resolution.AddonScale);

    public static Size Emblem => new(256 * SettingManager.Instance.Resolution.EmblemScale,
        256 * SettingManager.Instance.Resolution.EmblemScale);

    public static Size Item => new(256 * SettingManager.Instance.Resolution.ItemScale,
        256 * SettingManager.Instance.Resolution.ItemScale);

    public static Size Offering => new(256 * SettingManager.Instance.Resolution.OfferingScale,
        256 * SettingManager.Instance.Resolution.OfferingScale);

    public static Size Perk => new(256 * SettingManager.Instance.Resolution.PerkScale,
        256 * SettingManager.Instance.Resolution.PerkScale);

    public static Size Power => new(256 * SettingManager.Instance.Resolution.PowerScale,
        256 * SettingManager.Instance.Resolution.PowerScale);

    public static Size DailyRitual = new(128 * SettingManager.Instance.Resolution.DailyRitualScale,
        128 * SettingManager.Instance.Resolution.DailyRitualScale);

    public static Size StatusEffect = new(128 * SettingManager.Instance.Resolution.StatusEffectScale,
        128 * SettingManager.Instance.Resolution.StatusEffectScale);

    public static Size GetResolutionScale(string type)
    {
        switch (type)
        {
            case "banner": //1280x300
                return Banner;
            case "portrait":
                return Portrait;
            case "addon": //256x256
                return Addon;
            case "emblem":
                return Emblem;
            case "item":
                return Item;
            case "offering":
                return Offering;
            case "perk":
                return Perk;
            case "power":
                return Power;
            case "daily": //128x128
            case "dailyritual":
                return DailyRitual;
            case "status":
            case "statuseffect":
                return StatusEffect;
            default:
                return Perk;
        }
    }
}