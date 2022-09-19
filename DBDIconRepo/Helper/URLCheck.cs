using System.Net.Http;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public static class URL
{
    public static async Task<bool> Exists(string? url)
    {
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

    public static async Task<bool> IsContentExists(Octokit.Repository? repo, string? path) => await Exists(GetGithubRawContent(repo, path));

    public static async Task<bool> IsContentExists(IconPack.Model.PackRepositoryInfo? repo, string? path)
        => await Exists(GetGithubRawContent(repo, path));


    public static string GetGithubRawContent(Octokit.Repository? repo, string? path)
        => GetGithubRawContent(repo.Owner.Login, repo.Name, repo.DefaultBranch, path);

    public static string GetGithubRawContent(IconPack.Model.PackRepositoryInfo? repo, string? path)
        => GetGithubRawContent(repo.Owner, repo.Name, repo.DefaultBranch, path);

    public static string GetGithubRawContent(string? owner, string? name, string? defaultBranch, string? path)
        => $"https://raw.githubusercontent.com/{owner}/{name}/{defaultBranch}/{path}";

    public static string GetIconAsGitRawContent(IconPack.Model.PackRepositoryInfo? repo, string? path)
        => GetGithubRawContent(repo, EnsurePathIsForURL(path));

    public static string EnsurePathIsForURL(string? match)
    { 
        //In case of "\\Perks\\DLC5\\iconPerks_DeadHard.png" or "/Perks/DLC5/iconPerks_DeadHard.png"
        if (match.StartsWith("/") || match.StartsWith("\\"))
            match = match.Substring(1);
        //In case of "Perks\\DLC5\\iconPerks_DeadHard.png"
        if (match.Contains("\\"))
            match = match.Replace("\\", "/");
        return match;
    }

    public static void OpenURL(string? url)
    {
        if (url is null) return;
        if (!url.StartsWith("https"))
            //Disallow from open anything else, beside link
            return;

        var pif = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true };
        System.Diagnostics.Process.Start(pif);
    }
}