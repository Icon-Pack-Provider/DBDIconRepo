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

    public static OctokitService? Instance
    {
        get
        {
            ThrowHelper.APINotInitialize();
            return Ioc.Default.GetService<OctokitService>();
        }
    }
}
