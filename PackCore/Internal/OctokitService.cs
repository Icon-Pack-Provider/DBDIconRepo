using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using IconPack.Internal.Helper;
using Octokit;
using static IconPack.Resource.Terms;

namespace IconPack.Internal;

internal class OctokitService
{
    public GitHubClient Client { get; set; }

    public OctokitService(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Client = new(new ProductHeaderValue(productHeaderValue));
            return;
        }
        Client = new(new ProductHeaderValue(productHeaderValue))
        {
            Credentials = new(token)
        };
    }

    public OctokitService(GitHubClient client)
    {
        Client = client;
    }

    public OctokitService()
    {
        Client = new(new ProductHeaderValue(productHeaderValue));
    }

    public static void InitializeInstance(OctokitService instance)
    {
        _instance = instance;
    }

    private static OctokitService? _instance = null;
    public static OctokitService? Instance
    {
        get
        {
            if (_instance is null)
            {
                ThrowHelper.APINotInitialize();
                return null;
            }
            return _instance;
        }
    }

    internal static LibGit2Sharp.Credentials GetLibGitCredential(string owner)
    {
        return new LibGit2Sharp.UsernamePasswordCredentials()
        {
            Username = owner,
            Password = Instance.Client.Credentials.GetToken()
        };
    }
}
