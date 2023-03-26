using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Service;
using Octokit;
using System.Threading.Tasks;
using System;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using DBDIconRepo.Model;
using DBDIconRepo.Helper;
using System.IO;
using System.Linq;

namespace DBDIconRepo.ViewModel;

public partial class UserViewModel : ObservableObject, IRateInfo
{
    public UserViewModel()
    {
        //Check cached
        DirectoryInfo dir = new(SettingManager.Instance.CacheAndDisplayDirectory);
        var files = dir.GetFiles($"{GitService.LoginUsername}.*", SearchOption.AllDirectories);
        if (files.FirstOrDefault(file => file.NameOnly() == GitService.LoginUsername) is FileInfo pfp)
            ProfilePicUri = pfp.FullName;
        else
            GetProfilePic().Await(() => { });
    }

    GitHubClient client => OctokitService.Instance.GitHubClientInstance;

    public OctokitService GitService => OctokitService.Instance;
    public Setting Config => SettingManager.Instance;

    [ObservableProperty]
    string? profilePicUri;

    public async Task GetProfilePic()
    {
        await GitService.CacheProfilePic();
        var loggedin = await client.User.Current();
        ProfilePicUri = loggedin.AvatarUrl;
    }

    [ObservableProperty]
    int? requestPerHour;

    [ObservableProperty]
    int? requestRemain;

    [ObservableProperty]
    string? resetIn;

    [RelayCommand]
    public void CheckRateLimit()
    {
        var apiInfo = client.GetLastApiInfo();
        if (apiInfo?.RateLimit is null)
        {
            requestPerHour = null;
            requestRemain = null;
            resetIn = null;
            return;
        }

        var rateLimit = apiInfo.RateLimit;

        RequestPerHour = rateLimit.Limit;
        RequestRemain = rateLimit.Remaining;
        var reset = rateLimit.Reset.UtcDateTime;
        var now = DateTime.UtcNow;
        var resetts = reset - now;
        ResetIn = $"{resetts.TotalHours:00}:{resetts.Minutes:00}:{resetts.Seconds:00}";
    }

    [RelayCommand]
    public async void DestructivelyCheckRateLimit()
    {
        var rateLimit = await client.Miscellaneous.GetRateLimits();

        RequestPerHour = rateLimit.Resources.Core.Limit;
        RequestRemain = rateLimit.Resources.Core.Remaining;
        var reset = rateLimit.Resources.Core.Reset.UtcDateTime;
        var now = DateTime.UtcNow;
        var resetts = reset - now;
        ResetIn = $"{resetts.TotalHours:00}:{resetts.Minutes:00}:{resetts.Seconds:00}";
    }

    [RelayCommand]
    private void LogoutOfGithub()
    {
        SecureSettingService svc = new();
        svc.Logout();
        OctokitService.Instance.InitializeGit();        
    }
}
