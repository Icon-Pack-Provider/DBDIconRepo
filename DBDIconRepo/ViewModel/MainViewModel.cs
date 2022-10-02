using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;
using DBDIconRepo.Service;

namespace DBDIconRepo.ViewModel;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {

    }

    [ObservableProperty]
    private string currentPageName = "Home";

    public OctokitService GitService => OctokitService.Instance;
    public Setting Config => SettingManager.Instance;
}
