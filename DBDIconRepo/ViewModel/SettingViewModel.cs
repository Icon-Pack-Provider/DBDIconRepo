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

    [RelayCommand]
    private void LocateDBD(RoutedEventArgs? obj)
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

    private void FindDBDXboxAction(RoutedEventArgs? obj)
    {
        throw new NotImplementedException();
    }

    private void FindDBDEpicAction(RoutedEventArgs? obj)
    {
        throw new NotImplementedException();
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
