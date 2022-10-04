using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;
using IconPack.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public partial class LocalStarHelper : ObservableObject, IStar
{
    private const string LocalStarFolder = "LocalStar";
    private const string LocalStarMarker = "⭐.yes";

    [ObservableProperty]
    string forOwner = "Anonymous";

    [ObservableProperty]
    string forName = Environment.UserName;

    [ObservableProperty]
    ObservableCollection<PackRepositoryInfo> allStarred = new();
    public async Task<bool> IsRepoStarred(PackRepositoryInfo info)
    {
        //Check on list first
        if (AllStarred.Contains(info))
            return true;
        //Confirm on disk
        string path = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory, LocalStarFolder,
            info.Owner, info.Name);
        var folder = new DirectoryInfo(path);
        if (!folder.Exists)
        {
            return false;
        }

        string marker = Path.Join(path, LocalStarMarker);
        FileInfo file = new(marker);
        if (!file.Exists)
            return false;
        //This code only here to make this async error dissapear
        await Task.Delay(1);
        //This repo is indeed starred
        return true;
    }

    public async Task Initiallze()
    {
        string path = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory, LocalStarFolder);
        DirectoryInfo dir = new(path);
        var starInfo = dir.GetFiles("*.yes", SearchOption.AllDirectories);
        AllStarred = new();
        foreach (var file in starInfo)
        {
            string json = await File.ReadAllTextAsync(file.FullName);
            var deseralized = JsonSerializer.Deserialize<PackRepositoryInfo>(json);
            AllStarred.Add(deseralized);
        }
    }

    public async Task Star(PackRepositoryInfo info)
    {
        //Add to list
        if (!AllStarred.Contains(info))
            AllStarred.Add(info);
        //Write to disk
        string path = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory, LocalStarFolder,
            info.Owner, info.Name);
        var folder = new DirectoryInfo(path);
        if (!folder.Exists)
        {
            folder.Create();
        }

        string marker = Path.Join(path, LocalStarMarker);
        FileInfo file = new(marker);
        if (!file.Exists)
        {
            string json = JsonSerializer.Serialize(info);
            await File.WriteAllTextAsync(file.FullName, json);
        }
    }

    public async Task UnStar(PackRepositoryInfo info)
    {
        //Remove from list first
        if (AllStarred.Contains(info))
            AllStarred.Remove(info);
        //Then on list
        string path = Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory, LocalStarFolder,
            info.Owner, info.Name);
        var folder = new DirectoryInfo(path);

        string marker = Path.Join(path, LocalStarMarker);
        FileInfo file = new(marker);
        if (file.Exists)
        {
            file.Delete();
            await Task.Delay(1);
            folder.Delete();
        }
    }
}