using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Dialog;
using DBDIconRepo.Helper;
using DBDIconRepo.Service;
using DBDIconRepo.ViewModel;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;
using IconFolder = IconInfo.Strings.Terms;
using System.Text;
using IconInfo.Icon;

namespace DBDIconRepo.Model;

//Use for commands and parameter for bindings
public partial class PackDisplay : ObservableObject
{
    public PackDisplay(Pack _info)
    {
        Info = _info;

        InitializeCommand();
    }

    Pack? _base;
    public Pack? Info
    {
        get => _base;
        set => SetProperty(ref _base, value);
    }

    //Include images
    ObservableCollection<IDisplayItem>? _previewSauces;
    //Limit to just 4 items!
    public ObservableCollection<IDisplayItem>? PreviewSources
    {
        get => _previewSauces;
        set => SetProperty(ref _previewSauces, value);
    }

    //
    public ICommand? SearchForThisAuthor { get; private set; }
    public ICommand? InstallThisPack { get; private set; }
    public ICommand? OpenPackDetailWindow { get; private set; }

    private void InitializeCommand()
    {
        SearchForThisAuthor = new RelayCommand<RoutedEventArgs>(SearchForThisAuthorAction); 
        InstallThisPack = new RelayCommand<RoutedEventArgs>(InstallThisPackAction);
        OpenPackDetailWindow = new RelayCommand<RoutedEventArgs>(OpenPackDetailWindowAction);
    }

    private void OpenPackDetailWindowAction(RoutedEventArgs? obj)
    {
        Messenger.Default.Send(new RequestViewPackDetailMessage(Info), MessageToken.REQUESTVIEWPACKDETAIL);
    }

    private async void InstallThisPackAction(RoutedEventArgs? obj)
    {
        //Show selection
        PackInstall install = new(Info);
        if (install.ShowDialog() == true)
        {
            ObservableCollection<IPackSelectionItem>? installPick = (install.DataContext as PackInstallViewModel).InstallableItems;

            bool result = DownloadSomeOrAllConsultant.ShouldCloneOrNot(installPick);

            if (result)
            {
                //Clone
                //Register for download progress
                Messenger.Default.Register<PackDisplay, DownloadRepoProgressReportMessage, string>(this,
                    $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{Info.Repository.Name}",
                    HandleDownloadProgress);
                Messenger.Default.Register<PackDisplay, InstallationProgressReportMessage, string>(this,
                    $"{MessageToken.REPORTINSTALLPACKTOKEN}{Info.Repository.Name}",
                    HandleInstallProgress);
                //Actually ask to start download
                CacheOrGit.DownloadPack(await OctokitService.Instance.GitHubClientInstance.Repository.Get(Info.Repository.ID), Info).Await(() =>
                {
                    IconManager.Install(Setting.Instance.DBDInstallationPath, installPick.Where(i => i.IsSelected == true).ToList(), Info);
                });
            }
            else
            {
                //TODO:Properly handle this download method
                //Register for download progress
                Messenger.Default.Register<PackDisplay, IndetermineRepoProgressReportMessage, string>(this,
                    $"{MessageToken.REPOSITORYDOWNLOADREPORTTOKEN}{Info.Repository.Name}",
                    HandleDownloadProgress);
                Messenger.Default.Register<PackDisplay, InstallationProgressReportMessage, string>(this,
                    $"{MessageToken.REPORTINSTALLPACKTOKEN}{Info.Repository.Name}",
                    HandleInstallProgress);
                GitAbuse.DownloadIndivisualItems(installPick, Info).Await(() =>
                {
                    IconManager.Install(Setting.Instance.DBDInstallationPath, installPick.Where(i => i.IsSelected == true).ToList(), Info);
                });
            }
        }            
    }

    private void SearchForThisAuthorAction(RoutedEventArgs? obj)
    {
        Messenger.Default.Send(new RequestSearchQueryMessage(Info.Author), MessageToken.REQUESTSEARCHQUERYTOKEN);
    }

    PackState _state = PackState.None;
    public PackState CurrentPackState
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    double _totalProgress;
    public double TotalDownloadProgress
    {
        get => _totalProgress;
        set => SetProperty(ref _totalProgress, value);
    }

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

    int _installProgress = -1;
    public int CurrentInstallProgress
    {
        get => _installProgress;
        set => SetProperty(ref _installProgress, value);
    }

    int _totalInstall = -1;
    public int TotalInstallProgress
    {
        get => _totalInstall;
        set => SetProperty(ref _totalInstall, value);
    }

    string? _latestInstall;
    public string? LatestInstalledFile
    {
        get => _latestInstall;
        set => SetProperty(ref _latestInstall, value);
    }
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
            //Messenger.Default.Register<PackDisplay, InstallationProgressReportMessage, string>(this,
            //    $"{MessageToken.REPORTINSTALLPACKTOKEN}{Info.Repository.Name}",
            //    HandleInstallProgress);
        }
    }

    public void HandleURLs()
    {
        if (PreviewSources is null)
            PreviewSources = new ObservableCollection<IDisplayItem>();
        //Is this pack have banner?
        string path = CacheOrGit.GetDisplayContentPath(Info.Repository.Owner, Info.Repository.Name);
        if (File.Exists($"{path}\\.banner.png")) //Banner exist, link to it on github
        {
            //Load image
            PreviewSources.Add(new BannerDisplay(URL.GetGithubRawContent(Info.Repository, ".banner.png")));
        }
        else //banner not exist, get URLs for perk icons that required to display on setting
        {
            var matchSearch = new List<string>();
            foreach (var item in Setting.Instance.PerkPreviewSelection)
            {
                StringBuilder matcher = new();
                if (item is Perk perk)
                {
                    matcher.Append(IconFolder.Perk);
                    if (!string.IsNullOrEmpty(perk.Folder))
                    {
                        matcher.Append('/');
                        matcher.Append(perk.Folder);
                    }
                    matcher.Append('/');
                    matcher.Append(perk.File);
                    matcher.Append(".png");
                }
                //else if (item is Portrait portrait)
                //{
                //    matcher.Append(IconFolder.Portrait);
                //    if (!string.IsNullOrEmpty(portrait.Folder))
                //    {
                //        matcher.Append('/');
                //        matcher.Append(portrait.Folder);
                //    }
                //    matcher.Append('/');
                //    matcher.Append(portrait.File);
                //    matcher.Append(".png");
                //}
                //TODO:Add support for displaying other stuff

                int index = Info.ContentInfo.Files.IndexOf(matcher.ToString());
                if (index >= 0)
                    matchSearch.Add(Info.ContentInfo.Files[index]);
            }
            if (matchSearch.Count < 4)
            {
                //Fill the rest with random
                while (matchSearch.Count < 4)
                {
                    Random r = new();
                    var randomPick = Info.ContentInfo.Files[r.Next(0, Info.ContentInfo.Files.Count)];
                    if (!matchSearch.Contains(randomPick))
                        matchSearch.Add(randomPick);
                }
            }
            foreach (var icon in matchSearch)
            {
                PreviewSources.Add(new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, icon)));
            }
        }
    }
}
