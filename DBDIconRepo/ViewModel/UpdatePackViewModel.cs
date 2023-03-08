using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DBDIconRepo.Helper;
using DBDIconRepo.Helper.Uploadable;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Uploadable;
using DBDIconRepo.Service;
using DBDIconRepo.Strings;
using IconInfo;
using IconInfo.Icon;
using IconInfo.Information;
using IconInfo.Internal;
using IconPack;
using IconPack.Model;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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
        var selected = UserPacks[SelectedRepository];
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
                NewPotentialIcons.Add(new UploadableFile()
                {
                    DisplayName = "Icon pack banner",
                    FilePath = file.FullName,
                    Name = ".banner"
                });
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
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
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
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
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
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
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

    [RelayCommand]
    private void FinishSelection()
    {
        CurrentPage = UpdatePages.CommitMessage;
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