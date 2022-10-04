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

namespace DBDIconRepo.Model;

//Use for commands and parameter for bindings
public partial class PackDisplay : ObservableObject
{
    public PackDisplay(Pack _info)
    {
        Info = _info;
        SettingManager.Instance.PropertyChanged += MonitorSetting;
    }

    private void MonitorSetting(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Setting.InstallEverythingInPack))
            return;
        installPackOption = SettingManager.Instance.InstallEverythingInPack;
        OnPropertyChanged(nameof(ShouldInstallEverything));
    }

    [ObservableProperty]
    Pack? info;

    //Include images
    //Limit to just 4 items!
    [ObservableProperty]
    ObservableCollection<IDisplayItem>? previewSources;

    [RelayCommand]
    private void OpenPackDetailWindow(RoutedEventArgs? obj)
    {
        Messenger.Default.Send(new RequestViewPackDetailMessage(Info), MessageToken.REQUESTVIEWPACKDETAIL);
    }

    [RelayCommand]
    private async void InstallThisPack(RoutedEventArgs? obj)
    {
        ObservableCollection<IPackSelectionItem>? installationPick = new();
        if (ShouldInstallEverything)
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
    private void SearchForThisAuthor(RoutedEventArgs? obj)
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
                PreviewSources.Add(new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, icon)));
            }
        }
    }

    bool? installPackOption = null;
    public bool ShouldInstallEverything
    {
        get
        {
            if (installPackOption is null)
                installPackOption = SettingManager.Instance.InstallEverythingInPack;
            return installPackOption.Value;
        }
        set
        {
            if (SetProperty(ref installPackOption, value))
            {
                SettingManager.Instance.InstallEverythingInPack = value;
            }
        }
    }

    [RelayCommand]
    private void SetInstallAll()
    {
        ShouldInstallEverything = true;
    }

    [RelayCommand]
    private void SetNotInstallAll()
    {
        ShouldInstallEverything = false;
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
    }

    [RelayCommand]
    private async Task UnFavoriteThisPack()
    {
        await star.BaseService.UnStar(Info.Repository);
        isFav = false;
        OnPropertyChanged(nameof(IsFavorited));
    }

    
    #endregion
}