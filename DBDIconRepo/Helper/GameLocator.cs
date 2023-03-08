using GameFinder.StoreHandlers.EGS;
using GameFinder.StoreHandlers.Steam;
using System;

namespace DBDIconRepo.Helper;

public static class GameLocator
{
    const int SteamDBDID = 381210;
    public static string FindDBDOnSteam()
    {
        //Not support on other platform beside Windows
        if (!OperatingSystem.IsWindows())
            return string.Empty;

        var handler = new SteamHandler();

        var dbd = handler.FindOneGameById(SteamDBDID, out string[] errors);
        if (dbd is not null)
        {
            return dbd.Path;
        }
        return string.Empty;
    }

    const string EpigDBDID = "611482b8586142cda48a0786eb8a127c:467a7bed47ec44d9b1c9da0c2dae58f7:Brill";
    public static string FindDBDOnEpig()
    {
        //Not support on other platform beside Windows
        if (!OperatingSystem.IsWindows())
            return string.Empty;

        var handler = new EGSHandler();
        if (handler is null)
            return string.Empty;

        var dbd = handler.FindOneGameById(EpigDBDID, out string[] errors);
        if (dbd is not null)
        {
            return dbd.InstallLocation;
        }

        //Bruteforce as I'm not certain that long string of texts up there is actually DBD 
        foreach (var (game, error) in handler.FindAllGames())
        {
            if (game is not null)
            {
                //I can't tell if the display name gonna be spaced or not. Better safe than sorry
                //¯\_(ツ)_/¯
                bool d = game.DisplayName.ToLower().Contains("dead");
                bool b = game.DisplayName.ToLower().Contains("by");
                bool dd = game.DisplayName.ToLower().Contains("daylight");
                if (d && b && dd)
                {
                    return game.InstallLocation;
                }
            }
        }
        //No DBD found; probably...
        return string.Empty;
    }
}
