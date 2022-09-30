using IconPack.Model;
using System.Formats.Asn1;
using static IconPack.Resource.Terms;

namespace IconPack.Internal.Helper;

/// <summary>
/// Work with URL/Git
/// </summary>
internal class URL
{
    public static async Task<byte[]> GetBytes(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
    }

    public static async Task<bool> Exists(string? url)
    {
        url = EnsurePathIsWebURL(url);
        try
        {
            using HttpClient client = new();
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            if (response.IsSuccessStatusCode)
                return true;
            return false;
        }
        catch { return false; }
    }

    public static async Task<bool> IsBannerExist(string? owner, string? name, string? defaultBranch)
        => await Exists(GetGithubRawContent(owner, name, defaultBranch, BannerFile));

    public static string GetBannerURL(string? owner, string? name, string? defaultBranch)
        => GetGithubRawContent(owner, name, defaultBranch, BannerFile);

    public static async Task<bool> IsReadmeExist(string? owner, string? name, string? defaultBranch)
        => await Exists(GetGithubRawContent(owner, name, defaultBranch, ReadmeFile));

    public static async Task<bool> IsPackJSONExist(string? owner, string? name, string? defaultBranch)
        => await Exists(GetGithubRawContent(owner, name, defaultBranch, PackJson));


    #region Overloads

    public static async Task<bool> IsBannerExist(PackRepositoryInfo repository)
        => await Exists(GetGithubRawContent(repository, BannerFile));
    public static async Task<bool> IsBannerExist(Octokit.Repository repository)
        => await Exists(GetGithubRawContent(repository, BannerFile));

    public static string GetBannerURL(PackRepositoryInfo repository)
        => GetBannerURL(repository.Owner, repository.Name, repository.DefaultBranch);

    public static string GetBannerURL(Octokit.Repository repository)
        => GetBannerURL(repository.Owner.Login, repository.Name, repository.DefaultBranch);

    public static async Task<bool> IsReadmeExist(PackRepositoryInfo repository)
        => await Exists(GetGithubRawContent(repository, ReadmeFile));
    public static async Task<bool> IsReadmeExist(Octokit.Repository repository)
        => await Exists(GetGithubRawContent(repository, ReadmeFile));

    public static async Task<bool> IsPackJSONExist(PackRepositoryInfo repository)
        => await Exists(GetGithubRawContent(repository, PackJson));
    public static async Task<bool> IsPackJSONExist(Octokit.Repository repository)
        => await Exists(GetGithubRawContent(repository, PackJson));


    #endregion
    #region Private helpers
    private static string EnsurePathIsWebURL(string? input)
    {
        input = NullCheck(input);
        if (input.Contains('\\'))
            input = input.Replace('\\', '/');
        return input;
    }

    private static string NullCheck(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        return input;
    }

    public static string GetGithubRawContent(Octokit.Repository repository, string? path)
        => GetGithubRawContent(repository.Owner.Login, repository.Name, repository.DefaultBranch, path);

    public static string GetGithubRawContent(PackRepositoryInfo repository, string? path)
        => GetGithubRawContent(repository.Owner, repository.Name, repository.DefaultBranch, path);

    public static string GetGithubRawContent(string? owner, string? name, string? defaultBranch, string? path)
        => $"https://raw.githubusercontent.com/{owner}/{name}/{defaultBranch}/{EnsurePathIsWebURL(path)}";
    #endregion
}
