using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using SelectionListing;
using System.Windows;

namespace DBDIconRepo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        OctokitService.Instance.InitializeGit();
        if (OctokitService.Instance.IsAnonymous)
        {
            Packs.Initialize(SettingManager.Instance.CacheAndDisplayDirectory);
            Lists.Initialize(SettingManager.Instance.CacheAndDisplayDirectory);
        }
        else
        {
            Packs.Initialize(OctokitService.Instance.GitHubClientInstance, SettingManager.Instance.CacheAndDisplayDirectory);
            Lists.Initialize(OctokitService.Instance.GitHubClientInstance, SettingManager.Instance.CacheAndDisplayDirectory);
        }
        StarService.Instance.InitializeStarService();
        base.OnStartup(e);
    }

    public static bool IsDevelopmentBuild()
    {
#if DEBUG 
        return true;
#else
        return false;
#endif
    }
}
