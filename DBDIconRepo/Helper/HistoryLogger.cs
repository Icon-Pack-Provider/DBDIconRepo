using CommunityToolkit.Mvvm.ComponentModel;
using DBDIconRepo.Model;
using DBDIconRepo.Model.History;
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
        string json = JsonSerializer.Serialize<IHistoryItem>(item, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        writer.Write(json);
    }
}