using DBDIconRepo.Model;
using IconInfo.Internal;
using IconPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Helper;

public static class IconManager
{
    static string[] ignoreDirectory = new string[]
    {
        "Banners",
        "NewContentSplash"
    };

    static string[] ignoreList = new string[]
    {
        "empty.png",
        "Missing.png",
        "Anniversary_Hidden.png",
        "hidden.png",
        "categoryIcon_outfits_lg.png"
    };
    public static bool Uninstall(string dbdPath)
    {
        try
        {
            DirectoryInfo info = new DirectoryInfo(dbdPath);
            if (!dbdPath.Contains("UI") && !dbdPath.Contains("Icons"))
            {
                info = new($"{dbdPath}\\DeadByDaylight\\Content\\UI\\Icons");
            }
            
            var files = info.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (ignoreList.Contains(file.Name))
                    continue;
                if (ignoreDirectory.Contains(file.Directory.Name))
                    continue;

                file.Delete();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async void Install(string dbdPath, IList<IPackSelectionItem> selections, IconPack.Model.Pack? packInfo)
    {
        foreach (var item in selections)
        {
            if (item.IsSelected != true)
                continue;
            //Report currently install file
            Messenger.Default.Send(new InstallationProgressReportMessage(item.FullPath, selections.Count), $"{MessageToken.REPORTINSTALLPACKTOKEN}{packInfo.Repository.Name}");

            //string iconPath = CacheOrGit.GetContentPath(packInfo.Repository.Owner, packInfo.Repository.Name, item.FullPath);
            var iconFolder = Packs.GetPackCacheClonedFolder(packInfo);
            string iconPath = Path.Join(iconFolder.FullName, item.FilePath);
            string targetPath = dbdPath;
            string extensionPath = "\\DeadByDaylight\\Content\\UI\\Icons\\";
            if (!targetPath.Contains(extensionPath))
                targetPath = Path.Join(targetPath, extensionPath, item.FilePath);
            else
                targetPath = Path.Join(targetPath, item.FilePath); //Incase some user manually put in extension path on setting
            FileInfo info = new FileInfo(targetPath);
            if (!Directory.Exists(info.DirectoryName))
                Directory.CreateDirectory(info.DirectoryName);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
            try
            {
                var stream = await File.ReadAllBytesAsync(iconPath);
                using FileStream fs = new(targetPath, FileMode.Create, FileAccess.Write);
                fs.Write(stream, 0, stream.Length);
            }
            catch (DirectoryNotFoundException noDir)
            {
                //Re-clone???
                throw new RepoNotUpdatedException();
            }
            catch (FileNotFoundException noFile)
            {
                throw new RepoNotUpdatedException();
            }
        }
    }

    public static async Task<bool> OrganizeIcon(string rootDirectory, string iconPath)
    {
        var info = new FileInfo(iconPath);
        if (info.Extension != ".png")
            return false;
        if (iconPath.Contains("iconAction_carriedBody"))
        {
            //Move to actions
            if (info?.Directory?.Name == "Actions") //Already sorted
                return true;
            //Create "Actions" on rootDirectory
            DirectoryInfo actionsFolder = new(Path.Combine(rootDirectory, "Actions"));
            if (!actionsFolder.Exists)
                actionsFolder.Create();
            await Task.Run(() =>
            {
                info?.MoveTo(Path.Join(actionsFolder.FullName, info.Name));
            });
        }
        //Move
        var name = Path.GetFileNameWithoutExtension(iconPath);
        IBasic? iconInfo = default;
        IFolder? iconFolder = default;
        try { iconInfo = IconTypeIdentify.FromPath(iconPath); }
        catch { }
        if (iconInfo is null || iconInfo is not IBasic)
        {
            try { iconInfo = IconTypeIdentify.FromFile(iconPath); }
            catch { }
        }
        if (iconInfo is UnknownIcon)
            return false;
        if (iconInfo is IFolder subFolder)
        {
            iconFolder = subFolder;
        }

        //Type
        Type typeKey = iconInfo?.GetType() ?? typeof(UnknownIcon);
        bool haveKey = TypeToFolder.ContainsKey(typeKey);
        if (!haveKey)
            return false;
        string typeText = haveKey ? TypeToFolder[typeKey] : string.Empty;

        //SubFolder
        string subFolderText = iconFolder?.Folder ?? string.Empty;
        string finalPath = Path.Join(rootDirectory, typeText, subFolderText, info?.Name);

        FileInfo finalFile = new(finalPath);
        DirectoryInfo finalDirectory = new(finalFile?.Directory?.FullName ?? string.Empty);
        if (!finalDirectory.Exists)
            finalDirectory.Create();

        await Task.Run(() =>
        {
            info?.MoveTo(finalPath);
        });
        return true;
    }

    public static Dictionary<Type, string> TypeToFolder => new()
    {
        { typeof(IconInfo.Icon.Addon), "ItemAddons" },
        { typeof(IconInfo.Icon.DailyRitual), "DailyRituals" },
        { typeof(IconInfo.Icon.Portrait), "CharPortraits" },
        { typeof(IconInfo.Icon.Emblem), "Emblems" },
        { typeof(IconInfo.Icon.Offering), "Favors" },
        { typeof(IconInfo.Icon.Item), "items" },
        { typeof(IconInfo.Icon.Perk), "Perks" },
        { typeof(IconInfo.Icon.Power), "Powers" },
        { typeof(IconInfo.Icon.StatusEffect), "StatusEffects" },
        { typeof(IconInfo.Icon.Archive), "Archive" },
        { typeof(IconInfo.Icon.AuricCellPack), "Packs" },
        { typeof(IconInfo.Icon.Help), "Help" },
        { typeof(IconInfo.Icon.HelpLoading), "HelpLoading" },
        { typeof(IconInfo.Icon.StoreBackground), "StoreBackgrounds" }
    };
}

/// <summary>
/// Indicate the current pack need to clone again
/// </summary>
public class RepoNotUpdatedException : Exception
{

}