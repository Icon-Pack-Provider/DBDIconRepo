using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack.Model;
using Octokit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public partial class OnlineStarHelper : ObservableObject, IStar
{
    GitHubClient client => OctokitService.Instance.GitHubClientInstance;

    [ObservableProperty]
    string forOwner = "";

    [ObservableProperty]
    string forName = "";

    [ObservableProperty]
    ObservableCollection<PackRepositoryInfo> allStarred = new();
    public async Task<bool> IsRepoStarred(PackRepositoryInfo info)
    {
        //Check on list first
        if (AllStarred.Contains(info))
            return true;
        //Confirm on git
        return await client.Activity.Starring.CheckStarred(info.Owner, info.Name);
    }

    public async Task Initiallze()
    {
        //Get list of starred repos from Git
        var allOnline = await client.Activity.Starring.GetAllForCurrent();
        var withTag = allOnline.Select(r => r.Topics.Contains(IconPack.Resource.Terms.PackTag)).ToList();
        AllStarred = new(allOnline
            .Where(r => r.Topics.Any(topic => topic == IconPack.Resource.Terms.PackTag))
            .Select(repo => new PackRepositoryInfo(repo)));
    }

    public async Task Star(PackRepositoryInfo info)
    {
        //Add to list
        if (!AllStarred.Contains(info))
            AllStarred.Add(info);
        //Starred into account
        await client.Activity.Starring.StarRepo(info.Owner, info.Name);
    }

    public async Task UnStar(PackRepositoryInfo info)
    {
        //Remove from list first
        if (AllStarred.Contains(info))
            AllStarred.Remove(info);
        //Then update it online
        await client.Activity.Starring.RemoveStarFromRepo(info.Owner, info.Name);
    }
}