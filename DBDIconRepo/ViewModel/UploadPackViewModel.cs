﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel;

public partial class UploadPackViewModel : ObservableObject
{
    public OctokitService Git => Singleton<OctokitService>.Instance;
    public Setting Config => SettingManager.Instance;

    [ObservableProperty]
    string workingDirectory = string.Empty;

    [RelayCommand]
    private void SetWorkingDirectory()
    {
        if (!OperatingSystem.IsWindows())
            return;
        if (!OperatingSystem.IsWindowsVersionAtLeast(7, 0))
            return;
        VistaFolderBrowserDialog browse = new()
        {
            Description = "Select folder that contain icons to upload",
            Multiselect = false,
            ShowNewFolderButton = false
        };
        var result = browse.ShowDialog();
        if (!result.HasValue) return;
        if (!result.Value) return;

        //Check working directory
        DirectoryInfo folder = new(browse.SelectedPath);
        var files = folder.GetFiles("*", SearchOption.AllDirectories);
        bool validFolder = false;
        foreach (var file in files)
        {
            if (file.Name == ".banner.png")
            {
                validFolder = true;
                IsBannerNowExist = true;
                PreviewOption = PackPreviewOption.Banner;
                continue;
            }
            try
            {
                var identify = IconTypeIdentify.FromPath(file.FullName);
                if (identify is UnknownIcon)
                    continue;
                if (identify is not UnknownIcon)
                {
                    validFolder = true;
                    break;
                }
            }
            catch { continue; }
        }
        if (!validFolder)
        {
            CurrentPage = UploadPages.InvalidWorkDirectory;
            return;
        }

        WorkingDirectory = browse.SelectedPath;
        CurrentPage = UploadPages.Preparing;
        PrepareWorkingFolder().Await(() =>
        {
            CurrentPage = UploadPages.FillDetail;
        });
    }

    #region Preparing page

    public async Task PrepareWorkingFolder()
    {
        //Ask to sort directory
        do
        {
            IsWaitingForPermissionToMoveResponse = PermissionToSort == SortFolderPermissionResponse.Waiting;
            TextDisplayWhileWaitingForPermissionToMoveResponse += '.';
            if (TextDisplayWhileWaitingForPermissionToMoveResponse.Length > 3)
                TextDisplayWhileWaitingForPermissionToMoveResponse = "";
            await Task.Delay(750);
        }
        while (PermissionToSort == SortFolderPermissionResponse.Waiting);
        if (PermissionToSort != SortFolderPermissionResponse.Allow)
            return;

        //Sort folder
    }

    private void SortWorkingFolder()
    {
        //TODO:Add function to sort folder
        //Move files into a new folder inside workingfolder name "NotIcons"
    }

    [ObservableProperty]
    SortFolderPermissionResponse permissionToSort = SortFolderPermissionResponse.Waiting;

    [ObservableProperty]
    bool isWaitingForPermissionToMoveResponse;

    public Visibility ShowWhileWaitingForPermissionToMoveResponse
        => IsWaitingForPermissionToMoveResponse ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    string textDisplayWhileWaitingForPermissionToMoveResponse = "";

    [RelayCommand]
    private void ResponseAllowToMove() => PermissionToSort = SortFolderPermissionResponse.Allow;
    [RelayCommand]
    private void ResponseAllowNotToMove() => PermissionToSort = SortFolderPermissionResponse.Disallow;

    #endregion

    #region Filling page
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepositoryOnGitName))]
    string repoDisplayName = string.Empty;

    public string RepositoryOnGitName
    {
        get
        {
            if (string.IsNullOrEmpty(RepoDisplayName))
                return string.Empty;
            StringBuilder bd = new();
            for (int i = 0;i < RepoDisplayName.Length; i++)
            {
                if (RepoDisplayName[i] >= 'a' && RepoDisplayName[i] <= 'z')
                    bd.Append(RepoDisplayName[i]);
                else if (RepoDisplayName[i] >= 'A' && RepoDisplayName[i] <= 'Z')
                    bd.Append(RepoDisplayName[i]);
                else
                {
                    if (bd.Length > 0 && bd[^1] == '-')
                        continue;
                    else
                        bd.Append('-');
                }
            }
            return bd.ToString();
        }
    }

    [ObservableProperty]
    string repoDescription = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewOptionExplainer))]
    PackPreviewOption previewOption = PackPreviewOption.UserDefined;
    //Gray out selection if it's banner and it's exist a file
    //Ungray the selection if the file is gone

    //Hacky way to monitor the preview option
    partial void OnPreviewOptionChanged(PackPreviewOption value)
    {
        if (value == PackPreviewOption.Banner)
        {
            //Sub to event
            Messenger.Default.Send(new MonitorForAppFocusMessage(() => CheckForBannerExistance(), () => { IsWaitingForReturn = true; }), 
                MessageToken.RequestSubToAppActivateEvent);
        }
        else
        {
            //Unsub events
            Messenger.Default.Send(new MonitorForAppFocusMessage(false),
                MessageToken.RequestSubToAppActivateEvent);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSelectPreviewOption))]
    [NotifyPropertyChangedFor(nameof(ShowIfBannerStillNotExist))]
    bool isBannerNowExist;

    public bool CanSelectPreviewOption => !IsBannerNowExist;

    public Visibility ShowIfBannerStillNotExist
        => IsBannerNowExist ? Visibility.Collapsed : Visibility.Visible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewOptionExplainer))]
    bool isWaitingForReturn;

    public void CheckForBannerExistance()
    {
        FileInfo banner = new(Path.Join(WorkingDirectory, ".banner.png"));
        IsBannerNowExist = banner.Exists;
        if (IsBannerNowExist == false)
            IsWaitingForReturn = false;
    }

    public string PreviewOptionExplainer
    {
        get
        {
            if (IsBannerNowExist)
                return "A preview option is locked to banner" +
                    "\r\nRemove .banner file to select other option";
            if (IsWaitingForReturn)
                return "Awaiting for the return with a banner";
            switch (PreviewOption)
            {
                case PackPreviewOption.UserDefined: return "Show pack preview based on user setting";
                case PackPreviewOption.Fixed: return "Show pack preview based on creator option";
                case PackPreviewOption.Banner: return "Show pack preview using banner" +
                        "\r\nBanner display only support image resolution 1280x300" +
                        "\r\nBanner has to be .png file image type" +
                        "\r\nPut the banner on selected folder and name it \".banner\"";
                default: return string.Empty;
            }
        }
    }

    [RelayCommand]
    private void OpenWorkFolder()
    {
        Process.Start("explorer", WorkingDirectory);
    }

    [ObservableProperty]
    bool allowPackMetadata;

    public Visibility CanSetBannerAsPackPreview
        => AllowPackMetadata ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    IconPack.Model.Pack generatedPackInfo = new();

    [ObservableProperty]
    string uploadProgresses = string.Empty;

    [ObservableProperty]
    bool uploadingPack = false;

    [RelayCommand]
    private async Task UploadNewIconPack()
    {
        UploadingPack = true;
        UploadProgresses += "\r\nMoving files";
        var work = new DirectoryInfo(WorkingDirectory);
        //Move everything out of working directory
        var actualWorkFolder = new DirectoryInfo(WorkingDirectory);
        var originalFolders = actualWorkFolder.GetDirectories();
        var originalFiles = actualWorkFolder.GetFiles("*.*", SearchOption.TopDirectoryOnly);

        var temporalMove = new DirectoryInfo(Path.Combine(WorkingDirectory, "Temp"));
        temporalMove.Create();
        await Task.Run(() =>
        {
            foreach (var folder in originalFolders)
            {
                folder.MoveTo(Path.Combine(temporalMove.FullName, folder.Name));
                UploadProgresses += $"\r\nMoving folder {folder.Name} out of working directory";
            }
            foreach (var file in originalFiles)
            {
                file.MoveTo(Path.Join(temporalMove.FullName, file.Name));
                UploadProgresses += $"\r\nMoving file {file.Name} out of working directory";
            }
        });
        

        var toGit = new DirectoryInfo(Path.Combine(WorkingDirectory, "Git"));
        toGit.Create();
        //Local repo
        /* git init
         * git add README.md
         * git commit -m "first commit"
         * git branch -M main
         * git remote add origin 
         * git push -u origin main */
        //Create local repo
        UploadProgresses += $"Creating local repository at {toGit.FullName}";
        Repository.Init(toGit.FullName);
        using Repository localRepo = new(toGit.FullName);
        //Move everything into initalized repo
        var allStages = new List<string>();

        await Task.Run(() =>
        {
            var rootFolders = temporalMove.GetDirectories();
            foreach (var folder in rootFolders)
            {
                folder.MoveTo(Path.Combine(toGit.FullName, folder.Name));
                allStages.AddRange(folder.GetFiles().Select(file => file.FullName));
                UploadProgresses += $"Moving folder {folder.Name} to {Path.Combine(toGit.FullName, folder.Name)}";
            }
            var rootFiles = temporalMove.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in rootFiles)
            {
                file.MoveTo(Path.Join(toGit.FullName, file.Name));
                UploadProgresses += $"Moving file {file.Name} to {Path.Combine(toGit.FullName, file.Name)}";
                if (file.Name.Contains(".banner")
                    || file.Name.Contains("pack.json"))
                    continue;
                allStages.Add(file.FullName);
            }
        });
        //Staging icons only
        await Task.Run(() =>
        {
            UploadProgresses += "Staging all icons as change made";
            Commands.Stage(localRepo, allStages);
        });
        //Commit
        var email = await Git.GitHubClientInstance.User.Email.GetAll();
        var first = email.FirstOrDefault();
        if (first is null) return;
        var author = new Signature(Config.GitUsername, first.Email, DateTimeOffset.Now);
        await Task.Run(() =>
        {
            UploadProgresses += "Commit changes with message \"Initial icons upload\"";
            localRepo.Commit("Initial icons upload", author, author);
        });
        //
        //Remote add

        //For remote add
        UploadProgresses += "Creating new repository";
        //create empty remote repo
        var remoteRepo = await Git.GitHubClientInstance.Repository.Create(new(RepositoryOnGitName)
        {
            Description = RepoDescription,
            Private = false,
            Visibility = Octokit.RepositoryVisibility.Public
        });
        UploadProgresses += $"Repository {RepositoryOnGitName} created";
        //set topic
        await Git.GitHubClientInstance.Repository.ReplaceAllTopics(remoteRepo.Id, new Octokit.RepositoryTopics(names: getPackTopic()));

        //Remote add origin
        var onlineRemote = localRepo.Network.Remotes.Add("origin", remoteRepo.CloneUrl);
        localRepo.Branches.Update(localRepo.Head,
            localBranch => localBranch.Remote = onlineRemote.Name,
            localBranch => localBranch.UpstreamBranch = localRepo.Head.CanonicalName);

        //Online branch
        UploadProgresses += $"Pushing (Uploading) icons to newly created repository";
        var pushOption = new PushOptions()
        {
            CredentialsProvider = (a,b,c) => GetLibGit2SharpCredential(),
            OnPushTransferProgress = (current, total, bytes) =>
            {
                UploadProgresses += $"Pushing (Uploading) {current}/{total} {((float)current / (float)total):00.00}%";
                return true;
            }
        };
        await Task.Run(() =>
        {
            localRepo.Network.Push(remote: onlineRemote, "HEAD", @"refs/heads/master", pushOption);
        });

        if (!AllowPackMetadata)
        {
            UploadProgresses += "Upload icons finished, switching to mainpage";
            await Task.Delay(1000);
            Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
            return;
        }

        UploadProgresses += "Upload icons finished, gathering pack metadata";
        //Upload pack banner and metadata
        var packMetaData = await IconPack.Packs.GetPack(remoteRepo);
        packMetaData.Name = RepoDisplayName;
        string packJsonPath = Path.Join(toGit.FullName, "pack.json");
        var writer = File.CreateText(packJsonPath);
        UploadProgresses += "Writing pack metadata to file";
        await writer.WriteAsync(JsonSerializer.Serialize(packMetaData));
        //Commit 
        List<string> allMetaData = new();
        allMetaData.Add(packJsonPath);
        allMetaData.Add(Path.Join(toGit.FullName, ".banner.png"));
        //Stage
        Commands.Stage(localRepo, allMetaData);
        //Commit
        localRepo.Commit("Additional pack meta data", author, author);
        //Push
        UploadProgresses += "Push (Upload) all metadata into repository";
        await Task.Run(() =>
        {
            localRepo.Network.Push(remote: onlineRemote, "HEAD", @"refs/heads/master", pushOption);
        });
        //Kick user to other page
        UploadProgresses += "Finished upload pack and all metadata, switching to mainpage";
        await Task.Delay(1000);
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }

    /*
public void PushChanges() {
    try {
        var remote = repo.Network.Remotes["origin"];
        var options = new PushOptions();
        var credentials = new UsernamePasswordCredentials { Username = username, Password = password };
        options.Credentials = credentials;
        var pushRefSpec = @"refs/heads/master";
        repo.Network.Push(remote, pushRefSpec, options, new Signature(username, email, DateTimeOffset.Now),
            "pushed changes");
    }
    catch (Exception e) {
        Console.WriteLine("Exception:RepoActions:PushChanges " + e.Message);
    }
}*/
    #endregion
    private LibGit2Sharp.UsernamePasswordCredentials GetLibGit2SharpCredential()
    {
        return new LibGit2Sharp.UsernamePasswordCredentials()
        {
            Username = Config.GitUsername,
            Password = new SecureSettingService().GetSecurePassword()
        };
    }

    private IEnumerable<string> getPackTopic()
    {
        yield return "dbd-icon-pack";
    }

    #region Pages
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnSetWorkDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnInvalidWorkDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnSetWorkDirectoryAndInvalidWorkDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnPreparingPack))]
    [NotifyPropertyChangedFor(nameof(ShowOnFillDetail))]
    [NotifyPropertyChangedFor(nameof(ShowOnUploading))]
    UploadPages currentPage = UploadPages.SetWorkDirectory;

    public Visibility ShowOnSetWorkDirectory 
        => CurrentPage == UploadPages.SetWorkDirectory 
        ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnInvalidWorkDirectory
        => CurrentPage == UploadPages.InvalidWorkDirectory
        ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnPreparingPack
        => CurrentPage == UploadPages.Preparing
        ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnFillDetail
        => CurrentPage == UploadPages.FillDetail
        ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnUploading
        => CurrentPage == UploadPages.Uploading
        ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnSetWorkDirectoryAndInvalidWorkDirectory
        => CurrentPage <= UploadPages.InvalidWorkDirectory
        ? Visibility.Visible : Visibility.Collapsed;
    #endregion
}

public enum UploadPages
{
    SetWorkDirectory,
    InvalidWorkDirectory,
    Preparing,
    FillDetail,
    Uploading
}

public enum SortFolderPermissionResponse
{
    Waiting,
    Allow,
    Disallow
}

public enum PackPreviewOption
{
    UserDefined,
    Fixed,
    Banner
}