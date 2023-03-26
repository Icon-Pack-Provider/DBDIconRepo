using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Dialog;
using DBDIconRepo.Service;
using Octokit;
using System.Windows;
using System;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using DBDIconRepo.Model;
using DBDIconRepo.Helper;
using System.Threading.Tasks;

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
    private void ManuallyLoginToGithub()
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

    public static string clientID = "17e24b3ee77c8e9027ad";
    public static string developmentTestSecret = string.Empty;

    [RelayCommand]
    private void LoginToGithub()
    {
        if (!AssociationURIHelper.IsRegistered())
            AssociationURIHelper.RegisterAppURI();


        string stateName = SettingManager.Instance.OauthStateName;
        if (string.IsNullOrEmpty(stateName))
        {
#if DEBUG
            stateName = $"DEVENV-{Environment.UserName}@{Environment.UserDomainName}.{Guid.NewGuid()}";
#else
            stateName = $"{Environment.UserName}@{Environment.UserDomainName}.{Guid.NewGuid()}";
#endif
            SettingManager.Instance.OauthStateName = stateName;
            SettingManager.SaveSettings();
        }
        var request = new OauthLoginRequest(clientID)
        {
            Scopes = { "user", "repo" },
            RedirectUri = new Uri("dbdiconrepo://authenticate"),
            State = stateName,
            AllowSignup = true
        };
        var uri = client.Oauth.GetGitHubLoginUrl(request);
        URL.OpenURL(uri.ToString());
    }

    public static async Task ContinueAuthenticateAsync(AuthRequest auth)
    {
        if (SettingManager.Instance.OauthStateName != auth.State)
            return;
        var tokenRequest = new OauthTokenRequest(clientID, developmentTestSecret, auth.Code);
        var token = await OctokitService.Instance.GitHubClientInstance.Oauth.CreateAccessToken(tokenRequest);
        OctokitService.Instance.GitHubClientInstance.Credentials = new(token.AccessToken);
        var me = await OctokitService.Instance.GitHubClientInstance.User.Current();
        SecureSettingService sss = new();
        sss.SaveSecurePassword(token.AccessToken);
        SettingManager.Instance.GitUsername = me.Login;
    }
}
