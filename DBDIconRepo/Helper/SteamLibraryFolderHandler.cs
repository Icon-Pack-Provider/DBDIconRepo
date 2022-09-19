using System;
using System.Collections.Generic;
using System.Linq;

namespace DBDIconRepo.Helper
{
    public static class SteamLibraryFolderHandler
    {
        const string DBDAppID = "381210";
        public static string GetDeadByDaylightPath(string steamLibFolder)
        {
            List<string> splits = steamLibFolder.Split(separator:'\n', options:StringSplitOptions.RemoveEmptyEntries).ToList();
            int dbdIndex = -1;
            //Look down
            for (int i = 0; i < splits.Count; i++)
            {
                splits[i] = splits[i].Trim();
                if (splits[i].StartsWith($"\"{DBDAppID}"))
                {
                    dbdIndex = i;
                    break;
                }
            }
            string actualPath = "";
            //Look up
            if (dbdIndex < 0) //No DBD found on any steam folders
                return "";

            for (int i = dbdIndex;i > 0; i--)
            {
                if (splits[i].StartsWith("\"path"))
                {
                    actualPath = splits[i].Split("\t\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                    actualPath = actualPath.Replace("\\\\", "\\");
                    break;
                }
            }

            if (string.IsNullOrEmpty(actualPath)) //No DBD found on any steam folders
                return "";

            return $"{actualPath}\\steamapps\\common\\Dead by Daylight";
        }
    }
}
