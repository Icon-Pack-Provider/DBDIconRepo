using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Dialog;
using DBDIconRepo.Service;
using Octokit;
using System.Windows;
using System;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using DBDIconRepo.Model;

namespace DBDIconRepo.ViewModel;

public partial class AnonymousUserViewModel : ObservableObject
{
    GitHubClient client => OctokitService.Instance.GitHubClientInstance;

    public OctokitService GitService => OctokitService.Instance;

    [ObservableProperty]
    int? requestPerHour;

    [ObservableProperty]
    int? requestRemain;

    [ObservableProperty]
    string? resetIn;

    [RelayCommand]
    private void CheckRateLimit()
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
    private async void DestructivelyCheckRateLimit()
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
    private void LoginToGithub()
    {
        Login login = new()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        if (login.ShowDialog() == true)
        {
            //Force switch to other page?
            Messenger.Default.Send(new SwitchToOtherPageMessage("loggedIn"), MessageToken.RequestMainPageChange);
        }
    }
}
