using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using GameFinder.Common;
using IconInfo;
using IconInfo.Information;
using IconInfo.Internal;
using LibGit2Sharp;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        if (folder.GetFiles("*.png", SearchOption.AllDirectories).Length < 1) return;
        //Check if there's a trace of .git
        var findDotGit = folder.GetDirectories(".git");
        if (findDotGit.Length > 0 && findDotGit.FirstOrDefault() is DirectoryInfo dotGitFolder) 
        {
            //Throw error
            DialogHelper.Show("This folder is already exist as other icon packs," +
                "\r\nplease use update pack instead");
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

    private IUploadableItem? Find(string name, ObservableCollection<IUploadableItem> subitems = null)
    {
        if (subitems is null)
            subitems = Uploadables;
        foreach (var item in subitems)
        {
            if (item is UploadableFolder sub)
            {
                var res = Find(name, sub.SubItems);
                if (res is not null)
                    return res;
                continue;
            }
            if (item.IsSelected == false)
                continue;
            if (item.Name == name)
                return item;
        }
        return null;
    }

    private UploadableFile RandomIcon(ObservableCollection<IUploadableItem> subitems = null)
    {
        if (subitems is null)
            subitems = Uploadables;
reroll:
        int index = Random.Shared.Next(0, subitems.Count);
        if (subitems[index] is UploadableFolder folder)
        {
            if (subitems[index].IsSelected.HasValue && subitems[index].IsSelected == false)
                goto reroll;
            return RandomIcon(folder.SubItems);
        }
        if (subitems[index].IsSelected.HasValue && subitems[index].IsSelected == false)
            goto reroll;
        else if (subitems[index] is UploadableFile file)
            return file;
        return new();
    }

    private bool IsMainFolderExist(string folderName)
    {
        foreach (var item in Uploadables)
        {
            if (item is UploadableFolder folder && folder.Name == folderName)
                return true;
        }
        return false;
    }

    [RelayCommand]
    public async Task DetermineUploadableItems()
    {
        Uploadables = new();
        DirectoryInfo dir = new(WorkingDirectory);
        var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (file.FullName.Contains(".banner.png"))
            { 
                continue; 
            }
            if (file.Extension != ".png")
                continue;
            if (IconTypeIdentify.FromFile(file.FullName) is IBasic icon)
            {
                if (icon is UnknownIcon)
                    continue;
                string mainFolder = IconTypeIdentify.GetMainFolderFromType(icon);
                if (!IsMainFolderExist(mainFolder))
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
                if (Uploadables.FirstOrDefault(find => find.Name == mainFolder) is UploadableFolder mainUploadFolder)
                {
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
                }
            }
            continue;
        }
    }

    private void SetCollapseState(bool state, ObservableCollection<IUploadableItem>? sub = null)
    {
        if (sub is null)
            sub = Uploadables;
        foreach (var item in sub)
        {
            if (item is UploadableFile)
                continue;
            if (item is UploadableFolder folder && folder.SubItems is not null && folder.SubItems.Count > 0)
                SetCollapseState(state, folder.SubItems);
            item.IsExpand = state;
        }
    }

    [RelayCommand] private void CollapseAllFolder() => SetCollapseState(false);
    [RelayCommand] private void ExpandAllFolder() => SetCollapseState(true);

    private void SetSelectionState(bool state, ObservableCollection<IUploadableItem>? sub = null)
    {
        if (sub is null)
            sub = Uploadables;
        foreach (var item in sub)
        {
            if (item is UploadableFolder folder && folder.SubItems is not null && folder.SubItems.Count > 0)
                SetSelectionState(state, folder.SubItems);
            item.IsSelected = state;
        }
    }
    [RelayCommand] private void SelectAllItem() => SetSelectionState(true);
    [RelayCommand] private void UnSelectAllItem() => SetSelectionState(false);

    [RelayCommand]
    private void FinishSelection()
    {
        CurrentPage = UploadPages.FillDetail;
        //Update icons preview
        FillPreviewSourcesWithUserSetting();
    }

    [RelayCommand] private void CancelSelectionRevert() => CurrentPage = UploadPages.SetWorkDirectory;
    #endregion

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
                    continue; //TODO:Move to update mode instead of upload mode?
                else if (allDirs[i].EndsWith("NotIcon"))
                    continue;
                else if (allDirs[i].EndsWith("Git"))
                    continue; //TODO:Move to update mode instead of upload mode?
                DirectoryInfo dir = new(allDirs[i]);
                if (dir.EnumerateFiles().Count() < 1)
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
        //TODO: Instead of cancel everything, copy everything that is icons to AppFolder (IconRepository) on app data and upload from there
        Messenger.Default.Send(new SwitchToOtherPageMessage("home"), MessageToken.RequestMainPageChange);
    }

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
        //Update banner text explainer event
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

    public Visibility ShowOnFixedIconPreviewOption => PreviewOption == PackPreviewOption.Fixed ? Visibility.Visible : Visibility.Collapsed;

    private void FillPreviewSourcesWithUserSetting()
    {
        PreviewSources = new();
        var userPrefs = SettingManager.Instance.PerkPreviewSelection;
        foreach (var item in userPrefs)
        {
            if (Find(item.File) is UploadableFile result)
            {
                PreviewSources.Add(new LocalSourceDisplay(result.FilePath));
            }
        }
        if (PreviewSources.Count < 4)
        {
            while (PreviewSources.Count != 4)
            {
                var rand = RandomIcon().FilePath;
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
        if (Uploadables.FirstOrDefault(icon => icon.DisplayName == "Icon pack banner") is UploadableFile banner)
        {
            PreviewSources.Add(new LocalSourceDisplay(banner.FilePath));
            UploadNewIconPackCommand.NotifyCanExecuteChanged();
        }
        else
        {
            //Fill with placeholder
            PreviewSources.Add(new PlaceholderSourceDisplay());
        }
    }

    [RelayCommand]
    private void AddRandomFixedIcons()
    {
    roll_again:
        var rand = RandomIcon();
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewOptionExplainer))]
    bool isWaitingForReturn;

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
        if (IsBannerNowExist == false)
            IsWaitingForReturn = false;
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

    private bool CanUploadPack
    {
        get
        {
            if (string.IsNullOrEmpty(RepoDisplayName))
                return false;
            if (PreviewOption == PackPreviewOption.Fixed && FixedIcons.Count != 4)
                return false;
            CheckForBannerExistance();
            if (PreviewOption == PackPreviewOption.Banner && !IsBannerNowExist)
                return false;
            return true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUploadPack))]
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

public interface IUploadableItem
{
    /// <summary>
    /// Use as folder path eg. Perks, CharPortraits
    /// </summary>
    string Name { get; set; }
    /// <summary>
    /// Explain what it was eg.Perks icon, Portrait icons etc.
    /// </summary>
    string DisplayName { get; set; }
    bool? IsSelected { get; set; }
    bool IsExpand { get; set; }
    UploadableFolder? Parent { get; }
}

public partial class UploadableFolder : ObservableObject, IUploadableItem
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string displayName = "";

    [ObservableProperty]
    private bool? isSelected = true;

    [ObservableProperty]
    private bool isExpand = true;

    public string SubFolderDisplay
        => $"{(Parent is not null ? "\\" : "")}{(Parent is not null ? Parent.Name : "")}";

    partial void OnIsSelectedChanged(bool? value)
    {
        if (Parent is not null)
            Parent.NotifyChildSelectionChanged();
        //Update child to match
        if (value is null)
            return;
        foreach (var child in SubItems)
        {
            child.IsSelected = value;
        }
    }

    [ObservableProperty]
    private ObservableCollection<IUploadableItem> subItems = new();

    UploadableFolder? parent = null;
    public UploadableFolder? Parent
    {
        get => parent;
        private set => parent = value;
    }

    public UploadableFolder(UploadableFolder? root = null)
    {
        this.parent = root;
    }

    internal void NotifyChildSelectionChanged()
    {
        var bools = SubItems.Select(i => i.IsSelected).Distinct().ToList();
        if (bools.Count <= 1)
            IsSelected = bools[0];
        else
            IsSelected = null;
    }
}

public partial class UploadableFile : ObservableObject, IUploadableItem
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string displayName = "";

    [ObservableProperty]
    private string filePath = "";

    [ObservableProperty]
    private bool? isSelected = true;

    [ObservableProperty]
    private bool isExpand = true;

    partial void OnIsSelectedChanged(bool? value)
    {
        if (Parent is not null)
        {
            //Notify parent
            Parent.NotifyChildSelectionChanged();
        }
    }

    UploadableFolder? parent = null;
    public UploadableFolder? Parent
    {
        get => parent;
        private set => parent = value;
    }

    public UploadableFile(UploadableFolder? root = null)
    {
        parent = root;
    }
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