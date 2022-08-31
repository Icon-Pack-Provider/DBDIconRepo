using IconPack.Helper;
using Octokit;

namespace IconPack.Service;
public class OctokitService
{
    public GitHubClient? GitHubClientInstance;

    public void InitializeGit(string token)
    {
        GitHubClientInstance = new GitHubClient(new ProductHeaderValue("ballz"));
        if (!string.IsNullOrEmpty(token))
        {
            var tokenAuth = new Credentials(token);
            GitHubClientInstance.Credentials = tokenAuth;
        }
    }

    public static OctokitService Instance
        => Singleton<OctokitService>.Instance;
}