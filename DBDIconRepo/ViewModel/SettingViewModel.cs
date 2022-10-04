using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using System.Windows;
using System;
using System.IO;

namespace DBDIconRepo.ViewModel;

public partial class SettingViewModel : ObservableObject
{
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
            MessageBox.Show($"Icon pack uninstall succesfully!");
        }
    }
}

public enum FindDBDFor
{
    Steam,
    Xbox,
    Epig
}