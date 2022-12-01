using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using System.Windows;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.ViewModel;

public partial class SettingViewModel : ObservableObject
{
    public SettingViewModel()
    {
        AvailableBackgrounds = new(BackgroundRandomizer.AvailableBackgrounds);
        AvailableBackgrounds.Insert(0, "Custom");
    }

    public Setting Config => SettingManager.Instance;

    [RelayCommand]
    private void BrowseForDBD(RoutedEventArgs? obj)
    {
        Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
        {
            RootFolder = Environment.SpecialFolder.CommonDocuments,
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true,
            Description = "Locate Dead by Daylight installation folder"
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            //Validate path
            Config.DBDInstallationPath = dialog.SelectedPath;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FindDBDForSteam))]
    [NotifyPropertyChangedFor(nameof(FindDBDForXbox))]
    [NotifyPropertyChangedFor(nameof(FindDBDForEpig))]
    FindDBDFor platformLocatorTarget;

    [RelayCommand]
    private void SetDBDLocatorToSteam() => PlatformLocatorTarget = FindDBDFor.Steam;
    [RelayCommand]
    private void SetDBDLocatorToXbox() => PlatformLocatorTarget = FindDBDFor.Xbox;
    [RelayCommand]
    private void SetDBDLocatorToEpig() => PlatformLocatorTarget = FindDBDFor.Epig;

    public bool FindDBDForSteam => PlatformLocatorTarget == FindDBDFor.Steam;
    public bool FindDBDForXbox => PlatformLocatorTarget == FindDBDFor.Xbox;
    public bool FindDBDForEpig => PlatformLocatorTarget == FindDBDFor.Epig;
    
    [RelayCommand]
    private void LocateDBD()
    {
        switch (PlatformLocatorTarget)
        {
            case FindDBDFor.Steam:
                LocateDBDForSteam();
                break;
            case FindDBDFor.Xbox:
                LocateDBDForXbox();
                break;
            case FindDBDFor.Epig:
                LocateDBDForEpig();
                break;
            default:
                //TODO:Adding support for Locating DBD on Toaster or something
                break;
        }
    }

    [RelayCommand]
    private void LocateDBDForSteam()
    {
        //Locate steam installation folder
        string? steamPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "").ToString();
        string libraryFolderFile = $"{steamPath}\\steamapps\\libraryfolders.vdf";
        if (File.Exists(libraryFolderFile))
        {
            string content = File.ReadAllText(libraryFolderFile);
            string dbdPath = SteamLibraryFolderHandler.GetDeadByDaylightPath(content);
            if (!string.IsNullOrEmpty(dbdPath))
            {
                Config.DBDInstallationPath = dbdPath;
            }
        }
    }

    [RelayCommand]
    private void LocateDBDForXbox()
    {

    }

    [RelayCommand]
    private void LocateDBDForEpig()
    {

    }

    [RelayCommand]
    private void ResetSetting(RoutedEventArgs? obj)
    {
        SettingManager.DeleteSettings();
        App.Current.Shutdown();
    }

    [RelayCommand]
    private void UninstallIconPack(RoutedEventArgs? obj)
    {
        if (string.IsNullOrEmpty(Config.DBDInstallationPath))
            return;
        if (IconManager.Uninstall(Config.DBDInstallationPath))
        {
            DialogHelper.Show($"Icon pack uninstall succesfully!");
        }
    }

    [RelayCommand]
    private void SetAllResMinimal()
    {
        Config.Resolution.SetAll(0.1);
    }

    [RelayCommand]
    private void SetAllResHalf()
    {
        Config.Resolution.SetAll(0.5);
    }

    [RelayCommand]
    private void SetAllResFull()
    {
        Config.Resolution.SetAll(1);
    }

    int bgOption = -1;
    public int BackgroundModeSetting
    {
        get
        {
            if (bgOption < 0)
            {
                bgOption = Config.BackgroundMode;
            }
            return bgOption;
        }
        set
        {
            if (SetProperty(ref bgOption, value))
            {
                Config.BackgroundMode = value;
                OnPropertyChanged(nameof(CanSetCustomBackground));
            }
        }
    }

    public Visibility CanSetCustomBackground => BackgroundModeSetting == (int)BackgroundOption.Lock
        ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    ObservableCollection<string>? availableBackgrounds = null;

    int _selectedCustomBackground = -1;
    public int SelectedCustomBackground
    {
        get
        {
            if (BackgroundModeSetting != (int)BackgroundOption.Lock)
                return -1;
            if (_selectedCustomBackground == -1)
                _selectedCustomBackground = AvailableBackgrounds.IndexOf(Config.LockedBackgroundPath);
            if (_selectedCustomBackground == -1)
                _selectedCustomBackground++;
            return _selectedCustomBackground;
        }
        set
        {
            if (SetProperty(ref _selectedCustomBackground, value))
            {
                if (AvailableBackgrounds[value] == "Custom")
                    ChooseBackground();
                else
                    Config.LockedBackgroundPath = AvailableBackgrounds[value];
            }
        }
    }

    [RelayCommand]
    private void ChooseBackground()
    {
        Ookii.Dialogs.Wpf.VistaOpenFileDialog dialog = new()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Title = "Select background picture",
            Multiselect = false,
            Filter = "Pictures|*.png;*.jpg;*.jpeg"
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            if (!File.Exists(dialog.FileName))
                return;

            var copy = File.ReadAllBytes(dialog.FileName);
            //Is it custom right now?
            var custom = Path.Join(SettingManager.Instance.CacheAndDisplayDirectory, "custombg");
            var custom1 = Path.Join(SettingManager.Instance.CacheAndDisplayDirectory, "bgcustom");
            try
            {
                if (File.Exists(custom))
                    File.Delete(custom);
                File.WriteAllBytes(custom, copy);
                SettingManager.Instance.LockedBackgroundPath = custom;
            }
            catch
            {
                if (File.Exists(custom1))
                    File.Delete(custom1);
                File.WriteAllBytes(custom1, copy);
                SettingManager.Instance.LockedBackgroundPath = custom1;
            }
            return;
        }
        SelectedCustomBackground = 1;
    }
}

public enum FindDBDFor
{
    Steam,
    Xbox,
    Epig
}