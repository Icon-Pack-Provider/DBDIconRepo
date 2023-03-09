using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using SelectionListing;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace DBDIconRepo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    private const string AppUniqueName = "DBDIconRepository:SpaghettiOfMadness";
    private bool _isIntantiated;
    private Mutex? _appMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        //Single instance
        _appMutex = new Mutex(true, AppUniqueName, out _isIntantiated);

        if (!_isIntantiated)
        {
            _appMutex = null;
            // activate existing instance
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    ShowWindow(process.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
            // exit current instance
            Current.Shutdown();
            return;
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
        base.OnExit(e);

        if (_isIntantiated)
        {
            _appMutex.ReleaseMutex();
        }

        _appMutex.Close();
        _appMutex = null;
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
