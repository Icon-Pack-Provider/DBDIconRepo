using DBDIconRepo.Model;
using IconPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

}

/// <summary>
/// Indicate the current pack need to clone again
/// </summary>
public class RepoNotUpdatedException : Exception
{

}