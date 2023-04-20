using DBDIconRepo.Model.Uploadable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.Helper.Uploadable;

public static class UploadableExtensions
{
    /// <summary>
    /// Get all paths of UploadableFile that is selected
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetAllSelectedPaths(this ObservableCollection<IUploadableItem> root)
    {
        if (root is null) //Is it null?
            return Array.Empty<string>();
        if (root.Count <= 0) //Is it empty?
            return Array.Empty<string>();

        return HelpGetAllSelectedPaths(root);
    }

    private static IEnumerable<string> HelpGetAllSelectedPaths(ObservableCollection<IUploadableItem> subItems)
    {
        foreach (var item in subItems) 
        {
            if (item is UploadableFile file && !Equals(file.IsSelected, false))
            {
                yield return file.FilePath;
            }
            else if (item is UploadableFolder sub && !Equals(sub.IsSelected, false))
            {
                foreach (var si in HelpGetAllSelectedPaths(sub.SubItems))
                    yield return si;
            }
        }
    }

    /// <summary>
    /// Search for UploadableFile
    /// </summary>
    /// <param name="root"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IUploadableItem? SearchFile(this ObservableCollection<IUploadableItem> root, string name)
    {
        if (root is null) //Is it null?
            return null;
        if (root.Count <= 0) //Is it empty?
            return null;

        return HelpFind(name, root);
    }

    private static IUploadableItem? HelpFind(string name, ObservableCollection<IUploadableItem>? subItems = null)
    {
        foreach (var item in subItems)
        {
            if (item.IsSelected == false)
                continue;
            if (item is UploadableFolder sub)
            {
                var res = HelpFind(name, sub.SubItems);
                if (res is not null)
                    return res;
                continue;
            }
            if (item.Name == name)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// Get one of random icon that is selected
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public static UploadableFile? GetRandomIcon(this ObservableCollection<IUploadableItem> root)
    {
        if (root is null) //Is it null?
            return null;
        if (root.Count <= 0) //Is it empty?
            return null;
        reroll:
        int index = Random.Shared.Next(0, root.Count);
        if (root[index] is UploadableFolder folder)
        {
            if (Equals(folder.IsSelected, false))
                goto reroll;
            return GetRandomIcon(folder.SubItems);
        }

        if (Equals(root[index].IsSelected, false))
            goto reroll;

        if (root[index] is UploadableFile file && file.IsSelected == true)
            return file;
        return null;
    }

    /// <summary>
    /// Search for existance of specific main folder, eg.Perks, CharPortraits etc.
    /// </summary>
    /// <param name="root"></param>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public static bool IsMainFolderExist(this ObservableCollection<IUploadableItem> root, string folderName)
    {
        foreach (var item in root)
        {
            if (item is UploadableFolder folder && folder.Name == folderName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Set expand state of every folder
    /// </summary>
    /// <param name="root"></param>
    /// <param name="state"></param>
    public static void SetExpansionState(this ObservableCollection<IUploadableItem> root, bool state)
    {
        if (root is null)
            return;
        if (root.Count <= 0)
            return;

        foreach (var item in root)
        {
            if (item is UploadableFile)
                continue;
            if (item is UploadableFolder folder && folder.SubItems is not null && folder.SubItems.Count > 0)
                SetExpansionState(folder.SubItems, state);
            item.IsExpand = state;
        }
    }

    /// <summary>
    /// Set selection state of every single items
    /// </summary>
    /// <param name="root"></param>
    /// <param name="state"></param>
    public static void SetSelectionState(this ObservableCollection<IUploadableItem> root, bool state)
    {
        if (root is null)
            return;
        if (root.Count <= 0)
            return;

        foreach (var item in root)
        {
            if (item is UploadableFolder folder && folder.SubItems is not null && folder.SubItems.Count > 0)
                SetSelectionState(folder.SubItems, state);
            item.IsSelected = state;
        }
    }

    const string bannerText = "Icon pack banner";
    public static void AddOrUpdateBanner(this ObservableCollection<IUploadableItem> root, string bannerPath)
    {
        if (root.FirstOrDefault(i => i.DisplayName == bannerText) is not UploadableFile file)
        {
            root.Add(new UploadableFile()
            {
                Name = ".banner",
                DisplayName = bannerText,
                FilePath = bannerPath,
                IsSelected = true,
                IsExpand = true
            });
            return;
        }
        file.FilePath = bannerPath;
    }

    public static bool IsBannerExist(this ObservableCollection<IUploadableItem> root)
    {
        if (root.FirstOrDefault(i => i.DisplayName == bannerText) is UploadableFile file)
            return true;
        return false;
    }

    public static IUploadableItem? GetBanner(this ObservableCollection<IUploadableItem> root)
    {
        if (root.FirstOrDefault(i => i.DisplayName == bannerText) is UploadableFile file)
            return file;
        return null;
    }
}
