using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DBDIconRepo.Helper;
using DBDIconRepo.Helper.Uploadable;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Uploadable;
using DBDIconRepo.Service;
using IconInfo;
using IconInfo.Information;
using IconInfo.Internal;
using IconPack;
using IconPack.Model;
using LibGit2Sharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel;

public partial class UpdatePackViewModel : ObservableObject
{
    public OctokitService Git => Singleton<OctokitService>.Instance;
    public Setting Config => SettingManager.Instance;

    #region Pages
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOnLoadOrNoRepos))]
    [NotifyPropertyChangedFor(nameof(ShowOnLoadRepos))]
    [NotifyPropertyChangedFor(nameof(IsLoadingRepos))]
    [NotifyPropertyChangedFor(nameof(ShowOnNoRepos))]
    [NotifyPropertyChangedFor(nameof(ShowOnWaitForRepos))]
    [NotifyPropertyChangedFor(nameof(ShowOnSetNewIconsDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnInvalidIconDirectory))]
    [NotifyPropertyChangedFor(nameof(ShowOnSelectNewIcons))]
    [NotifyPropertyChangedFor(nameof(ShowOnCommitMessage))]
    private UpdatePages currentPage = UpdatePages.LoadRepos;

    public Visibility ShowOnLoadOrNoRepos => CurrentPage < UpdatePages.WaitForRepos ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowOnLoadRepos => CurrentPage == UpdatePages.LoadRepos ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnNoRepos => CurrentPage == UpdatePages.NoRepos ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnWaitForRepos => CurrentPage == UpdatePages.WaitForRepos ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnSetNewIconsDirectory => CurrentPage == UpdatePages.SetNewIconsDirectory ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnInvalidIconDirectory => CurrentPage == UpdatePages.InvalidIconDirectory ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnSelectNewIcons => CurrentPage == UpdatePages.SelectNewIcons ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnCommitMessage => CurrentPage == UpdatePages.CommitMessage ? Visibility.Visible : Visibility.Collapsed;

    public bool IsLoadingRepos => CurrentPage == UpdatePages.LoadRepos;

    public UpdatePackViewModel()
    {
        ListOwnedRepository().Await(() =>
        {
            if (UserPacks.Count == 0)
            {
                CurrentPage = UpdatePages.NoRepos;
            }
            CurrentPage = UpdatePages.WaitForRepos;
        });
    }

    [ObservableProperty]
    ObservableCollection<Pack> userPacks = new();

    private async Task ListOwnedRepository()
    {
        if (Git.UserRepositories is null)
        {
            //Get list
            await Git.GetUserRepos();
        }
        foreach (var repo in Git.UserRepositories)
        {
            var pack = await Packs.GetPack(repo);
            if (pack is null)
                continue;

            UserPacks.Add(pack);
        }
    }
    #endregion

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanOpenRepositoryOnGit))]
    private int selectedRepository = -1;

    partial void OnSelectedRepositoryChanged(int value)
    {
        if (SelectedRepository > -1)
        {
            CurrentPage = UpdatePages.SetNewIconsDirectory;
        }
        else if (SelectedRepository > -1 && CurrentPage > UpdatePages.SetNewIconsDirectory)
        {
            //TODO:Reset inputs of selections and commit messages
            CurrentPage = UpdatePages.SetNewIconsDirectory;
        }
    }

    public bool CanOpenRepositoryOnGit => SelectedRepository >= 0;

    [RelayCommand]
    public void OpenGitRepository() => URL.OpenURL(UserPacks[SelectedRepository].Repository.CloneUrl);

    [ObservableProperty]
    string selectFolderErrorMessage = string.Empty;

    [RelayCommand]
    public async Task SetNewIconDirectory()
    {
        if (!OperatingSystem.IsWindows())
            return;
        if (!OperatingSystem.IsWindowsVersionAtLeast(7, 0))
            return;
        VistaFolderBrowserDialog browse = new()
        {
            Description = "Select folder that contain icons to update",
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
            CurrentPage = UpdatePages.InvalidIconDirectory;
            return;
        }

        //Hol' up! Is this appdata folder??
        if (folder.FullName.Contains(Config.CacheAndDisplayDirectory))
        {
            SelectFolderErrorMessage = "This is a working folder that required by app, please select other folder!";
            CurrentPage = UpdatePages.InvalidIconDirectory;
            return;
        }

        WorkingDirectory = folder.FullName;

        IsCloningRepository = true;
        await CheckAndCloneRepository();
        IsCloningRepository = false;
        await DetermineIcons();
        CurrentPage = UpdatePages.SelectNewIcons;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowWhileCloningRepository))]
    bool isCloningRepository = false;

    public Visibility ShowWhileCloningRepository => IsCloningRepository ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty]
    string cloningProgressText = string.Empty;

    public async Task CheckAndCloneRepository()
    {
        Pack selected = UserPacks[SelectedRepository];
        DirectoryInfo cloned =
            new(Path.Combine(SettingManager.Instance.CacheAndDisplayDirectory,
            "Upload",
            selected.Repository.Owner,
            selected.Repository.Name));

        await selected.ClonePackTo(cloned, (prog) =>
        {
            var progressOverall = new DownloadRepoProgressReportMessage(prog);
            switch (progressOverall.CurrentState)
            {
                case DownloadState.Enumerating:
                    CloningProgressText = $"Cloning repository {Math.Round(progressOverall.EstimateProgress * 5d)}%";
                    break;
                case DownloadState.Compressing:
                    CloningProgressText = $"Cloning repository {5d + Math.Round(progressOverall.EstimateProgress * 15d)}%";
                    break;
                case DownloadState.Transfering:
                    CloningProgressText = $"Cloning repository {20d + Math.Round(progressOverall.EstimateProgress * 70d)}%";
                    break;
                case DownloadState.CheckingOut:
                    CloningProgressText = $"Cloning repository {90d + Math.Round(progressOverall.EstimateProgress * 10d)}%";
                    break;
            }
            if (progressOverall.EstimateProgress > 0.99d && progressOverall.CurrentState <= DownloadState.CheckingOut
                || progressOverall.CurrentState == DownloadState.Done)
            {
                CloningProgressText = "Repository downloaded";
            }            
        });
    }

    [RelayCommand]
    public async Task DetermineIcons()
    {
        NewPotentialIcons = new();
        DirectoryInfo dir = new(WorkingDirectory);
        var files = dir.GetFiles("*.png", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.FullName.Contains(".banner.png"))
            {
                //New banner?
                NewPotentialIcons.AddOrUpdateBanner(file.FullName);
                continue;
            }
            //NoLicense not supported, for now...
            if (file.FullName.Contains("NoLicense"))
                continue;
            if (IconTypeIdentify.FromFile(file.FullName) is not IBasic icon)
                continue;
            if (icon is UnknownIcon)
                continue;

            string mainFolder = IconTypeIdentify.GetMainFolderFromType(icon);
            //Is main folder exist? If not, add one.
            if (!NewPotentialIcons.IsMainFolderExist(mainFolder))
            {
                bool isFound = Info.Folders.TryGetValue(mainFolder, out MainFolder foundedMain);
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    NewPotentialIcons.Add(new UploadableFolder()
                    {
                        Name = mainFolder,
                        DisplayName = foundedMain.Name
                    });
                }, SettingManager.Instance.SacrificingAppResponsiveness ?
                System.Windows.Threading.DispatcherPriority.Send :
                System.Windows.Threading.DispatcherPriority.Background);
            }

            if (NewPotentialIcons.FirstOrDefault(find => find.Name == mainFolder) is not UploadableFolder mainUploadFolder)
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
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
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
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                (toWorkOn as UploadableFolder)?.SubItems.Add(new UploadableFile((toWorkOn as UploadableFolder))
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

    [ObservableProperty]
    private ObservableCollection<IUploadableItem> newPotentialIcons = new();

    [RelayCommand]
    private void GoBackToSetIconDirectory() => CurrentPage = UpdatePages.SetNewIconsDirectory;

    [ObservableProperty]
    private string workingDirectory = string.Empty;

    [RelayCommand] private void CollapseAllFolder() => NewPotentialIcons.SetExpansionState(false);
    [RelayCommand] private void ExpandAllFolder() => NewPotentialIcons.SetExpansionState(true);
    [RelayCommand] private void SelectAllItem() => NewPotentialIcons.SetSelectionState(true);
    [RelayCommand] private void UnSelectAllItem() => NewPotentialIcons.SetSelectionState(false);

    private string GetPotentiallyWorkingDirectory()
    {
        if (UserPacks is null || UserPacks.Count < 1)
            return string.Empty;
        if (SelectedRepository <= 0 || SelectedRepository >= UserPacks.Count)
            return string.Empty;
        return Path.Join(SettingManager.Instance.CacheAndDisplayDirectory,
                "Upload",
                UserPacks[SelectedRepository]?.Repository?.Owner,
                UserPacks[SelectedRepository]?.Repository?.Name);
    }

    private bool IsRepoAlreadyExistLocally()
    {
        string path = GetPotentiallyWorkingDirectory();
        return Directory.Exists(path);
    }

    [RelayCommand]
    private async void FinishSelection()
    {
        //Check for changes, then go to next page if there's actually any changes
        //Just to be safe
        if (Git.IsAnonymous)
        {
            DialogHelper.Show("Please login to GitHub to continue!", "How are you even got here?");
            return;
        }

        //Copy new selected files to git
        var gitWorkFolder = new DirectoryInfo(GetPotentiallyWorkingDirectory());

        var files = NewPotentialIcons.GetAllSelectedPaths().ToList();
        await Task.Run(() =>
        {
            foreach (var file in files)
            {
                StringBuilder journey = new(gitWorkFolder.FullName);
                if (journey[journey.Length - 1] != '\\')
                    journey.Append('\\');
                //Update banner?
                if (file.Contains(".banner"))
                {
                    journey.Append(".banner.png");
                    FileInfo destination = new(journey.ToString());
                    if (destination.Directory?.Exists == false)
                        destination.Directory.Create();
                    File.Copy(file, destination.FullName);
                    continue;
                }
                //Identify icon
                var info = IconTypeIdentify.FromFile(file);
                if (info is null)
                    continue;
                if (info is UnknownIcon)
                    continue;
                //Root folder append
                journey.Append(IconTypeIdentify.GetMainFolderFromType(info));
                journey.Append('\\');
                //Append subfolder if exist
                if (info is IFolder sub)
                {
                    journey.Append(sub.Folder);
                    journey.Append('\\');
                }
                //Filename
                journey.Append(info.File);
                journey.Append(".png");
                //Copy
                FileInfo target = new(journey.ToString());
                if (target.Directory?.Exists == false)
                    target.Directory.Create();
                File.Copy(file, target.FullName, true);
            }
        });
        //Check if all of these actually changes
        using var repository = new Repository(gitWorkFolder.FullName);
        var status = repository.RetrieveStatus();
        if (!status.IsDirty)
        {
            DialogHelper.Show("Icon is already up to date with selected folder.");
            return;
        }

        //Stage everything
        var stages = new List<string>();
        foreach (var item in status)
        {
            switch (item.State)
            {
                case FileStatus.Nonexistent:
                case FileStatus.Unaltered:
                case FileStatus.Unreadable:
                case FileStatus.Ignored:
                case FileStatus.Conflicted:
                    continue;
                default:
                    stages.Add(item.FilePath);
                    break;
            }
        }

        Commands.Stage(repository, stages);

        //
        CurrentPage = UpdatePages.CommitMessage;
    }

    [ObservableProperty]
    string commitTitle = string.Empty;

    [ObservableProperty]
    string commitDetail = string.Empty;

    [RelayCommand]
    public async Task PushUpdatePack()
    {
        Pack? selected = UserPacks[SelectedRepository];
        var gitWorkFolder = new DirectoryInfo(GetPotentiallyWorkingDirectory());
        using var repository = new Repository(gitWorkFolder.FullName);

        //Commit
        var email = await Git.GitHubClientInstance.User.Email.GetAll();
        var first = email.FirstOrDefault();
        if (first is null) return;

        var author = new Signature(Config.GitUsername, first.Email, DateTimeOffset.Now);
        await Task.Run(() =>
        {
            repository.Commit($"{CommitTitle}\r\n{CommitDetail}", author, author);
        });

        var onlineRemote = repository.Network.Remotes.FirstOrDefault();
        //Push
        repository.Network.Push(onlineRemote, "HEAD", @$"refs/heads/{selected.Repository.DefaultBranch}", new PushOptions
        {
            CredentialsProvider = (a,b,c) => OctokitService.Instance.GetLibGit2SharpCredential()
        });
        DialogHelper.Show("Update pack completed!");
        await Task.Delay(1000);
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }
}

public enum UpdatePages
{
    LoadRepos,
    NoRepos,
    WaitForRepos,
    SetNewIconsDirectory,
    InvalidIconDirectory,
    SelectNewIcons,
    CommitMessage
}