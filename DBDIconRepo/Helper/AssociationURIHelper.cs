using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace DBDIconRepo.Helper;

public static class AssociationURIHelper
{
    public const string AppURI = "dbdiconrepo";

    public static void RegisterAppURI()
    {
        if (!OperatingSystem.IsWindows())
            return;
        using var key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Classes\\{AppURI}");
        key.SetValue("", $"URL:{AppURI}");
        key.SetValue("URL Protocol", "");

        string actualExecutable = GetAppExecutableLocation();
        
        using var defaultIcon = key.CreateSubKey("DefaultIcon");
        defaultIcon.SetValue("", $"{actualExecutable},1");

        using var commandKey = key.CreateSubKey(@"shell\open\command");
        commandKey.SetValue("", $"\"{actualExecutable}\" \"%1\"");
    }

    public static bool IsRegistered()
    {
        if (!OperatingSystem.IsWindows())
            return false;
        if (Registry.CurrentUser.OpenSubKey($"SOFTWARE\\Classes\\{AppURI}") is RegistryKey key)
        {
            var executableLocation = GetAppExecutableLocation();
            var sub = key.OpenSubKey(@"shell\open\command");
            //Correct execution folder?
            var setExecutableLocation = sub.GetValue("").ToString();
            setExecutableLocation = setExecutableLocation.Substring(1);
            if (!setExecutableLocation.StartsWith(executableLocation))
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public static string GetAppExecutableLocation()
    {
        string appLocation = typeof(App).Assembly.Location;
        FileInfo fif = new(appLocation);
        string actualExecutable = $"{fif.Directory}\\{fif.NameOnly()}.exe";
        return actualExecutable;
    }
}
