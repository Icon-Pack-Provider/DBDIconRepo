using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using Octokit;

namespace DBDIconRepo.Service;

public partial class OctokitService : ObservableObject
{
    public GitHubClient? GitHubClientInstance { get; private set; }

    [ObservableProperty]
    ReadOnlyObservableCollection<Repository>? userRepositories = null;

    public async Task GetUserRepos()
    {
        ObservableCollection<Repository> userRepos = new(await GitHubClientInstance.Repository.GetAllForCurrent());
        UserRepositories = new(userRepos);
    }

    [ObservableProperty]
    private bool isAnonymous;

    [ObservableProperty]
    private string loginUsername;

    public void InitializeGit()
    {
        GitHubClientInstance = new GitHubClient(new ProductHeaderValue("ballz"));
        var username = SettingManager.Instance.GitUsername;
        SecureSettingService userToken = new();
        var passOrToken = userToken.GetSecurePassword();
        if (passOrToken is null)
        {
            IsAnonymous = true;
            LoginUsername = string.Empty;
            return;
        }

        Credentials? tokenAuth = null;
        if (string.IsNullOrEmpty(username))
        {
            tokenAuth = new Credentials(passOrToken);
            try
            {
                Task.Run(async () =>
                {
                    var user = await GitHubClientInstance.User.Current();
                    SettingManager.Instance.GitUsername = user.Login;
                }).Await(() => { });
            }
            catch { /*Probably not enough permission on token scope?*/ }
        }
        else
        {
            tokenAuth = new Credentials(username, passOrToken);
            LoginUsername = username;
        }
        GitHubClientInstance.Credentials = tokenAuth;
        IsAnonymous = false;
    }

    public async Task CacheProfilePic()
    {
        var pfpFile = Path.Join(SettingManager.Instance.CacheAndDisplayDirectory, $"{LoginUsername}.png");
        var pfpFileInfo = new FileInfo(pfpFile);
        if (pfpFileInfo.Exists)
            return;

        var loggedin = await GitHubClientInstance.User.Current();
        byte[] pfp = await URL.LoadImageAsBytesFromOnline(loggedin.AvatarUrl);
        File.WriteAllBytes(pfpFile, pfp);        
    }

    public byte[] GetLoggedInUserAvatar()
    {
        var pfpFile = Path.Join(SettingManager.Instance.CacheAndDisplayDirectory, $"{LoginUsername}.png");
        var pfpFileInfo = new FileInfo(pfpFile);
        if (pfpFileInfo.Exists)
        {
            return File.ReadAllBytes(pfpFile);
        }
        return Array.Empty<byte>();
    }

    public LibGit2Sharp.UsernamePasswordCredentials GetLibGit2SharpCredential()
    {
        if (IsAnonymous)
            throw new Exception();
        return new LibGit2Sharp.UsernamePasswordCredentials()
        {
            Username = LoginUsername,
            Password = new SecureSettingService().GetSecurePassword()
        };
    }
    public static OctokitService Instance 
        => Singleton<OctokitService>.Instance;
}
