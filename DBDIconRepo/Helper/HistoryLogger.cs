using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using IconInfo.Icon;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DBDIconRepo.Service;

public static class HistoryLogger
{
    private const string historyDir = "History";
    private static DirectoryInfo GetHistoryDirectory()
    {
        string path = SettingManager.Instance.CacheAndDisplayDirectory;
        path = Path.Combine(path, historyDir);
        DirectoryInfo dir = new(path);
        if (!dir.Exists)
            dir.Create();
        return dir;
    }

    public static ObservableCollection<IHistoryItem>? LoadHistory()
    {
        //Check disk
        var dir = GetHistoryDirectory();
        var entries = dir.GetFiles();
        if (entries.Length < 1)
            return new();
        ObservableCollection<IHistoryItem>? items = new();
        foreach (var entry in entries)
        {
            var json = File.ReadAllText(entry.FullName);
            var item = JsonSerializer.Deserialize<IHistoryItem>(json);
            items.Add(item);
        }
        return items;

    }

    public static void SaveHistory(IHistoryItem? item)
    {
        if (!SettingManager.Instance.SaveHistory)
            return;
        if (item is null)
            return;
        //Check disk
        var dir = GetHistoryDirectory();
        var entries = dir.GetFiles();
        if (entries.FirstOrDefault(entry => Equals(entry.NameOnly(), item.Victim.Repository.ID.ToString())) is FileInfo file)
        {
            file.Delete();
        }

        using var writer = file.CreateText();
        string json = JsonSerializer.Serialize(item, item.GetType(), new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        writer.Write(json);
    }
}

public partial class HistoryInstallPack : ObservableObject, IHistoryItem
{
    [ObservableProperty]
    private Pack? victim = null;

    [ObservableProperty]
    private HistoryType action;

    [ObservableProperty]
    private DateTime time;

    [ObservableProperty]
    private List<IPackSelectionItem>? installedIcons;
}

public partial class HistoryViewPack : ObservableObject, IHistoryItem
{
    [ObservableProperty]
    private Pack? victim = null;

    [ObservableProperty]
    private HistoryType action;

    [ObservableProperty]
    private DateTime time;
}

public interface IHistoryItem
{
    DateTime Time { get; set; }
    Pack Victim { get; set; }
    HistoryType Action { get; set; }
}

public enum HistoryType
{
    ViewDetail,
    Install
}