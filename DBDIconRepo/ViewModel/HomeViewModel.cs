using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace DBDIconRepo.ViewModel;

public partial class HomeViewModel : ObservableObject
{
    public HomeViewModel()
    {
        //Monitor settings
        SettingManager.Instance.PropertyChanged += MonitorSettingChanged;
        //Register messages
        Messenger.Default.Register<HomeViewModel, RequestSearchQueryMessage, string>(this,
            MessageToken.REQUESTSEARCHQUERYTOKEN, HandleRequestedSearchQuery);
        ////Messenger.Default.Unregister<FilterOptionChangedMessage, string>(this, MessageToken.FILTEROPTIONSCHANGETOKEN);
        Messenger.Default.Register<HomeViewModel, FilterOptionChangedMessage, string>(this, 
            MessageToken.FILTEROPTIONSCHANGETOKEN, HandleFilterChanged);

        Task.Run(async () =>
        {
            //
            AllAvailablePack = new();
            //
            Packs.Initialize(OctokitService.Instance.GitHubClientInstance, SettingManager.Instance.CacheAndDisplayDirectory);
            var packs = await Packs.GetPacks();
            foreach (var pack in packs)
            {
                if (!Config.ShowDefaultPack)
                {
                    if (pack.Name == "Dead-by-daylight-Default-icons" &&
                        pack.Author == "Icon-Pack-Provider")
                    {
                        continue;
                    }
                }
                PackDisplay display = new(pack);
                if (display is null)
                    continue;
                //Check readme
                await Packs.CheckPackReadme(pack);
                //Check banner data
                await Packs.CheckPackBanner(pack);
                //Get banner or perks URL for pack showcasing/preview samples
                await display.GatherPreview();
                AllAvailablePack.Add(display);
            }
        }).Await(() =>
        {
            //Task done!
            //Filters
            ApplyFilter();
        }, (e) =>
        {

        });
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
        if (e.PropertyName == nameof(Setting.ShowDefaultPack))
        {
            if (!Config.ShowDefaultPack)
            {
                var def = AllAvailablePack.FirstOrDefault(i => i.Info.Author == "Icon-Pack-Provider" && i.Info.Name == "Dead-by-daylight-Default-icons");
                if (def is not null)
                {
                    var index = AllAvailablePack.IndexOf(def);
                    AllAvailablePack.RemoveAt(index);
                    OnPropertyChanged(nameof(FilteredList));
                }
            }
            else
            {
                Task.Run(async () =>
                {
                    if (AllAvailablePack is not null)
                    {
                        AllAvailablePack.Clear();
                    }
                    else
                    {
                        AllAvailablePack = new();
                    }
                    var packs = await Packs.GetPacks();
                    foreach (var pack in packs)
                    {
                        PackDisplay display = new(pack);
                        if (display is null)
                            continue;
                        //Check readme
                        await Packs.CheckPackReadme(pack);
                        //Check banner data
                        await Packs.CheckPackBanner(pack);
                        //Get banner or perks URL for pack showcasing/preview samples
                        await display.GatherPreview();
                        AllAvailablePack.Add(display);
                    }
                }).Await(() =>
                {
                    OnPropertyChanged(nameof(FilteredList));
                });
            }
            return;
        }

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
        //Messenger.Default.Unregister<FilterOptionChangedMessage, string>(this, MessageToken.FILTEROPTIONSCHANGETOKEN);
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
                    if (QueryResults is null)
                        QueryResults = new();
                    else
                        QueryResults.Clear();
                    //Pack name search
                    var allFoundName = AllAvailablePack
                    .Where(i => i.Info.Name.Contains(value))
                    .Select(i => i.Info.Name).Distinct();
                    var allFoundAuthor = AllAvailablePack
                    .Where(i => i.Info.Repository.Owner.Contains(value))
                    .Select(i => i.Info.Repository.Owner).Distinct();
                    QueryResults = new(allFoundName.Concat(allFoundAuthor));
                    
                    OnPropertyChanged(nameof(FilteredList));
                });
            }
        }
    }

    [ObservableProperty]
    ObservableCollection<string> queryResults;

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
                pack.Info.Repository.Owner.ToLower().Contains(SearchQuery.ToLower())).ToList();
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

    public Setting? Config => SettingManager.Instance;

    #region Commands
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
}
