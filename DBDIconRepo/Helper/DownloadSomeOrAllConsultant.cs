using DBDIconRepo.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.Helper;

public static class DownloadSomeOrAllConsultant
{
    internal static bool ShouldCloneOrNot(ObservableCollection<IPackSelectionItem>? installPick)
    {
        bool shouldCloneOrDownload = false;
        if (Setting.Instance.AlwaysClonePackRepo)
            shouldCloneOrDownload = true;
        if (!shouldCloneOrDownload) //Last time decide
            shouldCloneOrDownload = ShouldIDownloadOrCloneRepo(installPick);

        return shouldCloneOrDownload;
    }

    /// <summary>
    /// Decide whether to clone repo or just download each items manually
    /// </summary>
    private static bool ShouldIDownloadOrCloneRepo(ObservableCollection<IPackSelectionItem>? installPick)
    {
        var selection = installPick.Select(i => i.IsSelected).DistinctBy(i => i);
        if (selection.Count() == 1)
        {
            //One single decistion, download or not.
            //And it is...
            return selection.First().Value;
        }

        //There's a mixed selection
        double total = Convert.ToDouble(installPick.Count());
        double totalSelection = Convert.ToDouble(installPick.Where(i => i.IsSelected == true).Count());

        double math = totalSelection / total;

        if (math > Setting.Instance.DownloadIfSelectMoreThanMeThreshold)
        {
            //Just clone
            return true;
        }

        //Manually download
        return false;
    }
}
