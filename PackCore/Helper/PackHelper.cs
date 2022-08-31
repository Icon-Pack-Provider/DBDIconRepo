using DebounceThrottle;
using IconPack.Model;
using IconPack.Service;
using IconPack.Strings;
using Octokit;
using System.Collections.ObjectModel;

namespace IconPack.Helper;

public static class PackHelper
{
    private static ThrottleDispatcher _throttler;
    public static async Task<ObservableCollection<Pack>> GetPacks()
    {
        var packs = new ObservableCollection<Pack>();
        _throttler = new(5000);
        await _throttler.ThrottleAsync(async () =>
        {
            packs = await GetPacksAsync();
        });
        return packs;
    }

    private static async Task<ObservableCollection<Pack>> GetPacksAsync()
    {
        var repos = await SearchGit();
        ObservableCollection<Pack> packs = new();

        foreach (var item in repos)
        {
            packs.Add(new()
            {
                Name = item.Name,
                Description = item.Description,
                URL = item.Url,
                LastUpdate = item.UpdatedAt.UtcDateTime,
                Authors = new((await OctokitService.Instance.GitHubClientInstance.Repository.Collaborator.GetAll(item.Id)).Select(c => c.Login)),
                ContentInfo = await PackContentInfo.GetContentInfo(OctokitService.Instance.GitHubClientInstance, item),
                Repository = new(item)
            });
        }
        return packs;
    }

    private static async Task<ObservableCollection<Repository>> SearchGit()
    {
        if (OctokitService.Instance.GitHubClientInstance is null)
            throw new NullReferenceException("Github service didn't initialize\r\nNo token provided");

        var request = new SearchRepositoriesRequest($"topic:{Terms.PackTag}");
        var result = await OctokitService.Instance.GitHubClientInstance.Search.SearchRepo(request);
        return new ObservableCollection<Repository>(result.Items);
    }
}
