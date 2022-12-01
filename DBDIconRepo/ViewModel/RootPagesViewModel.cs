using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using System;
using System.IO;
using System.Windows;

namespace DBDIconRepo.ViewModel;

public partial class RootPagesViewModel : ObservableObject
{
    [ObservableProperty]
    string backgroundImage = "";
    public RootPagesViewModel()
    {
        CheckIfDBDRunning();
        //Temporal bg
        BackgroundImage = BackgroundRandomizer.Get();
    }

    public void CheckIfDBDRunning()
    {
        IsDBDRunning = ProcessChecker.IsDBDRunning();
    }

    [ObservableProperty]
    private string currentPageName = "Home";

    public OctokitService GitService => OctokitService.Instance;
    public Setting Config => SettingManager.Instance;

    [ObservableProperty]
    bool isDBDRunning;

    public Visibility RevealWhenDBDIsRunning;

    [ObservableProperty]
    bool isInitializing;

    [ObservableProperty]
    string progressText = string.Empty;
}
