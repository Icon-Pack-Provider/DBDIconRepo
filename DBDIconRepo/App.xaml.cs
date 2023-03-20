using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using SelectionListing;
using SingleInstance;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string AppUniqueName = "DBDIconRepository:SpaghettiOfMadness";
    public SingleInstanceService InstanceInfo;

    public App()
    {
        InstanceInfo = new(AppUniqueName);
    }

    private async Task<Unit> ServerResponseAsync((string, Action<string>) receive)
    {
        var (message, endFunc) = receive;
        
        //TODO:Handle parameters
        try
        {
            Uri read = new(message);
            switch (read.Segments[0])
            {
                case "home":
                    Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
                    break;
            }
        }
        catch
        {

        }
        //eg. Page navigation

        endFunc("success"); // Send response
        return default;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        //URI Association
        if (!AssociationURIHelper.IsRegistered())
            AssociationURIHelper.RegisterAppURI();
        //Instance helper
        bool start = InstanceInfo.TryStartSingleInstance();
        if (start && InstanceInfo.IsFirstInstance)
        {
            // This is the first instance.
            InstanceInfo.StartListenServer();
            InstanceInfo.Received.SelectMany(ServerResponseAsync).Subscribe();
        }
        else
        {
            // This is not the first instance.
            if (e.Args.Length == 0)
            {
                Current.Shutdown();
                return;
            }
            else
            {
                ISingleInstanceService secondInstance = new SingleInstanceService(AppUniqueName);
                foreach (var arg in e.Args)
                {
                    
                    secondInstance.SendMessageToFirstInstanceAsync(arg);
                }
                Current.Shutdown();
            }
        }
        //Git
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
    }

    protected override void OnExit(ExitEventArgs e)
    {
        InstanceInfo.Dispose();
        base.OnExit(e);
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
