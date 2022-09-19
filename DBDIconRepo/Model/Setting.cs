using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Model;

public class Setting : ObservableObject
{
    public bool Set<T>(ref T storage, T value, [CallerMemberName]string? propertyName = null)
    {
        if (SetProperty(ref storage, value, propertyName))
        {
            if (ShouldSendMessageOnChange)
                Messenger.Default.Send(new SettingChangedMessage(propertyName, value), MessageToken.SETTINGVALUECHANGETOKEN);
            return true;
        }
        return false;
    }

    string _dbdPath = "";
    public string DBDInstallationPath
    {
        get => _dbdPath;
        set => Set(ref _dbdPath, value);
    }

    bool _useUncuratedContent = true;
    //TODO:Eventually load content only from specific repo pointed to other packs if this set to true (which is by default: true)
    public bool UseUncuratedContent
    {
        get => _useUncuratedContent;
        set => Set(ref _useUncuratedContent, value);
    }

    ObservableCollection<IconInfo.Icon.Perk> _selectedPreview = new()
    {
        IconInfo.Icons.Perks.Spine_Chill,
        IconInfo.Icons.Perks.Adrenaline,
        IconInfo.Icons.Perks.Sloppy_Butcher,
        IconInfo.Icons.Perks.Lightborn
    };
    public ObservableCollection<IconInfo.Icon.Perk> PerkPreviewSelection
    {
        get => _selectedPreview;
        set => Set(ref _selectedPreview, value);
    }

    FilterOptions _filters = FilterOptions.CompletePack;
    public FilterOptions FilterOptions
    {
        get => _filters;
        set => Set(ref _filters, value);
    }

    SortOptions _sort = SortOptions.Name;
    public SortOptions SortBy
    {
        get => _sort;
        set => Set(ref _sort, value);
    }

    bool _sortAscending;
    public bool SortAscending
    {
        get => _sortAscending;
        set => Set(ref _sortAscending, value);
    }

    bool _abuseGitOrDont = false;
    public bool AlwaysClonePackRepo
    {
        get => _abuseGitOrDont;
        set => Set(ref _abuseGitOrDont, value);
    }

    double _notDownloadThreshold = 0.2d;
    public double DownloadIfSelectMoreThanMeThreshold
    {
        get => _notDownloadThreshold;
        set => Set(ref _notDownloadThreshold, value);
    }

    //Banner res
    //1280x300
    int bannerDecodeWidth = 256;
    public int BannerDecodeWidth
    {
        get => bannerDecodeWidth;
        set => Set(ref bannerDecodeWidth, value);
    }

    int bannerDecodeHeight = 60;
    public int BannerDecodeHeight
    {
        get => bannerDecodeHeight;
        set => Set(ref bannerDecodeHeight, value);
    }

    //Perk res | Portrait res
    //256x256 | 512x512
    int iconPreviewDecodeWidth = 64;
    public int IconPreviewDecodeWidth
    {
        get => iconPreviewDecodeWidth; 
        set => Set(ref iconPreviewDecodeWidth, value);
    }

    int iconPreviewDecodeHeight = 64;
    public int IconPreviewDecodeHeight
    {
        get => iconPreviewDecodeHeight;
        set => Set(ref iconPreviewDecodeHeight, value);
    }
    

    #region Messenger manager
    [JsonIgnore]
    private static bool ShouldSendMessageOnChange = false;

    public static void EnableMessageGateOnSettingChanged() => ShouldSendMessageOnChange = true;
    public static void DisableMessageGateOnSettingChanged() => ShouldSendMessageOnChange = false;

    [JsonIgnore]
    private static Setting? _instance;
    [JsonIgnore]
    public static Setting? Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = SettingManager.LoadSettings();
                if (_instance is null)
                    _instance = new Setting();
            }
            return _instance;
        }
    }
    #endregion
}

public static class SettingManager
{
    [JsonIgnore]
    private const string SettingFilename = "settings.json";
    public static void SaveSettings(this Setting instance)
    {
        //Replace existing
        string settingFilePath = $"{Environment.CurrentDirectory}\\{SettingFilename}";
        if (File.Exists(settingFilePath))
        {
            File.Delete(settingFilePath);
        }

        string setting = JsonSerializer.Serialize(instance, new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = false
        });

        //Write to file
        using (StreamWriter writer = new(File.Create(settingFilePath), Encoding.UTF8))
        {
            writer.Write(setting);
        }
    }

    public static Setting? LoadSettings()
    {

        string settingFilePath = $"{Environment.CurrentDirectory}\\{SettingFilename}";

        if (!File.Exists(settingFilePath))
        {
            return null;
        }

        using (StreamReader reader = File.OpenText(settingFilePath))
        {
            Setting.DisableMessageGateOnSettingChanged();
            var setting = JsonSerializer.Deserialize<Setting>(reader.ReadToEnd());
            Setting.EnableMessageGateOnSettingChanged();
            return setting;
        }
    }

    public static void DeleteSettings()
    {
        string settingFilePath = $"{Environment.CurrentDirectory}\\{SettingFilename}";
        if (File.Exists(settingFilePath))
        {
            File.Delete(settingFilePath);
        }
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