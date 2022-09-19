using System;
using System.IO;
using DBDIconRepo.Helper;
using Octokit;

namespace DBDIconRepo.Service;

public class OctokitService
{
    public GitHubClient? GitHubClientInstance;
    private string token = "";

    public void InitializeGit()
    {
        GitHubClientInstance = new GitHubClient(new ProductHeaderValue("ballz"));
        if (string.IsNullOrEmpty(token))
        {
            string tokenFile = $"{Environment.CurrentDirectory}\\token.txt";
            if (File.Exists(tokenFile))
            {
                token = File.ReadAllText(tokenFile);
            }
        }
        if (!string.IsNullOrEmpty(token))
        {
            var tokenAuth = new Credentials(token);
            GitHubClientInstance.Credentials = tokenAuth;
        }
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
