using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using DBDIconRepo.ViewModel;
using DBDIconRepo.Views;
using IconPack;
using SelectionListing;
using SingleInstanceCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DBDIconRepo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, ISingleInstance
{
    private const string AppUniqueName = "DBDIconRepository:SpaghettiOfMadness";

    public void OnInstanceInvoked(string[] args)
    {
        foreach (var arg in args)
        {
            if (!arg.StartsWith(AssociationURIHelper.AppURI))
                continue;
            var request = AppURIHelper.Read(arg);
            switch (request)
            {
                case AuthRequest auth:
                    Current.Dispatcher.Invoke(() =>
                    {
                        AnonymousUserViewModel.ContinueAuthenticateAsync(auth).Await(() =>
                        {

                        },
                        (e) =>
                        {
                            DialogHelper.Show("Please make sure you're using latest version of the software!\r\n" +
                                "Or using Advanced login", "Fatal Error while login", Dialog.DialogButtons.Ok, Dialog.DialogSymbol.Information);
                        });
                    });                    
                    break;
                case NavigationRequest nav:
                    Current.Dispatcher.Invoke(() =>
                    {
                        if (Current.MainWindow is not RootPages root)
                            return;
                        root.SwitchPage(nav.Page);
                    });
                    break;
            }
        }
        WindowHelper.Restore();
    }
    private void StartupHandler(object sender, StartupEventArgs e)
    {
        bool isFirstInstance = this.InitializeAsFirstInstance(AppUniqueName);
        if (!isFirstInstance)
        {
            Environment.Exit(0);
            return;
        }
        //Git
        OctokitService.Instance.InitializeGit();
        //URI Association
        if (!AssociationURIHelper.IsRegistered())
            AssociationURIHelper.RegisterAppURI();
        //The rest of git for extensions
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
    }

    public static bool IsDevelopmentBuild()
    {
#if DEBUG 
        return true;
#else
        return false;
#endif
    }

    private void Exiting(object sender, ExitEventArgs e)
    {
        SingleInstance.Cleanup();
    }
}
