using IconPack.Helper;
using IconPack.Strings;
using Octokit;

namespace IconPack.Service;
public class OctokitService
{
    public GitHubClient? GitHubClientInstance;

    internal void InitializeGit(GitHubClient fromAPIUser) => GitHubClientInstance = fromAPIUser;

    internal void InitializeGit(string? token = null)
    {
        GitHubClientInstance = new GitHubClient(new ProductHeaderValue(Terms.PHVText));
        if (!string.IsNullOrEmpty(token))
        {
            var tokenAuth = new Credentials(token);
            GitHubClientInstance.Credentials = tokenAuth;
        }
        
    }

    public static OctokitService Instance
        => Singleton<OctokitService>.Instance;
}