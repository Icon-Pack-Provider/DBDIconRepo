using DBDIconRepo.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Helper
{
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
        public static void Uninstall(string dbdPath)
        {
            DirectoryInfo info = new DirectoryInfo($"{dbdPath}\\DeadByDaylight\\Content\\UI\\Icons");
            var files = info.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (ignoreList.Contains(file.Name))
                    continue;
                if (ignoreDirectory.Contains(file.Directory.Name))
                    continue;

                file.Delete();
            }
        }

        public static async void Install(string dbdPath, IList<IPackSelectionItem> selections, IconPack.Model.Pack? packInfo)
        {
            foreach (var item in selections)
            {
                if (item.IsSelected != true)
                    continue;
                //Report currently install file
                Messenger.Default.Send(new InstallationProgressReportMessage(item.Name, selections.Count), $"{MessageToken.REPORTINSTALLPACKTOKEN}{packInfo.Repository.Name}");

                string iconPath = CacheOrGit.GetContentPath(packInfo.Repository.Owner, packInfo.Repository.Name, item.FullPath);
                string targetPath = $"{dbdPath}\\DeadByDaylight\\Content\\UI\\Icons\\{item.FilePath}";
                FileInfo info = new FileInfo(targetPath);
                if (!Directory.Exists(info.DirectoryName))
                    Directory.CreateDirectory(info.DirectoryName);

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                    //File.OpenRead(@"C:test.bin");
                    //string hash = BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(FileOptions)); 
                }
                //File.Copy broke :/
                var stream = await File.ReadAllBytesAsync(iconPath);
                using FileStream fs = new(targetPath, FileMode.Create, FileAccess.Write);
                fs.Write(stream, 0, stream.Length);
            }
        }

    }
}
