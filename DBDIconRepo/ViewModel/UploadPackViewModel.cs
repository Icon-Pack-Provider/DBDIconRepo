﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Helper.Uploadable;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Uploadable;
using DBDIconRepo.Service;
using IconInfo;
using IconInfo.Information;
using IconInfo.Internal;
using LibGit2Sharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public UploadPackViewModel()
    {
        InitializeFillInfoPage();
    }

    public OctokitService Git => Singleton<OctokitService>.Instance;
    public Setting Config => SettingManager.Instance;

    [ObservableProperty]
    InProgressPack? workingPack = new();

    [ObservableProperty]
    string workingDirectory = string.Empty;

    //This will only now handle upload new pack, not existing one.
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
        //Is this empty folder?
        if (folder.GetFiles("*.png", SearchOption.AllDirectories).Length < 1)
        {
            SelectFolderErrorMessage = "No icon found!\r\nTry other folder?";
            CurrentPage = UploadPages.InvalidWorkDirectory;
            return;
        }
        //Check if there's a trace of .git
        var findDotGit = folder.GetDirectories(".git");
        if (findDotGit.Length > 0 && findDotGit.FirstOrDefault() is DirectoryInfo dotGitFolder && dotGitFolder.Exists) 
        {
            //Throw error
            SelectFolderErrorMessage = "This folder is already exist as other icon packs!\r\nplease use update pack instead.";
            CurrentPage = UploadPages.InvalidWorkDirectory;
            return;
        }

        WorkingDirectory = folder.FullName;
        CurrentPage = UploadPages.Preparing;

        //Let uploader select which icons to upload:
        DetermineUploadableItems().Await(() =>
        {
            CurrentPage = UploadPages.SelectIcons;
        });
    }

    [ObservableProperty]
    private string selectFolderErrorMessage;

    #region Select icons
    [ObservableProperty]
    private ObservableCollection<IUploadableItem> uploadables = new();

    [ObservableProperty] private Visibility thisWillHavePerks = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHavePortraits = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHavePowers = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHaveItems = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHaveAddons = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHaveStatus = Visibility.Collapsed;
    [ObservableProperty] private Visibility thisWillHaveOfferings = Visibility.Collapsed;

    partial void OnCurrentPageChanged(UploadPages value)
    {
        if (value != UploadPages.FillDetail)
            return;
        //Update list of available icons on pack
        ThisWillHavePerks = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "Perks" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHavePortraits = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "CharPortraits" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHavePowers = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "Powers" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHaveItems = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "items" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHaveAddons = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "ItemAddons" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHaveStatus = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "StatusEffects" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
        ThisWillHaveOfferings = Uploadables.Any(folder => folder is UploadableFolder && folder.Name == "Favors" && folder.IsSelected != false) ? Visibility.Visible : Visibility.Collapsed;
    }

    [RelayCommand]
    public async Task DetermineUploadableItems()
    {
        Uploadables = new();
        DirectoryInfo dir = new(WorkingDirectory);
        var files = dir.GetFiles("*.png", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.FullName.Contains(".banner.png"))
            {
                Uploadables.AddOrUpdateBanner(file.FullName);
                continue;
            }
            //NoLicense not supported
            if (file.FullName.Contains("NoLicense"))
                continue;
            if (IconTypeIdentify.FromFile(file.FullName) is not IBasic icon)
                continue;
            if (icon is UnknownIcon)
                continue;
            string mainFolder = IconTypeIdentify.GetMainFolderFromType(icon);
            if (!Uploadables.IsMainFolderExist(mainFolder))
            {
                bool isFound = Info.Folders.TryGetValue(mainFolder, out MainFolder foundedMain);
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    Uploadables.Add(new UploadableFolder()
                    {
                        Name = mainFolder,
                        DisplayName = foundedMain.Name
                    });
                }, SettingManager.Instance.SacrificingAppResponsiveness ?
                System.Windows.Threading.DispatcherPriority.Send :
                System.Windows.Threading.DispatcherPriority.Background);
            }
            if (Uploadables.FirstOrDefault(find => find.Name == mainFolder) is not UploadableFolder mainUploadFolder)
                continue;
            //Subfolder
            IUploadableItem? toWorkOn = mainUploadFolder; //This will sure change to subfolder if its exist
            IFolder? isFolder = icon as IFolder;
            if (isFolder is not null && !string.IsNullOrEmpty(isFolder.Folder))
            {
                if (!mainUploadFolder.SubItems.Any(sub => sub.Name == isFolder.Folder))
                {
                    //No subfolder, add:
                    var subFolderInfo = Info.SubFolders[isFolder.Folder];
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        mainUploadFolder.SubItems.Add(new UploadableFolder(mainUploadFolder)
                        {
                            Name = subFolderInfo.Folder,
                            DisplayName = subFolderInfo.Name,
                            SubItems = new()
                        });
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);

                }
                toWorkOn = mainUploadFolder.SubItems.FirstOrDefault(sub => sub.Name == isFolder.Folder);
            }
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                (toWorkOn as UploadableFolder).SubItems.Add(new UploadableFile((toWorkOn as UploadableFolder))
                {
                    Name = icon.File,
                    DisplayName = icon.Name,
                    FilePath = file.FullName
                });
            }, SettingManager.Instance.SacrificingAppResponsiveness ?
            System.Windows.Threading.DispatcherPriority.Send :
            System.Windows.Threading.DispatcherPriority.Background);
            continue;
        }
    }

    [RelayCommand] private void CollapseAllFolder() => Uploadables.SetExpansionState(false);
    [RelayCommand] private void ExpandAllFolder() => Uploadables.SetExpansionState(true);
    [RelayCommand] private void SelectAllItem() => Uploadables.SetSelectionState(true);
    [RelayCommand] private void UnSelectAllItem() => Uploadables.SetSelectionState(false);

    [RelayCommand]
    private void FinishSelection()
    {
        CurrentPage = UploadPages.FillDetail;
        //Update icons preview
        FillPreviewSourcesWithUserSetting();
    }

    [RelayCommand] private void CancelSelectionRevert() => CurrentPage = UploadPages.SetWorkDirectory;
    #endregion

    #region Filling page
    private void InitializeFillInfoPage()
    {
        if (FixedIcons is null)
            FixedIcons = new();
        FixedIcons.CollectionChanged += UpdateProperties;
    }

    private void UpdateProperties(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(ShowOnFixedIconsHaveRemovableIcon));
        OnPropertyChanged(nameof(IsFixedIconFull));
        OnPropertyChanged(nameof(AllowAddingFixedIcon));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepositoryOnGitName))]
    [NotifyCanExecuteChangedFor(nameof(UploadNewIconPackCommand))]
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
                else if (RepoDisplayName[i] >= '0' && RepoDisplayName[i] <= '9')
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
    [NotifyPropertyChangedFor(nameof(ShowBannerLocatorButton))]
    [NotifyPropertyChangedFor(nameof(BannerLocatorContext))]
    [NotifyPropertyChangedFor(nameof(ShowOnFixedIconPreviewOption))]
    [NotifyCanExecuteChangedFor(nameof(UploadNewIconPackCommand))]
    PackPreviewOption previewOption = PackPreviewOption.UserDefined;
    //Gray out selection if it's banner and it's exist a file
    //Ungray the selection if the file is gone

    //Hacky way to monitor the preview option
    partial void OnPreviewOptionChanged(PackPreviewOption value)
    {
        //Update Preview sources
        switch (value)
        {
            case PackPreviewOption.UserDefined:
                FillPreviewSourcesWithUserSetting();
                break;
            case PackPreviewOption.Fixed:
                FillPreviewSourcesWithFixedIcons();
                break;
            case PackPreviewOption.Banner:
                FillPreviewSourcesWithSingleBanner();
                break;
        }
    }

    public Visibility ShowOnFixedIconPreviewOption => PreviewOption == PackPreviewOption.Fixed ? Visibility.Visible : Visibility.Collapsed;

    private void FillPreviewSourcesWithUserSetting()
    {
        PreviewSources = new();
        var userPrefs = SettingManager.Instance.PerkPreviewSelection;
        foreach (var item in userPrefs)
        {
            if (Uploadables.SearchFile(item.File) is UploadableFile result)
            {
                PreviewSources.Add(new LocalSourceDisplay(result.FilePath));
            }
        }
        if (PreviewSources.Count < 4)
        {
            while (PreviewSources.Count != 4)
            {
                var rand = Uploadables.GetRandomIcon().FilePath;
                if (!PreviewSources.Any(i => i.URL == rand))
                    PreviewSources.Add(new LocalSourceDisplay(rand));
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnFixedIconsHaveRemovableIcon))]
    [NotifyPropertyChangedFor(nameof(IsFixedIconFull))]
    [NotifyPropertyChangedFor(nameof(AllowAddingFixedIcon))]
    [NotifyCanExecuteChangedFor(nameof(UploadNewIconPackCommand))]
    private ObservableCollection<IUploadableItem> fixedIcons = new();

    public bool IsFixedIconFull => FixedIcons.Count >= 4;
    public bool AllowAddingFixedIcon => !IsFixedIconFull;
    public Visibility ShowOnFixedIconsHaveRemovableIcon => FixedIcons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    private void FillPreviewSourcesWithFixedIcons()
    {
        PreviewSources = new();
        foreach (var icon in FixedIcons)
        {
            PreviewSources.Add(new LocalSourceDisplay((icon as UploadableFile).FilePath));
        }
        UploadNewIconPackCommand.NotifyCanExecuteChanged();
    }

    private void FillPreviewSourcesWithSingleBanner()
    {
        PreviewSources = new();
        if (Uploadables.IsBannerExist())
        {
            var banner = Uploadables.GetBanner() as UploadableFile;
            PreviewSources.Add(new LocalSourceDisplay(banner.FilePath));
            UploadNewIconPackCommand.NotifyCanExecuteChanged();
        }
        else //Fill with placeholder
        {            
            PreviewSources.Add(new PlaceholderSourceDisplay());
        }
    }

    [RelayCommand]
    private void AddRandomFixedIcons()
    {
    roll_again:
        var rand = Uploadables.GetRandomIcon();
        if (FixedIcons.Any(icon => icon.Name == rand.Name))
            goto roll_again;
        PreviewSources.Add(new LocalSourceDisplay(rand.FilePath));
        FixedIcons.Add(rand);
        FillPreviewSourcesWithFixedIcons();
    }

    [RelayCommand]
    private void AddThisIconAsFixedIcons(IUploadableItem selected)
    {
        if (selected is not UploadableFile)
            return;
        FixedIcons.Add(selected);
        FillPreviewSourcesWithFixedIcons();
    }

    [RelayCommand]
    private void RemoveThisIconFromFixedIcons(IUploadableItem selected)
    {
        if (selected is not UploadableFile)
            return;
        if (!FixedIcons.Any(icon => Equals(icon, selected)))
            return;
        FixedIcons.Remove(selected);
        FillPreviewSourcesWithFixedIcons();
    }

    [RelayCommand]
    private void RemoveAllFixedIcons()
    {
        FixedIcons.Clear();
        FillPreviewSourcesWithFixedIcons();
    }

    public Visibility ShowBannerLocatorButton => PreviewOption == PackPreviewOption.Banner ? Visibility.Visible : Visibility.Collapsed;

    public string BannerLocatorContext
    {
        get
        {
            if (Uploadables.FirstOrDefault(i => i.DisplayName == "Icon pack banner") is IUploadableItem item)
            {
                return "Change banner";
            }
            return "Set banner";
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSelectPreviewOption))]
    [NotifyPropertyChangedFor(nameof(ShowIfBannerStillNotExist))]
    [NotifyCanExecuteChangedFor(nameof(UploadNewIconPackCommand))]
    bool isBannerNowExist;

    public bool CanSelectPreviewOption => !IsBannerNowExist;

    public Visibility ShowIfBannerStillNotExist
        => IsBannerNowExist ? Visibility.Collapsed : Visibility.Visible;

    public void CheckForBannerExistance()
    {
        var item = Uploadables.FirstOrDefault(i => i.DisplayName == "Icon pack banner");
        if (item is not UploadableFile)
        {
            IsBannerNowExist = false;
            return;
        }
        FileInfo banner = new((item as UploadableFile).FilePath);
        IsBannerNowExist = banner.Exists;
    }

    public string PreviewOptionExplainer
    {
        get
        {
            if (IsBannerNowExist)
                return "A preview option is now locked to banner";
            switch (PreviewOption)
            {
                case PackPreviewOption.UserDefined: return "Show pack preview based on user setting";
                case PackPreviewOption.Fixed: return "Show pack preview based on creator option";
                case PackPreviewOption.Banner: return "Show pack preview using banner" +
                        "\r\nBanner display only support image resolution 1280x300" +
                        "\r\nBanner has to be .png file image type";
                default: return string.Empty;
            }
        }
    }

    [ObservableProperty]
    ObservableCollection<IDisplayItem>? previewSources = new();

    [RelayCommand]
    private void GrabPathForPackBanner()
    {
        if (!OperatingSystem.IsWindows())
            return;
        if (!OperatingSystem.IsWindowsVersionAtLeast(7, 0))
            return;
        VistaOpenFileDialog browse = new()
        {
            Filter = "PNG Image|*.png",
            FilterIndex = 0,
            Multiselect = false,
            ShowReadOnly = true,
            CheckFileExists = true,
            CheckPathExists = true,
            Title = "Select banner image to upload"
        };
        browse.FileOk += (sender, e) =>
        {
            if (e.Cancel) return;
            if (Uploadables.FirstOrDefault(file => file.DisplayName == "Icon pack banner") is IUploadableItem item)
            {
                (item as UploadableFile).FilePath = (sender as VistaOpenFileDialog).FileName;
            }
            else
            {
                Uploadables.Add(new UploadableFile()
                {
                    DisplayName = "Icon pack banner",
                    FilePath = (sender as VistaOpenFileDialog).FileName,
                    IsSelected = true
                });

            }            
            FillPreviewSourcesWithSingleBanner();
            OnPropertyChanged(nameof(BannerLocatorContext));
            OnPropertyChanged(nameof(CanUploadPack));
        };
        var result = browse.ShowDialog();
        if (!result.HasValue) return;
        if (!result.Value) return;
    }

    [RelayCommand]
    private void RemoveBanner()
    {
        Uploadables.Remove(Uploadables.First(i => i.DisplayName == "Icon pack banner"));
        FillPreviewSourcesWithSingleBanner();
        UploadNewIconPackCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void OpenWorkFolder()
    {
        Process.Start("explorer", WorkingDirectory);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSetBannerAsPackPreview))]
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

    private bool CanUploadPack()
    {
        if (string.IsNullOrEmpty(RepoDisplayName))
            return false;
        if (PreviewOption == PackPreviewOption.Fixed && FixedIcons.Count != 4)
            return false;
        if (PreviewOption == PackPreviewOption.Banner && !IsBannerNowExist)
            return false;
        return true;
    }

    private async Task<bool> IsRepoAlreadyExist()
    {
        try
        {
            var existanceCheck = await OctokitService.Instance.GitHubClientInstance.Repository.Get(Config.GitUsername, RepositoryOnGitName);
        }
        catch
        {
            return false;
        }
        return true;
    }

    private string GetPotentiallyWorkingDirectory() 
        => Path.Join(SettingManager.Instance.CacheAndDisplayDirectory,
            "Upload",
            SettingManager.Instance.GitUsername,
            RepositoryOnGitName);

    private bool IsRepoAlreadyExistLocally()
    {
        string path = GetPotentiallyWorkingDirectory();
        return Directory.Exists(path);
    }

    [RelayCommand(CanExecute = nameof(CanUploadPack))]
    private async Task UploadNewIconPack()
    {
        //Check if there's already cached project?
        bool isExistLocally = IsRepoAlreadyExistLocally();
        if (isExistLocally)
        {
            DialogHelper.Show("Please change the repository name or switch to update mode", "This repository already exist!");
            return;
        }
        //Check if it's already on git?
        bool isExistOnline = await IsRepoAlreadyExist();
        if (isExistOnline)
        {
            DialogHelper.Show("Please change the repository name or switch to update mode", "This repository already exist!");
            return;
        }
        //Just to be safe
        if (Git.IsAnonymous)
        {
            DialogHelper.Show("Please login to GitHub to continue!", "How are you even got here?");
            return;
        }

        UploadingPack = true;
        //Slightly changes: Copy everything instead of make changes to user selected folder
        var gitWorkFolder = new DirectoryInfo(GetPotentiallyWorkingDirectory());
        if (!gitWorkFolder.Exists)
            gitWorkFolder.Create();
        //Local repo
        /* git init
         * git add README.md
         * git commit -m "first commit"
         * git branch -M main
         * git remote add origin 
         * git push -u origin main */
        //Create local repo
        UploadProgresses.Insert(0, $"\r\nCreating local repository at {gitWorkFolder.FullName}");
        Repository.Init(gitWorkFolder.FullName);
        using Repository localRepo = new(gitWorkFolder.FullName);
        //Copy everything into initalized repo
        var iconStages = new List<string>();
        var bannerStages = new List<string>();

        await Task.Run(() =>
        {
            var toGit = Uploadables.GetAllSelectedPaths().ToList();
            foreach (var file in toGit)
            {
                StringBuilder destination = new(gitWorkFolder.FullName);
                if (destination[destination.Length - 1] != '\\')
                    destination.Append('\\');
                //Is it a banner?
                if (file.Contains(".banner"))
                {
                    //Is it even selected as banner?
                    if (PreviewOption != PackPreviewOption.Banner)
                        continue;
                    destination.Append(".banner.png");
                    FileInfo t2 = new(destination.ToString());
                    if (!t2.Directory.Exists)
                        t2.Directory.Create();
                    File.Copy(file, t2.FullName, true);
                    //Stage
                    bannerStages.Add(destination.ToString());
                    continue;
                }
                //Identify icon info
                var info = IconTypeIdentify.FromFile(file);
                //Then root 
                destination.Append(IconTypeIdentify.GetMainFolderFromType(info));
                destination.Append('\\');
                //Then subfolder, if exist
                if (info is IFolder sub)
                {
                    destination.Append(sub.Folder);
                    destination.Append('\\');
                }
                //Filename
                destination.Append(info.File);
                destination.Append(".png");
                //Copy
                //Confirm destination
                FileInfo target = new(destination.ToString());
                if (!target.Directory.Exists)
                    target.Directory.Create();
                File.Copy(file, target.FullName, true);
                //Then stage
                iconStages.Add(destination.ToString());
            }
        });
        //Staging icons only
        await Task.Run(() =>
        {
            UploadProgresses.Insert(0, "\r\nStaging all icons as change made");
            Commands.Stage(localRepo, iconStages);
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
        //Work on banner separately:
        if (PreviewOption == PackPreviewOption.Banner)
        {
            //Stage
            await Task.Run(() =>
            {
                UploadProgresses.Insert(0, "\r\nStaging banner into repo");
                Commands.Stage(localRepo, bannerStages);
            });
            //Commit
            await Task.Run(() =>
            {
                UploadProgresses.Insert(0, "\r\nCommit changes with message \"Banner upload\"");
                localRepo.Commit("Banner upload", author, author);
            });
        }
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
            CredentialsProvider = (a, b, c) => OctokitService.Instance.GetLibGit2SharpCredential(),
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
        var packMetaData = await IconPack.Packs.CreateOne(remoteRepo);
        packMetaData.Name = RepoDisplayName;
        packMetaData.Overrides = new()
        {
            Name = RepoDisplayName,
            Description = RepoDescription,
            DisplayFiles = new(GetIconOverrideOption()),
        };
        //LibGit2Sharp limitation;unable to init with default branch 💀
        packMetaData.Repository.DefaultBranch = "master";
        string packJsonPath = Path.Join(gitWorkFolder.FullName, "pack.json");
        var json = JsonSerializer.Serialize(packMetaData, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(packJsonPath, json);
        UploadProgresses.Insert(0, "\r\nWriting pack metadata to file");
        
        //Stage
        Commands.Stage(localRepo, new string[] { packJsonPath });
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
        DialogHelper.Show("Pack uploaded!");
        await Task.Delay(1000);
        IconPack.Packs.ResetAPICache(true, false);
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }

    private string[] GetIconOverrideOption()
    {        
        switch (PreviewOption)
        {
            case PackPreviewOption.Fixed:
                return FixedIcons.Select(i => GetFullPathName(i)).ToArray();
            default:
                return Array.Empty<string>();
        }
    }

    private string GetFullPathName(IUploadableItem i)
    {
        if (i is not UploadableFile file)
            return string.Empty;
        var info = IconTypeIdentify.FromFile(file.Name);
        if (info is UnknownIcon)
            return string.Empty;
        string main = IconTypeIdentify.GetMainFolderFromType(info);
        string sub = string.Empty;
        if (info is IFolder folder)
            sub = folder.Folder;
        return $"{main}/{sub}{(!string.IsNullOrEmpty(sub) ? "/" : "")}{info.File}.png";
    }
    #endregion

    private IEnumerable<string> getPackTopic()
    {
        yield return "dbd-icon-pack";
    }

    #region Pages
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnSetWorkDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnInvalidWorkDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnSelectIcons))]
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

    public Visibility ShowOnSelectIcons
        => CurrentPage == UploadPages.SelectIcons
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
    SelectIcons,
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