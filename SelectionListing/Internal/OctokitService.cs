using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SelectionListing.Internal.Helper;
using Octokit;

namespace SelectionListing.Internal;

internal class OctokitService
{
    public GitHubClient Client { get; set; }

    public OctokitService(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Client = new(new ProductHeaderValue("iconlistingapi"));
            return;
        }
        Client = new(new ProductHeaderValue("iconlistingapi"))
        {
            Credentials = new(token)
        };
    }

    public OctokitService(GitHubClient client)
    {
        Client = client;
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
            ThrowHelper.APINotInitialize();
            if (_instance is null)
                return null;
            return _instance;
        }
    }
}
