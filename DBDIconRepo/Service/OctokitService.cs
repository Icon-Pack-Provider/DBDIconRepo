using System;
using System.IO;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using Octokit;

namespace DBDIconRepo.Service;

public class OctokitService
{
    public GitHubClient? GitHubClientInstance;
    private string token = "";

    public void InitializeGit()
    {
        GitHubClientInstance = new GitHubClient(new ProductHeaderValue("ballz"));
        var tokenAuth = new Credentials(SettingManager.Instance.GitHubLoginToken);
        GitHubClientInstance.Credentials = tokenAuth;
    }

    public static OctokitService Instance
    {
        get
        {
            if (!Singleton<OctokitService>.HasInitialize)
                Singleton<OctokitService>.Instance.InitializeGit();
            return Singleton<OctokitService>.Instance;
        }
    }
}
