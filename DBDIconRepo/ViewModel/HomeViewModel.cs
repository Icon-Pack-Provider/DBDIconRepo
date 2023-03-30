using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.Design.AxImporter;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel;

public partial class HomeViewModel : ObservableObject, IDisposable
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGettingPacks))]
    bool gettingPacks = true;

    public Visibility IsGettingPacks => GettingPacks ? Visibility.Visible : Visibility.Collapsed;

    public HomeViewModel(Task<Pack[]> packGatherMethod, PackDisplayComponentOptions compOption)
    {
        InitializeVM();
        Task.Run(async () =>
        {
            var packs = await packGatherMethod;

            AllAvailablePack = new();
            Packs.Initialize(OctokitService.Instance.GitHubClientInstance, SettingManager.Instance.CacheAndDisplayDirectory);
        foreach (var pack in packs)
        {
                PackDisplay pd = new(pack, compOption);
            if (pd is null) //Somehow??
                continue;
            //Exclusion
            if (pd.Info.Author == "Icon-Pack-Provider")
            {
                if (pd.Info.Name != "Dead-by-daylight-Default-icons" && !SettingManager.Instance.ShowDevTestPack)
                    continue;
                else if (pd.Info.Name == "Dead-by-daylight-Default-icons" && !SettingManager.Instance.ShowDefaultPack)
                    continue;
            }
            //check if this pack have anything
            if (pd.Info.ContentInfo.Files.Count < 1)
                continue;
            else if (!HasAnyContentType(pd.Info.ContentInfo))
            {
                continue;
            }
            else
            {
                //Try verify file list again?
                pd.Info.ContentInfo.VerifyContentInfo();
                if (!HasAnyContentType(pd.Info.ContentInfo))
                {
                    continue;
                }
            }
            //Check for infos about pack repository and cache to disk
            //Check readme
            await Packs.CheckPackReadme(pack);
            //Banner and urls
            await pd.GatherPreview();
            AllAvailablePack.Add(pd);
            await Task.Delay(100);
        }
            InitializeVM();
        }).Await(() =>
        {
            GettingPacks = false;
            if (AllAvailablePack is not null)
            {
                CanSearch = AllAvailablePack.Count > 0;
                ApplyFilter();
            }
        }, (e) =>
        {
            GettingPacks = false;
            if (AllAvailablePack is not null)
            {
                CanSearch = AllAvailablePack.Count > 0;
                ApplyFilter();
    }
        });
    }

    bool HasAnyContentType(PackContentInfo pci)
    {
        if (!pci.HasAddons &&
            !pci.HasItems &&
            !pci.HasOfferings &&
            !pci.HasPerks &&
            !pci.HasPortraits &&
            !pci.HasPowers &&
            !pci.HasStatus)
            return false;
        return true;
    }

    public HomeViewModel() { }

    [ObservableProperty]
    private PackDisplayComponentOptions componentOptions = new();

    bool _hasInitialized = false;
    private void InitializeVM()
    {
        if (_hasInitialized)
            return;
        _hasInitialized = true;
        //Monitor settings
        SettingManager.Instance.PropertyChanged += MonitorSettingChanged;
        //Register messages
        Messenger.Default.Register<HomeViewModel, RequestSearchQueryMessage, string>(this,
            MessageToken.REQUESTSEARCHQUERYTOKEN, HandleRequestedSearchQuery);
        Messenger.Default.Register<HomeViewModel, FilterOptionChangedMessage, string>(this,
            MessageToken.FILTEROPTIONSCHANGETOKEN, HandleFilterChanged);
    }

    public void Dispose()
    {
        //Unregister messages
        UnregisterMessages();
    }

    private void HandleFilterChanged(HomeViewModel recipient, FilterOptionChangedMessage message)
    {
        _queryDebouncer.Debounce(500, () =>
        {
            ApplyFilter();
        });
    }

    private void MonitorSettingChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Save setting
        SettingManager.SaveSettings();
        //Remove default pack setting
        OnPropertyChanged(nameof(FilteredList));
    }

    private void HandleRequestedSearchQuery(HomeViewModel recipient, RequestSearchQueryMessage message)
    {
        if (message.Query is null)
            return;
        SearchQuery = message.Query;
    }

    public void UnregisterMessages()
    {
        //Stop monitoring settings
        SettingManager.Instance.PropertyChanged -= MonitorSettingChanged;
        //Unregister messages
        Messenger.Default.UnregisterAll(this);
    }

    private void ApplyFilter()
    {
        OnPropertyChanged(nameof(FilteredList));
    }

    [ObservableProperty]
    ObservableCollection<PackDisplay>? allAvailablePack;

    [ObservableProperty]
    bool isFilteredListEmpty;

    [ObservableProperty]
    bool canSearch = false;

    private Debouncer _queryDebouncer { get; } = new();
    string _query = string.Empty;
    public string SearchQuery
    {
        get => _query;
        set
        {
            if (SetProperty(ref _query, value))
            {
                _queryDebouncer.Debounce(value.Length == 0 ? 100 : 500, () =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (QueryResults is null)
                            QueryResults = new();
                        else
                            QueryResults.Clear();
                        //Pack name search
                        var allFoundName = AllAvailablePack
                        .Where(i => i.Info.Name.ToLower().Contains(value.ToLower()))
                        .Select(i => i.Info.Name).Distinct();
                        var allFoundAuthor = AllAvailablePack
                        .Where(i => i.Info.Author.Contains(value.ToLower()))
                        .Select(i => i.Info.Author).Distinct();
                        QueryResults = new(allFoundName.Concat(allFoundAuthor));

                        //Trigger filter
                        ApplyFilter();
                    });
                });
            }
        }
    }

    [ObservableProperty]
    ObservableCollection<string> queryResults = new();

    public ObservableCollection<PackDisplay> FilteredList
    {
        get
        {
            if (AllAvailablePack is null)
                return new ObservableCollection<PackDisplay>();

            List<PackDisplay> filtering = new(AllAvailablePack);

            //A search query option; null if SearchQuery is empty
            string? query = SearchQuery != string.Empty ?
                SearchQuery.Trim().ToLower() : null;
            //A search query for pack name (Replace " " with "-")
            string? nameAltQuery = query is not null ?
                query.Replace(' ', '-') : null;

            var filter = Config.FilterOptions;
            //Filter by pack with specific components
            for (int i = filtering.Count - 1; i >= 0; i--)
            {

                //Search query filter
                if (query is not null) //No query; Don't filter with query
                {
                    //Is this pack match with search query of name
                    //Use alt-name query if the pack using git repo name (where space is dash)
                    bool isNameMatch = filtering[i].Info.Name.Contains('-') ?
                        filtering[i].Info.Name.ToLower().Contains(query) :
                        filtering[i].Info.Name.ToLower().Contains(nameAltQuery);
                    //Is this pack match with searhc query of author name
                    bool isAuthorMatch = filtering[i].Info.Author.ToLower().Contains(query);
                    if (!isNameMatch && !isAuthorMatch)
                    {
                        //Not match any search query
                        //Remove
                        filtering.RemoveAt(i);
                    }
                }

                var itemCIF = filtering[i].Info.ContentInfo;
                //Component based filtering
                if ((filter.HasPerks && itemCIF.HasPerks) ||
                    (filter.HasAddons && itemCIF.HasAddons) ||
                    (filter.HasItems && itemCIF.HasItems) ||
                    (filter.HasOfferings && itemCIF.HasOfferings) ||
                    (filter.HasPowers && itemCIF.HasPowers) ||
                    (filter.HasStatus && itemCIF.HasStatus) ||
                    (filter.HasPortraits && itemCIF.HasPortraits))
                {
                    continue;
                }
                else
                {
                    filtering.RemoveAt(i);
                }
            }

            IsFilteredListEmpty = filtering.Count == 0;
            if (filtering.Count <= 0)
            {
                return new ObservableCollection<PackDisplay>();
            }

            //Sort before return
            switch (Config.SortBy)
            {
                case SortOptions.Name:
                    if (Config.SortAscending)
                        filtering = filtering.OrderBy(i => i.Info.Name).ToList();
                    else
                        filtering = filtering.OrderByDescending(i => i.Info.Name).ToList();
                    break;
                case SortOptions.Author:
                    if (Config.SortAscending)
                        filtering = filtering.OrderBy(i => i.Info.Author).ToList();
                    else
                        filtering = filtering.OrderByDescending(i => i.Info.Author).ToList();
                    break;
                case SortOptions.LastUpdate:
                    if (Config.SortAscending)
                        filtering = filtering.OrderBy(i => i.Info.LastUpdate).ToList();
                    else
                        filtering = filtering.OrderByDescending(i => i.Info.LastUpdate).ToList();
                    break;
            }
            return new ObservableCollection<PackDisplay>(filtering);
        }
    }

    public Setting? Config => SettingManager.Instance;

    #region Commands
    [RelayCommand] private void SetInstallEverything() => Config.InstallEverythingInPack = true;
    [RelayCommand] private void SetInstallSpcific() => Config.InstallEverythingInPack = false;


    [RelayCommand]
    private void OnlyPerkFilter(RoutedEventArgs? obj)
    {
        Config.FilterOptions.HasPerks = true;
        Config.FilterOptions.HasOfferings =
            Config.FilterOptions.HasStatus =
            Config.FilterOptions.HasPowers =
            Config.FilterOptions.HasPortraits =
            Config.FilterOptions.HasItems =
            Config.FilterOptions.HasAddons = false;
        OnPropertyChanged(nameof(FilteredList));
    }

    [RelayCommand]
    private void OnlyPortraitFilter(RoutedEventArgs? obj)
    {
        Config.FilterOptions.HasPortraits = true;
        Config.FilterOptions.HasOfferings =
            Config.FilterOptions.HasStatus =
            Config.FilterOptions.HasPowers =
            Config.FilterOptions.HasPerks =
            Config.FilterOptions.HasItems =
            Config.FilterOptions.HasAddons = false;
        OnPropertyChanged(nameof(FilteredList));
    }

    [RelayCommand]
    private void ClearFilter(RoutedEventArgs? obj)
    {
        Config.FilterOptions.HasOfferings =
            Config.FilterOptions.HasStatus =
            Config.FilterOptions.HasPowers =
            Config.FilterOptions.HasPortraits =
            Config.FilterOptions.HasAddons =
            Config.FilterOptions.HasItems =
            Config.FilterOptions.HasPerks = true;
        OnPropertyChanged(nameof(FilteredList));
    }

    [RelayCommand]
    private void SortByAscending() => SetSortAscending(true);

    [RelayCommand]
    private void SortByDescending() => SetSortAscending(false);

    public void SetSortAscending(bool value)
    {
        Config.SortAscending = value;
        OnPropertyChanged(nameof(FilteredList));
    }

    [RelayCommand]
    private void SortByName() => SetSortOption(SortOptions.Name);
    [RelayCommand]
    private void SortByAuthor() => SetSortOption(SortOptions.Author);
    [RelayCommand]
    private void SortByLastUpdate() => SetSortOption(SortOptions.LastUpdate);

    public void SetSortOption(SortOptions option)
    {
        Config.SortBy = option;
        OnPropertyChanged(nameof(FilteredList));
    }
    #endregion


    private PackView? currentPackView = null;
    public PackView CurrentPackView
    {
        get
        {
            if (currentPackView == null)
            {
                currentPackView = (PackView)Config.PackViewMode;
            }
            return currentPackView.Value;
        }
        set
        {
            if (SetProperty(ref currentPackView, value))
            {
                Config.PackViewMode = (int)value;
                OnPropertyChanged(nameof(ShowOnGridView));
                OnPropertyChanged(nameof(ShowOnListView));
                OnPropertyChanged(nameof(ShowOnTableView));
                OnPropertyChanged(nameof(CurrentViewIsGrid));
                OnPropertyChanged(nameof(CurrentViewIsList));
                OnPropertyChanged(nameof(CurrentViewIsTable));
            }
        }
    }

    public Visibility ShowOnGridView => CurrentPackView == PackView.Grid ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnListView => CurrentPackView == PackView.List ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ShowOnTableView => CurrentPackView == PackView.Table ? Visibility.Visible : Visibility.Collapsed;

    public bool CurrentViewIsGrid => CurrentPackView == PackView.Grid;
    public bool CurrentViewIsList => CurrentPackView == PackView.List;
    public bool CurrentViewIsTable => CurrentPackView == PackView.Table;

    [RelayCommand] private void SetViewToGrid() => CurrentPackView = PackView.Grid;
    [RelayCommand] private void SetViewToList() => CurrentPackView = PackView.List;
    [RelayCommand] private void SetViewToTable() => CurrentPackView = PackView.Table;
}

public enum PackView
{
    Grid,
    List,
    Table
}

public class PackDisplayComponentOptions
{
    public bool ShowFavoriteComponent { get; set; } = true;

    public PackDisplayComponentOptions() { }
}