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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.Model
{
    //Use for commands and parameter for bindings
    public partial class PackDisplay : ObservableObject
    {
        public PackDisplay(Pack _info)
        {
            Info = _info;

            InitializeCommand();
        }

        [ObservableProperty]
        Pack? info;

        //Include images
        //Limit to just 4 items!
        [ObservableProperty]
        ObservableCollection<IDisplayItem>? previewSources;

        [RelayCommand]
        private void OpenPackDetailWindow()
        {
            Messenger.Default.Send(new RequestViewPackDetailMessage(Info), MessageToken.REQUESTVIEWPACKDETAIL);
        }

        [RelayCommand]
        private async Task InstallThisPack()
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
                PreviewSources.Add(new BannerDisplay(URL.GetGithubRawContent(Info.Repository, ".banner.png")));
            else //banner not exist, get URLs for perk icons that required to display on setting
            {
                foreach (var icon in Setting.Instance.PerkPreviewSelection)
                {
                    if (Info.ContentInfo.Files.FirstOrDefault(i => i.ToLower().Contains(icon.ToLower())) is string match)
                    {
                        //This pack have this exact icon
                        PreviewSources.Add(new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, match)));
                    }
                }
                if (PreviewSources.Count < 1)
                {
                    //No content match the preview? Pick 4 random!
                    //If the pack has less than 10 icons? Don't try to random or it's gonna stuck too long on while loop!
                    string[] allPngs = Info.ContentInfo.Files.Where(i => i.EndsWith(".png")).ToArray();
                    if (allPngs.Length <= 10)
                    {
                        //Show all that its has or first four
                        PreviewSources = new ObservableCollection<IDisplayItem>(
                            Info.ContentInfo.Files.Select(file => new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, file))));
                        return;
                    }
                    List<int> AllRandomNumbers = new List<int>();
                    Random randomizer = new Random();
                    while (AllRandomNumbers.Count < 5)
                    {
                        int randomNumber = randomizer.Next(0, allPngs.Length);
                        if (!AllRandomNumbers.Contains(randomNumber))
                            AllRandomNumbers.Add(randomNumber);
                    }
                    foreach (var random in AllRandomNumbers)
                    {
                        PreviewSources.Add(new IconDisplay(URL.GetIconAsGitRawContent(Info.Repository, allPngs[random])));
                    }
                }
            }
        }
    }

    public interface IDisplayItem
    {
        string URL { get; set; }
    }

    public class OnlineSourceDisplay : ObservableObject, IDisplayItem
    {
        public OnlineSourceDisplay() { }

        public OnlineSourceDisplay(string url) { URL = url; }

        string? _url;
        public string? URL
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }
    }

    public enum PackState
    {
        None,
        Downloading,
        Installing
    }

    public class IconDisplay : OnlineSourceDisplay 
    { 
        public IconDisplay() { }
        public IconDisplay(string url) : base(url) { }
    }

    public class BannerDisplay : OnlineSourceDisplay
    {
        public BannerDisplay() { }
        public BannerDisplay(string url) : base(url) { }
    }
}
