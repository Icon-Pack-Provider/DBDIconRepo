using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Dialog;
using DBDIconRepo.Helper;
using DBDIconRepo.ViewModel;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using IconPack;
using System.Threading.Tasks;
using DBDIconRepo.Service;
using IconInfo.Internal;

namespace DBDIconRepo.Model;

//Use for commands and parameter for bindings
public partial class PackDisplay : ObservableObject
{
    public PackDisplay(Pack _info)
    {
        Info = _info;
    }

    [ObservableProperty]
    Pack? info;

    //Include images
    //Limit to just 4 items!
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MainPreviewItem))]
    [NotifyPropertyChangedFor(nameof(ShowIfMainPreviewIsIcon))]
    [NotifyPropertyChangedFor(nameof(RevealIfMainPreviewIsIcon))]
    [NotifyPropertyChangedFor(nameof(ShowIfMainPreviewIsBanner))]
    [NotifyPropertyChangedFor(nameof(RevealIfMainPreviewIsBanner))]
    ObservableCollection<IDisplayItem>? previewSources;

    public IDisplayItem? MainPreviewItem
    {
        get
        {
            if (PreviewSources is null)
                return null;
            if (PreviewSources.Count < 1)
                return null;
            return PreviewSources[0];
        }
    }

    public bool ShowIfMainPreviewIsIcon => MainPreviewItem is not null && MainPreviewItem is IconDisplay;
    public Visibility RevealIfMainPreviewIsIcon => ShowIfMainPreviewIsIcon ? Visibility.Visible : Visibility.Collapsed;

    public bool ShowIfMainPreviewIsBanner => MainPreviewItem is not null && MainPreviewItem is BannerDisplay;
    public Visibility RevealIfMainPreviewIsBanner => ShowIfMainPreviewIsBanner ? Visibility.Visible : Visibility.Collapsed;

    [RelayCommand]
    private void OpenPackDetailWindow()
    {
        Messenger.Default.Send(new RequestViewPackDetailMessage(Info), MessageToken.REQUESTVIEWPACKDETAIL);
    }

    [RelayCommand]
    private async void InstallThisPack()
    {
        if (string.IsNullOrEmpty(SettingManager.Instance.DBDInstallationPath))
        {
            MessageBox.Show("Please check the Setting and set the Dead by Daylight installation folder",
                "Installation path hasn't set yet.", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        ObservableCollection<IPackSelectionItem>? installationPick = new();
        if (SettingManager.Instance.InstallEverythingInPack)
        {
            installationPick = new(Info.ContentInfo.Files
                .Where(file => file.EndsWith(".png") && !file.StartsWith(".banner"))
                .Select(path => new PackSelectionFile(path)));
            goto cloning;
        }
        //Show selection
        PackInstall install = new(Info);
        if (install.ShowDialog() == true)
        {
            installationPick = (install.DataContext as PackInstallViewModel).InstallableItems;
            goto cloning;
        }
        else
        {
            return;
        }

        cloning:
        Messenger.Default.Register<PackDisplay, DownloadRepoProgressReportMessage, string>(this,
            $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{Info.Repository.Name}",
            HandleDownloadProgress);
        Messenger.Default.Register<PackDisplay, InstallationProgressReportMessage, string>(this,
            $"{MessageToken.REPORTINSTALLPACKTOKEN}{Info.Repository.Name}",
            HandleInstallProgress);
        await Packs.ClonePackToCacheFolder(Info, (progress) =>
        {
            Messenger.Default.Send(new DownloadRepoProgressReportMessage(progress),
                $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{Info.Repository.Name}");
        });
        IconManager.Install(SettingManager.Instance.DBDInstallationPath, installationPick.Where(i => i.IsSelected == true).ToList(), Info);
    }

    [RelayCommand]
    private void SearchForThisAuthor()
    {
        Messenger.Default.Send(new RequestSearchQueryMessage(Info.Author), MessageToken.REQUESTSEARCHQUERYTOKEN);
    }

    [ObservableProperty]
    PackState currentPackState = PackState.None;

    [ObservableProperty]
    double totalDownloadProgress;

    private void HandleDownloadProgress(PackDisplay recipient, DownloadRepoProgressReportMessage message)
    {
        CurrentPackState = PackState.Downloading;
        //Update progress
        /*Progress detail: Enumerating => 0-5%,
         * Compressing => 5-20%,
         * Transfering => 20-90%,
         * CheckingOut => 90-100%,
         * Done => 100%*/
        switch (message.CurrentState)
        {
            case DownloadState.Enumerating:
                TotalDownloadProgress = Math.Round(message.EstimateProgress * 5d);
                break;
            case DownloadState.Compressing:
                TotalDownloadProgress = 5d + Math.Round(message.EstimateProgress * 15d);
                break;
            case DownloadState.Transfering:
                TotalDownloadProgress = 20d + Math.Round(message.EstimateProgress * 70d);
                break;
            case DownloadState.CheckingOut:
                TotalDownloadProgress = 90d + Math.Round(message.EstimateProgress * 10d);
                break;
        }

        //Check if it's done
        if ((TotalDownloadProgress >= 99 && message.CurrentState <= DownloadState.CheckingOut)
            || message.CurrentState == DownloadState.Done)
        {
            Messenger.Default.Unregister<DownloadRepoProgressReportMessage, string>(recipient, $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{Info.Repository.Name}");
            TotalDownloadProgress = 100;
        }
    }

    [ObservableProperty]
    int currentInstallProgress = -1;

    [ObservableProperty]
    int totalInstallProgress = -1;

    [ObservableProperty]
    string? latestInstalledFile;

    private void HandleInstallProgress(PackDisplay recipient, InstallationProgressReportMessage message)
    {
        CurrentPackState = PackState.Installing;

        if (CurrentInstallProgress == -1)
            CurrentInstallProgress = 0;
        else
            CurrentInstallProgress++;

        LatestInstalledFile = message.Filename;
        TotalInstallProgress = message.TotalInstall;

        if (CurrentInstallProgress >= TotalInstallProgress - 1)
        {
            CurrentPackState = PackState.None;
            MessageBox.Show($"Pack {Info.Name} installed");
            Messenger.Default.Unregister<InstallationProgressReportMessage, string>(this,
                $"{MessageToken.REPORTINSTALLPACKTOKEN}{Info.Repository.Name}");
        }
    }

    public async Task GatherPreview()
    {
        if (PreviewSources is null)
            PreviewSources = new ObservableCollection<IDisplayItem>();
        //Does this pack has override preview?
        if (Info.Overrides is not null && Info.Overrides.DisplayFiles is not null)
        {
            //Check if it has icons override
            if (Info.Overrides.DisplayFiles.Count == 1 && Info.Overrides.DisplayFiles[0] == ".banner.png")
            {
                //Only banner
                var bannerURL = await Packs.GetPackBannerURL(Info);
                PreviewSources.Add(new BannerDisplay(bannerURL));
                return;
            }
            foreach (var icon in Info.Overrides.DisplayFiles)
            {
                var url = Packs.GetPackItemOnGit(info, icon);
                if (url is null)
                    continue;
                var newIcon = new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, icon));
                IBasic? iconInfo = IconTypeIdentify.FromPath(icon);
                if (iconInfo is not null || iconInfo is not UnknownIcon)
                    newIcon.Tooltip = iconInfo;
                PreviewSources.Add(newIcon);
            }
            return;
        }
        //Is this pack have banner?
        var bannerState = await Packs.IsPackBannerExist(Info);
        if (bannerState)
        {
            var bannerURL = await Packs.GetPackBannerURL(Info);
            PreviewSources.Add(new BannerDisplay(bannerURL));
            return;
        }
        else //banner not exist, get URLs for perk icons that required to display on setting
        {
            var matchSearch = new List<string>();
            foreach (var item in SettingManager.Instance.PerkPreviewSelection)
            {
                var found = info.ContentInfo.Files.FirstOrDefault(f => f.Contains(item.File));
                if (found is null)
                    continue;
                var index = info.ContentInfo.Files.IndexOf(found);
                if (index >= 0)
                    matchSearch.Add(Info.ContentInfo.Files[index]);
            }
            if (matchSearch.Count < 4)
            {
                //Fill the rest with random
                if (Info.ContentInfo.Files.Count <= 4)
                {
                    matchSearch.Clear();
                    matchSearch.AddRange(Info.ContentInfo.Files);
                }
                else
                {
                    while (matchSearch.Count < 4)
                    {
                        Random r = new();
                        var randomPick = Info.ContentInfo.Files[r.Next(0, Info.ContentInfo.Files.Count)];
                        if (!matchSearch.Contains(randomPick))
                            matchSearch.Add(randomPick);
                    }
                }
            }
            foreach (var icon in matchSearch)
            {
                var url = Packs.GetPackItemOnGit(info, icon);
                if (url is null)
                    continue;
                var newIcon = new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, icon));
                IBasic? iconInfo = IconTypeIdentify.FromPath(icon);
                if (iconInfo is not null || iconInfo is not UnknownIcon)
                    newIcon.Tooltip = iconInfo;
                PreviewSources.Add(newIcon);
            }
        }
    }

    #region Favorite/Starred
    bool isInitialized = false;
    bool? isFav = null;
    public bool? IsFavorited
    {
        get
        {
            if (isFav is not null || isInitialized)
            {
                return isFav.Value;
            }
            Action<Task<bool>> loading = async task =>
            {
                isFav = await task;
                OnPropertyChanged(nameof(IsFavorited));
                OnPropertyChanged(nameof(IsFavorited));
            };
            StarService.Instance.BaseService.IsRepoStarred(Info.Repository)
                .ContinueWith(loading,
                TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(_ => isInitialized = false)
                .ConfigureAwait(false);
            return null;
        }
    }

    public bool CanFavorite => IsFavorited is not null;
    StarService star = StarService.Instance;

    [RelayCommand]
    private async Task FavoriteThisPack()
    {
        await star.BaseService.Star(Info.Repository);
        isFav = true;
        OnPropertyChanged(nameof(IsFavorited));
        Messenger.Default.Send(new RepoStarChangedMessage(Info.Repository, true),
            MessageToken.RepoStarChangedToken);
    }

    [RelayCommand]
    private async Task UnFavoriteThisPack()
    {
        await star.BaseService.UnStar(Info.Repository);
        isFav = false;
        OnPropertyChanged(nameof(IsFavorited));
        Messenger.Default.Send(new RepoStarChangedMessage(Info.Repository, false),
            MessageToken.RepoStarChangedToken);
    }

    
    #endregion
}