using IconPack.Model;
using Octokit;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using IconPack.Internal;
using IconPack.Internal.Helper;
using static IconPack.Resource.Terms;
using static IconPack.Internal.Flags;
using static IconPack.Internal.Helper.IOHelper;
using System.Text.Json;
using System.Text;
using System.Collections.ObjectModel;

namespace IconPack;

public static class Packs
{
    public static void Initialize(string gitToken, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton((svc) => new OctokitService(gitToken))
                .BuildServiceProvider());
            WorkingDirectory = workingDirectory;
        }
    }

    public static void Initialize(GitHubClient client, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddSingleton((svc) => new OctokitService(client))
                .BuildServiceProvider());
            WorkingDirectory = workingDirectory;
        }
    }

    public static async Task<ObservableCollection<Pack?>> GetPacks()
    {
        ThrowHelper.APINotInitialize();

        var request = new SearchRepositoriesRequest($"topic:{PackTag}");
        var result = await OctokitService.Instance.Client.Search.SearchRepo(request);

        var packs = new ObservableCollection<Pack?>();
        foreach (var repo in result.Items)
        {
            var pack = await GetPack(repo);
            if (repo.UpdatedAt.UtcDateTime > pack.LastUpdate
                || MissingInfo(pack))
            {
                pack = await UpdateOne(pack, repo);
            }

            packs.Add(pack);
        }

        return packs;
    }

    public static async Task<Pack?> GetPack(Repository repo)
    {
        ThrowHelper.APINotInitialize();
        if (OctokitService.Instance is null)
            return null;

        //Directory confirmation
        if (!Directory.Exists(WorkingDirectory))
            Directory.CreateDirectory(WorkingDirectory);

        //Local pack.json info
        bool hasLocalPackJson = IsLocalPackJSONExist(repo);
        if (hasLocalPackJson)
        {
            var jsonFile = GetLocalPackJson(repo);
            string json = File.ReadAllText(jsonFile.FullName);
            return JsonSerializer.Deserialize<Pack?>(json);
        }

        //Check on dah web
        bool hasRepoPackJson = await URL.IsPackJSONExist(repo);
        if (hasRepoPackJson)
        {
            try
            {
                byte[] jsonraw = Array.Empty<byte>();
                jsonraw = await OctokitService.Instance.Client.Repository.Content.GetRawContent(repo.Owner.Login, repo.Name, PackJson);
                string json = Encoding.UTF8.GetString(jsonraw);
                File.WriteAllText(GetLocalPackJson(repo).FullName, json);
                return JsonSerializer.Deserialize<Pack?>(json);
            }
            catch { }
        }

        //Create new and cached it
        var pack = await CreateOne(repo);
        var serialized = JsonSerializer.Serialize(pack, new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = false
        });
        var packLocal = GetLocalPackJson(repo);
        if (packLocal.Exists)
            File.Delete(packLocal.FullName);
        File.WriteAllText(packLocal.FullName, serialized);
        return pack;
    }

    public static async Task<Pack> CreateOne(Repository repo)
    {
        Pack info = new()
        {
            Name = repo.Name,
            Description = repo.Description,
            Author = repo.Owner.Login,
            URL = repo.HtmlUrl,
            LastUpdate = repo.UpdatedAt.UtcDateTime,

            Repository = new(repo),

            ContentInfo = await PackContentInfo.GetContentInfo(repo)
        };

        return info;
    }

    public static async Task<Pack> UpdateOne(Pack? previous, Repository repo)
    {
        Pack info = new()
        {
            Name = previous.Name ?? repo.Name,
            Description = previous.Name ?? repo.Description,
            Author = previous.Author ?? repo.Owner.Login,
            URL = previous.URL is not null ? previous.URL : repo.HtmlUrl,
            LastUpdate = repo.UpdatedAt.UtcDateTime,

            Repository = previous.Repository is not null ? previous.Repository : new(repo)
            {
                CloneUrl = repo.CloneUrl,
                DefaultBranch = repo.DefaultBranch,
                ID = repo.Id,
                Name = repo.Name,
                Owner = repo.Owner.Login
            },

            ContentInfo = (PackContentInfo?)(previous.ContentInfo ?? await PackContentInfo.GetContentInfo(repo))
        };
        string json = JsonSerializer.Serialize(info, new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = false
        });
        File.WriteAllText(GetLocalPackJson(repo).FullName, json);
        return info;
    }

    public static bool MissingInfo(Pack? info)
    {
        var properties = from p in typeof(Pack).GetProperties()
                         where p.CanRead &&
                               p.CanWrite
                         select p;

        foreach (var property in properties)
        {
            var value = property.GetValue(info, null);
            if (value is null)
                return true;
        }
        return false;
    }
}
