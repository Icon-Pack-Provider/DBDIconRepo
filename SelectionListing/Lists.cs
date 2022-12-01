using CommunityToolkit.Mvvm.DependencyInjection;
using Octokit;
using SelectionListing.Internal;
using SelectionListing.Helper;
using static SelectionListing.Internal.Flags;
using static SelectionListing.Terms;
using System.Collections.ObjectModel;
using SelectionListing.Model;
using System;

namespace SelectionListing;

public static class Lists
{
    public static void Initialize(string gitToken, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new(gitToken));
            WorkingDirectory = workingDirectory;
        }
    }

    public static void Initialize(GitHubClient client, string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new(client));
            WorkingDirectory = workingDirectory;
        }
    }

    public static void Initialize(string workingDirectory)
    {
        if (!IsInitialized)
        {
            IsInitialized = true;
            OctokitService.InitializeInstance(new());
            WorkingDirectory = workingDirectory;
        }
    }

    /*
     * Pull from icon catagorize repo
     * https://github.com/Icon-Pack-Provider/IconCategorize
     * **https://github.com/Icon-Pack-Provider/IconRepoAddons
     * Then read folders content
     * Decide that if folder contains ".png" file make that folder a selection list
     * Otherwise, it's a Menu/Submenu
     * The listing class should consist of
     * > Name
     * > DisplayName
     * > Selection
     *   > .selectall > Select all
     *   > .selectnone > Select none*/
    public static async Task CheckCatagoryRepo(Action<string>? notifications = null)
    {
        //Check if directory exist
        var directory = IOHelper.GetCacheCatagorizeDirectory();
        //Check if config url exist
        var cloneConfig = IOHelper.GetCacheCatagorizeRepoURL();
        //Replace if changed
        if (!Equals(cloneConfig, CloneURL))
            CloneURL = cloneConfig;
        notifications?.Invoke($"Gathering program addons ({CloneURL})");
    reclone:

        if (!directory.Exists)
        {
            //Create new and pull
            var gitUser = await OctokitService.Instance.Client.User.Current();
            var gitToken = OctokitService.Instance.Client.Credentials.GetToken();
            notifications?.Invoke("Downloading program addons");
            await Task.Run(() =>
            {
                LibGit2Sharp.Repository.Clone(CloneURL, directory.FullName, new()
                {
                    CredentialsProvider = (url, user, cred) => GetCloneCredential(gitUser.Login, gitToken),
                    IsBare = false
                });
            });
            notifications?.Invoke("Program addons downloaded");
        }

    //Check if .git directory exist on cache directory
    //If not, delete it and go back up
        var dotGit = IOHelper.GetCacheDotGitDirectory();
        if (!dotGit.Exists)
        {
            //Nuke root directory and reclone
            directory.Delete(true);
            goto reclone;
        }

        if (dotGit.Exists)
        {
            //Check if it's been at least 3 days yet.
            var fetchHead = IOHelper.GetLastFetchGitFile();
            //Force update if it is
            if (fetchHead.LastWriteTimeUtc < (DateTime.UtcNow + TimeSpan.FromDays(3)))
            {
                using var libGitRepo = new LibGit2Sharp.Repository(directory.FullName);
                //Reset everything
                notifications?.Invoke("Updating program addons");
                await Task.Run(() =>
                {
                    var originMaster = libGitRepo.Branches["origin/main"];
                    libGitRepo.Reset(LibGit2Sharp.ResetMode.Hard, originMaster.Tip);
                });
                notifications?.Invoke("Program addons updated!");
            }
        }
    }

    public static ObservableCollection<SelectionMenuItem> GetListings()
    {
        ObservableCollection<SelectionMenuItem> listings = new();
        var directory = IOHelper.GetCacheCatagorizeDirectory();
        var dirs = directory.GetDirectories();
        foreach (var dir in dirs)
        {
            if (dir.Name == ".git")
                continue;
            SelectionMenuItem listing = new()
            {
                Name = dir.Name,
                DisplayName = IOHelper.GetDisplayFile(dir.FullName)
            };
            if (IOHelper.IsThisListingFolder(dir.FullName))
            {
                listing.Selections = new(dir.GetFiles("*.png", SearchOption.AllDirectories).Select(file => file.FullName.Replace(dir.FullName, "")));
            }
            else
            {
                listing.Childs = GetChilds(dir);
            }
            listings.Add(listing);
        }
        return listings;
    }

    private static ObservableCollection<SelectionMenuItem?> GetChilds(DirectoryInfo dir)
    {
        var folders = dir.GetDirectories();
        var childs = new ObservableCollection<SelectionMenuItem>();
        foreach (var folder in folders)
        {
            SelectionMenuItem listing = new()
            {
                Name = folder.Name,
                DisplayName = IOHelper.GetDisplayFile(folder.FullName)
            };
            if (IOHelper.IsThisListingFolder(folder.FullName))
            {
                listing.Selections = new(folder.GetFiles("*.png", SearchOption.AllDirectories).Select(file => file.FullName.Replace(folder.FullName, "")));
            }
            else
            {
                listing.Childs = GetChilds(folder);
            }
            childs.Add(listing);
        }
        return childs;
    }

    private static LibGit2Sharp.Credentials GetCloneCredential(string username, string gitToken)
    {
        return new LibGit2Sharp.UsernamePasswordCredentials()
        {
            Username = username,
            Password = gitToken
        };
    }
}
