using System;
using System.Linq;

namespace DBDIconRepo.Helper;

public static class VersionHelper
{
    /// <summary>
    /// Year.Month.Day.Revision
    /// </summary>
    public const string Version = "2023.04.04.1";

    /// <summary>
    /// Check if input version is newer
    /// </summary>
    /// <param name="version">Version input from git release tag</param>
    /// <returns>True: Newer version available | False: Updated</returns>
    public static bool IsNewer(string version)
    {
        var currentSplices = Version.Split('.').Select(int.Parse).ToList();
        DateTime current_r = new(currentSplices[0], currentSplices[1], currentSplices[2]);
        int current_rev = 0;
        if (currentSplices.Count >= 4)
            current_rev = currentSplices[3];

        var checkedSplices = version.Split('.').Select(int.Parse).ToList();
        DateTime checked_r = new(checkedSplices[0], checkedSplices[1], checkedSplices[2]);
        int checked_rev = 0;
        if (checkedSplices.Count >= 4)
            checked_rev = checkedSplices[3];

        if (!Equals(checked_r, current_r))
        {
            //Different day release
            //Return if checked is newer than current
            return checked_r > current_r;
        }
        else
        {
            //Same day release
            return checked_rev > current_rev; //Check whether its newer
        }
    }
}
