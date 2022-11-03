using CommunityToolkit.Mvvm.ComponentModel;
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
            catch { }
            try
            {
                var identify2 = IconTypeIdentify.FromFile(file.Name);
                if (identify2 is UnknownIcon)
                    continue;
                if (identify2 is not UnknownIcon)
                {
                    validFolder = true;
                    break;
                }
            }
            catch {  }
            continue;
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
        IsWaitingForPermissionToMoveResponse = false;
        if (PermissionToSort == SortFolderPermissionResponse.Allow)
        {
            IsSorting = true;
            SortWorkingFolder().Await(() =>
            {
                IsSorting = false;
            });
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowWhileSorting))]
    bool isSorting = false;

    [ObservableProperty]
    int sortMaxProgress = 1;

    [ObservableProperty]
    int sortCurrentProgress;

    public Visibility ShowWhileSorting => IsSorting == true ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    string sortingProgress = "";

    [RelayCommand]
    private async Task SortWorkingFolder()
    {
        //Move files into a new folder inside workingfolder name "NotIcons"
        //Sort folder
        await Task.Run(async () =>
        {
            var allFiles = Directory.GetFiles(WorkingDirectory, "*.*", SearchOption.AllDirectories);
            var allDirs = Directory.GetDirectories(WorkingDirectory);
            var tempIcons = new DirectoryInfo(Path.Combine(WorkingDirectory, "Temp"));
            var notIcons = new DirectoryInfo(Path.Combine(WorkingDirectory, "NotIcon"));
            if (!tempIcons.Exists)
                tempIcons.Create();
            if (!notIcons.Exists)
                notIcons.Create();
            SortMaxProgress = allFiles.Length;
            for (int i = 0; i < allFiles.Length; i++)
            {
                SortCurrentProgress = i;
                string? file = allFiles[i];
                SortingProgress = $"Sorting files {i}/{allFiles.Length}\r\n{file}";
                var info = new FileInfo(file);
                var name = Path.GetFileNameWithoutExtension(file);
                if (info.Extension != ".png")
                {
                    string newPath = file.Replace(WorkingDirectory, notIcons.FullName);
                    await Task.Run(() =>
                    {
                        SortingProgress = $"Moving {file} to\r\n{newPath}";
                        File.Move(file, newPath);
                    });
                    continue;
                }
                if (info.Name.StartsWith(".banner"))
                {
                    string newPath = file.Replace(workingDirectory, tempIcons.FullName);
                    await Task.Run(() =>
                    {
                        SortingProgress = $"Moving {file} to\r\n{newPath}";
                        File.Move(file, newPath);
                    });
                    continue;
                }
                var moved = await IconManager.OrganizeIcon(tempIcons.FullName, file);
                if (!moved)
                {
                    string newPath = file.Replace(WorkingDirectory, notIcons.FullName);
                    await Task.Run(() =>
                    {
                        SortingProgress = $"Moving {file} to\r\n{newPath}";
                        File.Move(file, newPath);
                    });
                }
            }
            //Delete empty directories
            for (int i = 0; i < allDirs.Length; i++)
            {
                if (allDirs[i].EndsWith("Temp"))
                    continue;
                else if (allDirs[i].EndsWith("NotIcon"))
                    continue;
                Directory.Delete(allDirs[i], true);
            }
        });
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
    private void ResponseAllowNotToMove()
    {
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }

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
        FileInfo banner = new(Path.Join(Path.Combine(WorkingDirectory, "Temp"), ".banner.png"));
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
    [NotifyPropertyChangedFor(nameof(IsUploadingPackRightNow))]
    [NotifyPropertyChangedFor(nameof(IsNotUploadingPackRightNow))]
    bool uploadingPack = false;

    public Visibility IsUploadingPackRightNow
        => UploadingPack ? Visibility.Visible : Visibility.Collapsed;

    public Visibility IsNotUploadingPackRightNow
        => UploadingPack ? Visibility.Collapsed : Visibility.Visible;

    [RelayCommand]
    private async Task UploadNewIconPack()
    {
        UploadingPack = true;
        UploadProgresses.Insert(0, "\r\nMoving files");
        var work = new DirectoryInfo(WorkingDirectory);
        //Move everything out of working directory
        var actualWorkFolder = new DirectoryInfo(WorkingDirectory);        
        var temporalMove = new DirectoryInfo(Path.Combine(WorkingDirectory, "Temp"));
        
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
        UploadProgresses.Insert(0, $"\r\nCreating local repository at {toGit.FullName}");
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
                UploadProgresses.Insert(0, $"\r\nMoving folder {folder.Name} to {Path.Combine(toGit.FullName, folder.Name)}");
            }
            var rootFiles = temporalMove.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in rootFiles)
            {
                file.MoveTo(Path.Join(toGit.FullName, file.Name));
                UploadProgresses.Insert(0, $"\r\nMoving file {file.Name} to {Path.Combine(toGit.FullName, file.Name)}");
                if (file.Name.Contains(".banner")
                    || file.Name.Contains("pack.json"))
                    continue;
                allStages.Add(file.FullName);
            }
        });
        //Staging icons only
        await Task.Run(() =>
        {
            UploadProgresses.Insert(0, "\r\nStaging all icons as change made");
            Commands.Stage(localRepo, allStages);
        });
        //Commit
        var email = await Git.GitHubClientInstance.User.Email.GetAll();
        var first = email.FirstOrDefault();
        if (first is null) return;
        var author = new Signature(Config.GitUsername, first.Email, DateTimeOffset.Now);
        await Task.Run(() =>
        {
            UploadProgresses.Insert(0, "\r\nCommit changes with message \"Initial icons upload\"");
            localRepo.Commit("Initial icons upload", author, author);
        });
        //
        //Remote add

        //For remote add
        UploadProgresses.Insert(0, "\r\nCreating new repository");
        //create empty remote repo
        var remoteRepo = await Git.GitHubClientInstance.Repository.Create(new(RepositoryOnGitName)
        {
            Description = RepoDescription,
            Private = false,
            Visibility = Octokit.RepositoryVisibility.Public
        });
        UploadProgresses.Insert(0, $"\r\nRepository {RepositoryOnGitName} created");
        //set topic
        await Git.GitHubClientInstance.Repository.ReplaceAllTopics(remoteRepo.Id, new Octokit.RepositoryTopics(names: getPackTopic()));

        //Remote add origin
        var onlineRemote = localRepo.Network.Remotes.Add("origin", remoteRepo.CloneUrl);
        localRepo.Branches.Update(localRepo.Head,
            localBranch => localBranch.Remote = onlineRemote.Name,
            localBranch => localBranch.UpstreamBranch = localRepo.Head.CanonicalName);

        //Online branch
        UploadProgresses.Insert(0, $"\r\nPushing (Uploading) icons to newly created repository");
        var pushOption = new PushOptions()
        {
            CredentialsProvider = (a,b,c) => GetLibGit2SharpCredential(),
            OnPushTransferProgress = (current, total, bytes) =>
            {
                UploadProgresses.Insert(0, $"\r\nPushing (Uploading) {current}/{total} {((float)current / (float)total):00.00}%");
                return true;
            }
        };
        await Task.Run(() =>
        {
            localRepo.Network.Push(remote: onlineRemote, "HEAD", @"refs/heads/master", pushOption);
        });

        if (!AllowPackMetadata)
        {
            UploadProgresses.Insert(0, "\r\nUpload icons finished, switching to mainpage");
            await Task.Delay(1000);
            Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
            return;
        }

        UploadProgresses.Insert(0, "\r\nUpload icons finished, gathering pack metadata");
        //Upload pack banner and metadata
        var packMetaData = await IconPack.Packs.GetPack(remoteRepo);
        packMetaData.Name = RepoDisplayName;
        packMetaData.Overrides = new()
        {
            Name = RepoDisplayName,
            Description = RepoDescription
        };
        string packJsonPath = Path.Join(toGit.FullName, "pack.json");
        var writer = File.CreateText(packJsonPath);
        UploadProgresses.Insert(0, "\r\nWriting pack metadata to file");
        await writer.WriteAsync(JsonSerializer.Serialize(packMetaData, new JsonSerializerOptions()
        {
            WriteIndented = true
        }));
        //Commit 
        List<string> allMetaData = new();
        allMetaData.Add(packJsonPath);
        allMetaData.Add(Path.Join(toGit.FullName, ".banner.png"));
        //Stage
        Commands.Stage(localRepo, allMetaData);
        //Commit
        localRepo.Commit("Additional pack meta data", author, author);
        //Push
        UploadProgresses.Insert(0, "\r\nPush (Upload) all metadata into repository");
        await Task.Run(() =>
        {
            localRepo.Network.Push(remote: onlineRemote, "HEAD", @"refs/heads/master", pushOption);
        });
        //Kick user to other page
        UploadProgresses.Insert(0, "\r\nFinished upload pack and all metadata, switching to mainpage");
        await Task.Delay(1000);
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }
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