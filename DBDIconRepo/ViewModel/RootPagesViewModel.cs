using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using DBDIconRepo.Strings;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DBDIconRepo.ViewModel;

public partial class RootPagesViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShouldShowAcrylicPanel))]
    [NotifyPropertyChangedFor(nameof(ShouldShowNonAcrylicPanel))]
    string backgroundImage = "";

    public Visibility ShouldShowAcrylicPanel => !string.IsNullOrEmpty(BackgroundImage) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShouldShowNonAcrylicPanel => string.IsNullOrEmpty(BackgroundImage) ? Visibility.Visible : Visibility.Collapsed;

    public void Initialize()
    {
        CheckIfDBDRunning();
        //Background
        BackgroundImage = BackgroundRandomizer.Get();
        Config.PropertyChanged += MonitorSetting; //Monitor for background change
        //Check for update
        CheckForUpdate().Await(() => { }, 
        (error) =>
        {
            Logger.Write($"{error.Message}\r\n{error.StackTrace}");
            UpdateState = CheckUpdateState.Failed;
        });
    }

    private async void MonitorSetting(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Setting.LockedBackgroundPath))
        {
            BackgroundImage = Config.LockedBackgroundPath;
        }
        else if (e.PropertyName == nameof(Setting.BackgroundMode))
        {
            BackgroundOption mode = (BackgroundOption)Config.BackgroundMode;
            switch (mode)
            {
                case BackgroundOption.Random:
                    BackgroundImage = BackgroundRandomizer.Get();
                    break;
            }
        }
        else if (e.PropertyName == nameof(Setting.LatestBeta))
        {
            await CheckForUpdate();
        }
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

    public string Version => Terms.Version;

    [RelayCommand]
    private async Task CheckForUpdate()
    {
        string url = Config.AppRepoURL;
        var splices = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (splices.Length != 4)
            UpdateState = CheckUpdateState.Failed;        
        var repo = await GitService.GitHubClientInstance.Repository.Get(splices[2], splices[3]);
        var releases = await GitService.GitHubClientInstance.Repository.Release.GetAll(repo.Id);
        
        if (!Config.LatestBeta)
        {
            var latest = releases.FirstOrDefault(i => !i.Prerelease);
            if (latest is null)
            {
                UpdateState = CheckUpdateState.Failed;
                return;
            }
            UpdateState = latest.TagName == Version ? CheckUpdateState.Updated : CheckUpdateState.Outdated;
            LatestVersionURL = latest.HtmlUrl;
        }
        else
        {
            var latest = releases.FirstOrDefault(i => i.Prerelease);
            if (latest is null)
            {
                UpdateState = CheckUpdateState.Failed;
            }
            UpdateState = latest.TagName == Version ? CheckUpdateState.Updated : CheckUpdateState.Outdated;
            LatestVersionURL = latest.HtmlUrl;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateIconColor))]
    [NotifyPropertyChangedFor(nameof(UpdateIconGlyph))]
    [NotifyPropertyChangedFor(nameof(UpdateLabel))]
    [NotifyPropertyChangedFor(nameof(AllowOpenAppURL))]
    CheckUpdateState updateState;

    public SolidColorBrush UpdateIconColor => updateState switch
    {
        CheckUpdateState.Updated => new(Colors.Green),
        CheckUpdateState.Outdated => new(Colors.Blue),
        _ => new(Colors.Red),
    };

    public string UpdateIconGlyph => updateState switch
    {
        CheckUpdateState.Updated => "\uE930",
        CheckUpdateState.Outdated => "\uE946",
        _ => "\uF384",
    };

    public string UpdateLabel => updateState switch
    {
        CheckUpdateState.Updated => "Program updated!",
        CheckUpdateState.Outdated => "Program outdated!",
        _ => "Check update failed!",
    };

    public bool AllowOpenAppURL => UpdateState == CheckUpdateState.Outdated;

    private string LatestVersionURL = string.Empty;

    [RelayCommand]
    public void OpenAppReleasePage()
    {
        if (LatestVersionURL == string.Empty)
            return;
        URL.OpenURL(LatestVersionURL);
    }
}

public enum CheckUpdateState
{
    Failed,
    Updated,
    Outdated
}