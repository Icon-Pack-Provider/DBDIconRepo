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
using CommunityToolkit.Mvvm.Messaging;
using IconPack.Model.Progress;

namespace IconPack;

public static class Packs
{
    public static void Initialize(string gitToken, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new(gitToken));
            WorkingDirectory = workingDirectory;
        }
    }

    public static void Initialize(GitHubClient client, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new(client));
            WorkingDirectory = workingDirectory;
        }
    }

    public static void Initialize(string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new());
            WorkingDirectory = workingDirectory;
        }
    }

    public static async Task<ObservableCollection<Pack?>> GetPacks(Action<string>? notifications = null)
    {
        ThrowHelper.APINotInitialize();

        var request = new SearchRepositoriesRequest($"topic:{PackTag}");
        var result = await OctokitService.Instance.Client.Search.SearchRepo(request);
        notifications?.Invoke($"Gather icon pack repositories\r\nFound {result.TotalCount} repositories");

        var packs = new ObservableCollection<Pack?>();
        foreach (var repo in result.Items)
        {
            notifications?.Invoke($"Processing icon pack infomation from {repo.Url}");
            var pack = await GetPack(repo);
            if (repo.UpdatedAt.UtcDateTime > pack.LastUpdate
                || IsMissingInfo(pack))
            {
                pack = await UpdateOne(pack, repo);
            }

            packs.Add(pack);
        }
        notifications?.Invoke("All repositories information has been gathered!");
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
            var deserialized = JsonSerializer.Deserialize<Pack?>(json);

            if (json.Contains(nameof(Pack.LastUpdate)))
            {
                //Last update on Json file is untrustworthy;
                //It's function is for program to write into its and store locally
                //Not store on GitHub repo
                deserialized.LastUpdate = DateTime.MinValue;
            }
            return deserialized;
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
        ThrowHelper.APINotInitialize();
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
        ThrowHelper.APINotInitialize();
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
        if (info.ContentInfo.Files is null || 
            (info.ContentInfo.Files is not null && info.ContentInfo.Files.Count < 1))
        {
            info.ContentInfo = await PackContentInfo.GetContentInfo(repo);
        }
        string json = JsonSerializer.Serialize(info, new JsonSerializerOptions()
        {
            WriteIndented = true,
            IncludeFields = false
        });
        File.WriteAllText(GetLocalPackJson(repo).FullName, json);
        return info;
    }

    public static bool IsMissingInfo(Pack? info)
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

    public static async Task CheckPackReadme(Pack? info)
    {
        ThrowHelper.APINotInitialize();
        var readmeState = IsLocalReadmeExist(info.Repository);
        if (readmeState is null)
        {
            //Readme state hasn't check yet
            //Check online readme
            var repoReadme = await URL.IsReadmeExist(info.Repository);
            //Set readme existance state
            SetLocalReadme(info.Repository, repoReadme);
            if (repoReadme)
            {
                //Save the content
                var readmeText = await OctokitService.Instance.Client.Repository.Content.GetReadme(info.Repository.ID);
                var readmeFile = GetLocalReadme(info.Repository);
                if (!readmeFile.Exists)
                    readmeFile.Create();
                File.WriteAllText(readmeFile.FullName, readmeText.Content);
            }
        }
    }

    public static async Task<string?> GetPackReadme(Pack? info)
    {
        ThrowHelper.APINotInitialize();
        var isReadmeExist = IsLocalReadmeExist(info.Repository);
        if (isReadmeExist is null)
            await CheckPackReadme(info);
        if (isReadmeExist.Value)
        {
            var readme = GetLocalReadme(info.Repository);
            return File.ReadAllText(readme.FullName);
        }
        return null;
    }

    public static async Task<bool> IsPackReadmeExist(Pack? info)
    {
        var readme = IsLocalReadmeExist(info.Repository);
        if (readme is null)
            return await URL.IsReadmeExist(info.Repository);
        return readme.Value;
    }

    public static async Task CheckPackBanner(Pack? info)
    {
        ThrowHelper.APINotInitialize();
        var bannerState = IsLocalBannerExist(info.Repository);
        if (bannerState is null)
        {
            //Check online
            var repoBanner = await URL.IsBannerExist(info.Repository);
            //Set state
            SetLocalBanner(info.Repository, repoBanner);
            //Then save the banner?
            //if (repoBanner)
            //{
            //    //Save the content
            //    var bannerURL = URL.GetBannerURL(info.Repository);
            //    var bannerBytes = await URL.GetBytes(bannerURL);
            //    var bannerFile = GetLocalBanner(info.Repository);
            //    if (!bannerFile.Exists)
            //        bannerFile.Create();
            //    File.WriteAllBytes(bannerFile.FullName, bannerBytes);
            //}
        }
    }

    /// <summary>
    /// Return pack .banner.png URL on repo
    /// Return null if the repo doesn't have banner
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static async Task<string?> GetPackBannerURL(Pack? info)
    {
        ThrowHelper.APINotInitialize();
        var bannerState = IsLocalBannerExist(info.Repository);
        if (bannerState is null)
            await CheckPackBanner(info);
        if (bannerState.Value)
        {
            //Get banner URL
            return URL.GetBannerURL(info.Repository);
        }
        return null;
    }

    public static async Task<bool> IsPackBannerExist(Pack? info)
    {
        var banner = IsLocalBannerExist(info.Repository);
        if (banner is null)
            return await URL.IsBannerExist(info.Repository);
        return banner.Value;
    }

    public static string? GetPackItemOnGit(Pack? info, string path)
    {
        if (path.Contains('\\'))
            path = path.Replace('\\', '/');
        return URL.GetGithubRawContent(info.Repository, path);
    }

    public static async Task ClonePackToCacheFolder(Pack? info, Action<ICloningProgress> progress)
    {
        bool isDotGitExist = IsRepoCloneDirectoryDotGitExist(info.Repository);
        nukeAndClone:
        if (!isDotGitExist)
        {
            //Delete directory, and clone
            var directory = GetRepoCloneDirectory(info.Repository);
            if (directory.Exists)
                directory.Delete(true); //Delete everything inside, prevent clone error
            directory.Create();
            await Task.Run(() =>
            {
                LibGit2Sharp.Repository.Clone(info.Repository.CloneUrl, directory.FullName, new LibGit2Sharp.CloneOptions()
                {
                    CredentialsProvider = (_url, _usr, _crd) => OctokitService.GetLibGitCredential(info.Repository.Owner),
                    IsBare = false,
                    OnProgress = (serverProc) => Report.ServerProgress(serverProc, info.Repository.Name, progress),
                    OnTransferProgress = (transfer) => Report.TransferProgress(info.Repository.Name, transfer, progress),
                    OnCheckoutProgress = (path, complete, total) => Report.CheckoutProgress(info.Repository.Name, progress, path, complete, total)
                });
            });
            progress?.Invoke(new EOver(info.Repository.Name));
            return;
        }
        else
        {
            //Check if it's latest and reset if it's not match repo
            var dir = GetRepoFetchHead(info.Repository);
            if (!dir.Exists)
            {
                isDotGitExist = false; //Force false to force clone
                goto nukeAndClone;
            }
            if (dir.LastWriteTimeUtc < (DateTime.UtcNow + TimeSpan.FromDays(1)))
            {
                //Less than 1 day clone, probably new. Return 
                progress?.Invoke(new EOver(info.Repository.Name));
                return;
            }
            await Task.Run(() =>
            {
                var directory = GetRepoCloneDirectory(info.Repository);
                using var libGitRepo = new LibGit2Sharp.Repository(directory.FullName);
                //Reset folder to match remote
                var mainbranch = libGitRepo.Branches[$"origin/{info.Repository.DefaultBranch}"];
                libGitRepo.Reset(LibGit2Sharp.ResetMode.Hard, mainbranch.Tip);
            });
            //Report its done fetching
            progress?.Invoke(new EOver(info.Repository.Name));
        }
    }    

    public static DirectoryInfo GetPackCacheClonedFolder(Pack? info)
    {
        return GetRepoCloneDirectory(info.Repository);
    }
}
