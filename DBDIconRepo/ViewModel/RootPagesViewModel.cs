using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using System.Windows;

namespace DBDIconRepo.ViewModel;

public partial class RootPagesViewModel : ObservableObject
{
    public RootPagesViewModel()
    {
        CheckIfDBDRunning();
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
}
