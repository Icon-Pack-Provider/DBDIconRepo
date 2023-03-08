using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public static ObservableCollection<IHistoryItem> LoadHistory()
    {
        //Check disk
        var dir = GetHistoryDirectory();
        var entries = dir.GetFiles();
        if (entries.Length < 1)
            return new();
        ObservableCollection<IHistoryItem> items = new();
        foreach (var entry in entries)
        {
            var json = File.ReadAllText(entry.FullName);
            items.Add(JsonSerializer.Deserialize<IHistoryItem>(json));
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
        var entries = dir.GetFiles(item.Victim.ToString());
        if (entries.FirstOrDefault() is FileInfo file)
            file.Delete();


        using var writer = new StreamWriter($"{dir.FullName}\\{item.Action}_{item.Victim}");
        string json = JsonSerializer.Serialize(item, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        writer.Write(json);
    }
}

public partial class HistoryInstallPack : HistoryViewPack
{
    [ObservableProperty]
    //Keep only Root/Folder/[sub]/Filename
    private List<string?> installedIcons = new();

    public HistoryInstallPack(Pack? pack, IList<IPackSelectionItem> installed) : base(pack)
    {
        InstalledIcons = new(installed.Where(i => i.IsSelected == true).Select(i => i.Info?.File));
    }

    public HistoryInstallPack() { }
}

public partial class HistoryViewPack : ObservableObject, IHistoryItem
{
    [ObservableProperty]
    private long victim;

    [ObservableProperty]
    private HistoryType action;

    [ObservableProperty]
    private DateTime time;

    public HistoryViewPack(Pack? pack)
    {
        Victim = pack.Repository.ID;
        Action = HistoryType.ViewDetail;
        Time = DateTime.Now;
    }

    public HistoryViewPack() { }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "HistoryType")]
[JsonDerivedType(typeof(HistoryInstallPack), nameof(HistoryInstallPack))]
[JsonDerivedType(typeof(HistoryViewPack), nameof(HistoryViewPack))]
public interface IHistoryItem
{
    DateTime Time { get; set; }
    /// <summary>
    /// Repository ID
    /// </summary>
    long Victim { get; set; }
    HistoryType Action { get; set; }
}

public enum HistoryType
{
    ViewDetail,
    Install
}