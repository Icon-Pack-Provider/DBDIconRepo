using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using GameFinder.Common;
using GameFinder.RegistryUtils;
using ValveKeyValue;
using Result = GameFinder.Common.Result<GameFinder.StoreHandlers.SteamGame>;

namespace GameFinder.StoreHandlers;

/// <summary>
/// Represents a game installed with Steam.
/// </summary>
/// <param name="AppId"></param>
/// <param name="Name"></param>
/// <param name="Path"></param>
public record SteamGame(int AppId, string Name, string Path);

/// <summary>
/// Handler for finding games installed with Steam.
/// </summary>
public class Steam : AHandler<SteamGame, int>
{
    internal const string RegKey = @"Software\Valve\Steam";

    private readonly IRegistry? _registry;

    /// <summary>
    /// Something, something, only windows have Registry
    /// </summary>
    public Steam()
    {
#if WINDOWS
        _registry = new WindowsRegistry();
#endif
    }

    /// <inheritdoc/>
    public override IEnumerable<Result> FindAllGames()
    {
        var (libraryFoldersFile, steamSearchError) = FindSteam();
        if (libraryFoldersFile is null)
        {
            yield return new Result(null, steamSearchError ?? "Unable to find Steam!");
            yield break;
        }

        var libraryFolderPaths = ParseLibraryFoldersFile(libraryFoldersFile);
        if (libraryFolderPaths is null || libraryFolderPaths.Count == 0)
        {
            yield return new Result(null, $"Found no Steam Libraries in {libraryFoldersFile.FullName}");
            yield break;
        }

        foreach (var libraryFolderPath in libraryFolderPaths)
        {
            var libraryFolder = new DirectoryInfo(libraryFolderPath);
            if (!libraryFolder.Exists)
            {
                yield return new Result(null, $"Steam Library {libraryFolder.FullName} does not exist!");
                continue;
            }

            var acfFiles = libraryFolder
                .EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly)
                .ToArray();

            if (acfFiles.Length == 0)
            {
                yield return new Result(null,$"Library folder {libraryFolder.FullName} does not contain any manifests");
                continue;
            }

            foreach (var acfFile in acfFiles)
            {
                yield return ParseAppManifestFile(acfFile, libraryFolder);
            }
        }
    }

    /// <inheritdoc/>
    public override Dictionary<int, SteamGame> FindAllGamesById(out string[] errors)
    {
        var (games, allErrors) = FindAllGames().SplitResults();
        errors = allErrors;

        return games.CustomToDictionary(game => game.AppId, game => game);
    }

    private (FileInfo? libraryFoldersFile, string? error) FindSteam()
    {
        try
        {
            var defaultSteamDir = GetDefaultSteamDirectory();
            var libraryFoldersFile = GetLibraryFoldersFile(defaultSteamDir);

            if (libraryFoldersFile.Exists)
            {
                return (libraryFoldersFile, null);
            }

            if (_registry is null)
            {
                return (null, $"Unable to find Steam in the default path {defaultSteamDir.FullName}");
            }

            var steamDir = FindSteamInRegistry();
            if (steamDir is null)
            {
                return (null, $"Unable to find Steam in the registry and the default path {defaultSteamDir.FullName}");
            }

            if (!steamDir.Exists)
            {
                return (null, $"Unable to find Steam in the default path {defaultSteamDir.FullName} and the path from the registry does not exist: {steamDir.FullName}");
            }

            libraryFoldersFile = GetLibraryFoldersFile(steamDir);
            if (!libraryFoldersFile.Exists)
            {
                return (null, $"Unable to find Steam in the default path {defaultSteamDir.FullName} and the path from the registry is not a valid Steam installation because {libraryFoldersFile.FullName} does not exist");
            }

            return (libraryFoldersFile, null);
        }
        catch (Exception e)
        {
            return (null, $"Exception while searching for Steam:\n{e}");
        }
    }

    private DirectoryInfo? FindSteamInRegistry()
    {
        if (_registry is null) return null;
        var currentUser = _registry.OpenBaseKey(RegistryHive.CurrentUser);

        using var regKey = currentUser.OpenSubKey(RegKey);
        if (regKey is null) return null;

        if (!regKey.TryGetString("SteamPath", out var steamPath)) return null;

        var directoryInfo = new DirectoryInfo(steamPath);
        return directoryInfo;
    }

    internal static DirectoryInfo GetDefaultSteamDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new DirectoryInfo(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Steam"
                ));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new DirectoryInfo(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Steam"));
        }
        
        throw new PlatformNotSupportedException();
    }

    internal static FileInfo GetLibraryFoldersFile(DirectoryInfo? steamDirectory = null)
    {
        steamDirectory = steamDirectory ?? GetDefaultSteamDirectory();

        var fileInfo = new FileInfo(Path.Join(
            steamDirectory.FullName,
            "steamapps",
            "libraryfolders.vdf"));

        return fileInfo;
    }

    private List<string>? ParseLibraryFoldersFile(FileInfo fileInfo)
    {
        try
        {
            using var stream = fileInfo.OpenRead();

            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var data = kv.Deserialize(stream, new KVSerializerOptions
            {
                HasEscapeSequences = true,
                EnableValveNullByteBugBehavior = true
            });

            if (data is null) return null;
            if (!data.Name.Equals("libraryfolders", StringComparison.OrdinalIgnoreCase)) return null;

            var paths = data.Children
                .Where(child => int.TryParse(child.Name, out _))
                .Select(child => child["path"])
                .Where(pathValue => pathValue is not null && pathValue.ValueType == KVValueType.String)
                .Select(pathValue => pathValue.ToString(CultureInfo.InvariantCulture))
                .Select(path => Path.Combine(path, "steamapps"))
                .ToList();

            return paths.Any() ? paths : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Result ParseAppManifestFile(FileInfo manifestFile, DirectoryInfo libraryFolder)
    {
        try
        {
            using var stream = manifestFile.OpenRead();

            var kv = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var data = kv.Deserialize(stream, new KVSerializerOptions
            {
                HasEscapeSequences = true,
                EnableValveNullByteBugBehavior = true
            });

            if (data is null)
            {
                return new Result(null, $"Unable to parse {manifestFile.FullName}");
            }

            if (!data.Name.Equals("AppState", StringComparison.OrdinalIgnoreCase))
            {
                return new Result(null, $"Manifest {manifestFile.FullName} is not a valid format!");
            }

            var appIdValue = data["appid"];
            if (appIdValue is null)
            {
                return new Result(null, $"Manifest {manifestFile.FullName} does not have the value \"appid\"");
            }

            var nameValue = data["name"];
            if (nameValue is null)
            {
                return new Result(null, $"Manifest {manifestFile.FullName} does not have the value \"name\"");
            }

            var installDirValue = data["installdir"];
            if (installDirValue is null)
            {
                return new Result(null, $"Manifest {manifestFile.FullName} does not have the value \"installdir\"");
            }

            var appId = appIdValue.ToInt32(NumberFormatInfo.InvariantInfo);
            var name = nameValue.ToString(CultureInfo.InvariantCulture);
            var installDir = installDirValue.ToString(CultureInfo.InvariantCulture);

            var gamePath = Path.Combine(
                libraryFolder.FullName,
                "common",
                installDir
            );

            var game = new SteamGame(appId, name, gamePath);
            return new Result(game, null);
        }
        catch (Exception e)
        {
            return new Result(null, $"Exception while parsing file {manifestFile.FullName}:\n{e}");
        }
    }
}
