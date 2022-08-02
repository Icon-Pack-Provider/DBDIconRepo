using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack.Model;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        OctokitService gitService => OctokitService.Instance;
        GitHubClient client => gitService.GitHubClientInstance;
        public void InitializeViewModel()
        {
            //Initialize commands
            InitializeCommands();

            //Monitor settings
            Messenger.Default.Register<HomeViewModel, SettingChangedMessage, string>(this,
                MessageToken.SETTINGVALUECHANGETOKEN, HandleSettingValueChanged);

            //Register messages
            Messenger.Default.Register<HomeViewModel, RequestSearchQueryMessage, string>(this,
                MessageToken.REQUESTSEARCHQUERYTOKEN, HandleRequestedSearchQuery);

            if (AllAvailablePack is null)
                AllAvailablePack = new ObservableCollection<PackDisplay>();
            FindPack().Await(() =>
            {
                //Filters
                ApplyFilter();
                //Monitor settings
                Messenger.Default.Register<HomeViewModel, FilterOptionChangedMessage, string>(this, MessageToken.FILTEROPTIONSCHANGETOKEN, HandleFilterOptionChanged);
            });

            Task.Delay(5000).Await(() =>
            {
                Setting.EnableMessageGateOnSettingChanged();
            });
        }

        private void HandleRequestedSearchQuery(HomeViewModel recipient, RequestSearchQueryMessage message)
        {
            if (message.Query is null)
                return;
            SearchQuery = message.Query;
        }

        private void HandleSettingValueChanged(HomeViewModel recipient, SettingChangedMessage message)
        {
            Config.SaveSettings();
        }

        private void HandleFilterOptionChanged(HomeViewModel recipient, FilterOptionChangedMessage message)
        {
            OnPropertyChanged(nameof(FilteredList));
        }

        public void UnregisterMessages()
        {
            Messenger.Default.Unregister<FilterOptionChangedMessage, string>(this, MessageToken.FILTEROPTIONSCHANGETOKEN);
        }

        private void ApplyFilter()
        {
            OnPropertyChanged(nameof(FilteredList));
        }

        ObservableCollection<PackDisplay>? _packs;
        public ObservableCollection<PackDisplay> AllAvailablePack
        {
            get => _packs;
            set => SetProperty(ref _packs, value);
        }

        bool _isEmpty;
        public bool IsFilteredListEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }

        private Debouncer _queryDebouncer { get; } = new Debouncer();
        string _query;
        public string SearchQuery
        {
            get => _query;
            set
            {
                if (SetProperty(ref _query, value))
                {
                    _queryDebouncer.Debounce(value.Length == 0 ? 100 : 500, () =>
                    {
                        OnPropertyChanged(nameof(FilteredList));
                    });
                }
            }
        }

        public ObservableCollection<PackDisplay> FilteredList
        {
            get
            {
                if (AllAvailablePack is null)
                    return new ObservableCollection<PackDisplay>();
                var afterQuerySearch = new List<PackDisplay>(AllAvailablePack);
                if (!string.IsNullOrEmpty(SearchQuery))
                {
                    afterQuerySearch = AllAvailablePack.Where(pack => 
                    pack.Info.Name.ToLower().Contains(SearchQuery.ToLower()) || 
                    pack.Info.Author.ToLower().Contains(SearchQuery.ToLower())).ToList();
                }
                var newList = new List<PackDisplay>();

                //Filter by perks
                var perks = afterQuerySearch.Where(x => x.Info.ContentInfo.HasPerks);
                if (Config.FilterOptions.HasPerks)
                    newList = newList.Union(perks).ToList();

                //Filter by add-ons
                var addons = afterQuerySearch.Where(x => x.Info.ContentInfo.HasAddons);
                if (Config.FilterOptions.HasAddons)
                    newList = newList.Union(addons).ToList();

                //Filter by items
                var items = afterQuerySearch.Where(x => x.Info.ContentInfo.HasItems);
                if (Config.FilterOptions.HasItems)
                    newList = newList.Union(items).ToList();

                //Filter by offerings
                var offerings = afterQuerySearch.Where(x => x.Info.ContentInfo.HasOfferings);
                if (Config.FilterOptions.HasOfferings)
                    newList = newList.Union(offerings).ToList();

                //Filter by powers
                var powers = afterQuerySearch.Where(x => x.Info.ContentInfo.HasPowers);
                if (Config.FilterOptions.HasPowers)
                    newList = newList.Union(powers).ToList();

                //Filter by status
                var status = afterQuerySearch.Where(x => x.Info.ContentInfo.HasStatus);
                if (Config.FilterOptions.HasStatus)
                    newList = newList.Union(status).ToList();

                //Filter by portraits
                var portraits = afterQuerySearch.Where(x => x.Info.ContentInfo.HasPortraits);
                if (Config.FilterOptions.HasPortraits)
                    newList = newList.Union(portraits).ToList();

                IsFilteredListEmpty = newList.Count == 0;
                if (newList.Count > 0)
                {
                    //Sort before return
                    switch (Config.SortBy)
                    {
                        case SortOptions.Name:
                            if (Config.SortAscending)
                                newList = newList.OrderBy(i => i.Info.Name).ToList();
                            else
                                newList = newList.OrderByDescending(i => i.Info.Name).ToList();
                            break;
                        case SortOptions.Author:
                            if (Config.SortAscending)
                                newList = newList.OrderBy(i => i.Info.Author).ToList();
                            else
                                newList = newList.OrderByDescending(i => i.Info.Author).ToList();
                            break;
                        case SortOptions.LastUpdate:
                            if (Config.SortAscending)
                                newList = newList.OrderBy(i => i.Info.LastUpdate).ToList();
                            else
                                newList = newList.OrderByDescending(i => i.Info.LastUpdate).ToList();
                            break;
                    }
                }
                return new ObservableCollection<PackDisplay>(newList);
            }
        }

        public Setting? Config => Setting.Instance;

        #region GIT
        
        private const string PackTag = "dbd-icon-pack";
        public async Task FindPack()
        {
            //TODO:Somewhere before this, search if all result already cached
            var request = new SearchRepositoriesRequest($"topic:{PackTag}");
            var result = await client.Search.SearchRepo(request);
            foreach (var repo in result.Items)
            {
                var packInfo = await CacheOrGit.GetPack(repo);
                PackDisplay? gatheredPack = new(packInfo);
                
                if (gatheredPack is null)
                    continue;
                //Preview icons
                await CacheOrGit.GatherPackDisplayData(repo, gatheredPack.Info, Config.PerkPreviewSelection.ToArray());
                gatheredPack.HandleURLs();
                AllAvailablePack.Add(gatheredPack);
            }
        }

        #endregion

        #region Commands
        public ICommand? SetFilterOnlyPerks { get; private set; }
        public ICommand? SetFilterOnlyPortraits { get; private set; }
        public ICommand? SetFilterShowAll { get; private set; }
        public ICommand? SetSortAscendingOption { get; private set; }
        public ICommand? SetSortOptions { get; private set; }
        public ICommand? BrowseForSteamInstallationPath { get; private set; }
        public ICommand? FindDBDSteam { get; private set; }
        public ICommand? FindDBDXbox { get; private set; }
        public ICommand? FindDBDEpic { get; private set; }

        public ICommand? ResetSettings { get; private set; }
        public ICommand? UninstallIconPack { get; private set; }
        public ICommand? InstallThisPack { get; private set; }


        private void InitializeCommands()
        {
            //Filter
            SetFilterOnlyPerks = new RelayCommand<RoutedEventArgs>(SetFilterOnlyPerksAction);
            SetFilterOnlyPortraits = new RelayCommand<RoutedEventArgs>(SetFilterOnlyPortraitsAction);
            SetFilterShowAll = new RelayCommand<RoutedEventArgs>(SetFilterCompletePackAction);
            //Sort
            SetSortAscendingOption = new RelayCommand<bool>(SetSortAscendingOptionAction);
            SetSortOptions = new RelayCommand<string>(SetSortOptionsAction);
            //Setting
            BrowseForSteamInstallationPath = new RelayCommand<RoutedEventArgs>(BrowseForSteamInstallationPathAction);
            FindDBDSteam = new RelayCommand<RoutedEventArgs>(FindDBDSteamAction);
            FindDBDXbox = new RelayCommand<RoutedEventArgs>(FindDBDXboxAction);
            FindDBDEpic = new RelayCommand<RoutedEventArgs>(FindDBDEpicAction);
            //
            ResetSettings = new RelayCommand<RoutedEventArgs>(ResetSettingsAction);
            UninstallIconPack = new RelayCommand<RoutedEventArgs>(UninstallIconPackAction);
        }

        private void SetFilterOnlyPerksAction(RoutedEventArgs? obj)
        {
            Config.FilterOptions.HasPerks = true;
            Config.FilterOptions.HasOfferings =
                Config.FilterOptions.HasStatus =
                Config.FilterOptions.HasPowers =
                Config.FilterOptions.HasPortraits =
                Config.FilterOptions.HasAddons = false;
            OnPropertyChanged(nameof(FilteredList));
        }

        private void SetFilterOnlyPortraitsAction(RoutedEventArgs? obj)
        {
            Config.FilterOptions.HasPortraits = true;
            Config.FilterOptions.HasOfferings =
                Config.FilterOptions.HasStatus =
                Config.FilterOptions.HasPowers =
                Config.FilterOptions.HasPerks =
                Config.FilterOptions.HasAddons = false;
            OnPropertyChanged(nameof(FilteredList));
        }

        private void SetFilterCompletePackAction(RoutedEventArgs? obj)
        {
            Config.FilterOptions.HasOfferings =
                Config.FilterOptions.HasStatus =
                Config.FilterOptions.HasPowers =
                Config.FilterOptions.HasPortraits =
                Config.FilterOptions.HasAddons =
                Config.FilterOptions.HasPerks = true;
            OnPropertyChanged(nameof(FilteredList));
        }

        private void SetSortAscendingOptionAction(bool input)
        {
            Config.SortAscending = input;
            OnPropertyChanged(nameof(FilteredList));
        }

        private void SetSortOptionsAction(string? obj)
        {
            if (obj is null)
                return;
            SortOptions parse = Enum.Parse<SortOptions>(obj);
            Config.SortBy = parse;
            OnPropertyChanged(nameof(FilteredList));
        }

        private void BrowseForSteamInstallationPathAction(RoutedEventArgs? obj)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.CommonDocuments,
                ShowNewFolderButton = false,
                UseDescriptionForTitle = true,
                Description = "Locate Dead by Daylight installation folder"
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                //Validate path
                Config.DBDInstallationPath = dialog.SelectedPath;
            }
        }

        private void FindDBDSteamAction(RoutedEventArgs? obj)
        {
            //Locate steam installation folder
            string? steamPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", "").ToString();
            string libraryFolderFile = $"{steamPath}\\steamapps\\libraryfolders.vdf";
            if (File.Exists(libraryFolderFile))
            {
                string content = File.ReadAllText(libraryFolderFile);
                string dbdPath = SteamLibraryFolderHandler.GetDeadByDaylightPath(content);
                if (!string.IsNullOrEmpty(dbdPath))
                {
                    Config.DBDInstallationPath = dbdPath;
                }
            }
        }

        private void FindDBDXboxAction(RoutedEventArgs? obj)
        {
            throw new NotImplementedException();
        }

        private void FindDBDEpicAction(RoutedEventArgs? obj)
        {
            throw new NotImplementedException();
        }


        private void ResetSettingsAction(RoutedEventArgs? obj)
        {
            SettingManager.DeleteSettings();
            App.Current.Shutdown();
        }

        private void UninstallIconPackAction(RoutedEventArgs? obj)
        {
            if (string.IsNullOrEmpty(Config.DBDInstallationPath))
                return;
            IconManager.Uninstall(Config.DBDInstallationPath);
        }

        #endregion

    }
}
