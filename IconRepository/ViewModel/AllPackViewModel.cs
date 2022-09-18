using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using IconPack.Model;
using IconPack.Model.Icon;
using IconRepository.Model;
using IconRepository.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IconRepository.ViewModel;

public partial class AllPackViewModel : ObservableObject
{
    Debouncer _debouncer;

    [ObservableProperty]
    public Service.Octokit git;

    [ObservableProperty]
    bool showSomething;

    [ObservableProperty]
    Setting? appConfig;

    public AllPackViewModel()
    {
        _debouncer = new();
        AppConfig = Ioc.Default.GetService<Setting>();
        Git = (Service.Octokit?)Ioc.Default.GetService<IGit>();
    }

    [ObservableProperty]
    ObservableCollection<PackViewModel> allPacks;

    public ObservableCollection<PackViewModel> FilteredPack
    {
        get
        {
            if (AllPacks is null)
            {
                CurrentPackDisplayState = DisplayState.NoPack;
                return new();
            }
            if (AllPacks.Count < 1)
            {
                CurrentPackDisplayState = DisplayState.NoPack;
                return new();
            }

            var pack = new ObservableCollection<PackViewModel>();
            if (IsSearchKeywordSearchable)
            {
                pack = new(AllPacks.Where(i => i.Name.Contains(SearchKeyword) || i.Authors.Any(i => i.Contains(SearchKeyword))));
            }
            else
            {
                pack = new(AllPacks);
            }

            CurrentPackDisplayState = pack.Count >= 1 ? DisplayState.Loaded : DisplayState.NoPack;

            if (IsSortAscending)
                return new(pack.OrderBy(i => OrderByCurrentParameter(i)));
            else
                return new(pack.OrderByDescending(i => OrderByCurrentParameter(i)));
        }
    }

    private object OrderByCurrentParameter(PackViewModel i)
    {
        switch (PackSortBy)
        {
            default:
            case SortBy.Name:
                return i.Name;
            case SortBy.Author:
                return i.Authors[0];
            case SortBy.LastUpdate:
                return i.LastUpdate;
        }
    }

    private SortBy? packSortBy = null;
    public SortBy PackSortBy
    {
        get
        {
            if (packSortBy is null)
            {
                packSortBy = AppConfig.SortOption;
            }
            return packSortBy.Value;
        }
        set
        {
            if (SetProperty(ref packSortBy, value))
            {
                AppConfig.SortOption = value;
                OnPropertyChanged(nameof(IsSortByName));
                OnPropertyChanged(nameof(IsSortByAuthor));
                OnPropertyChanged(nameof(IsSortByLastUpdate));
                OnPropertyChanged(nameof(FilteredPack));
            }
        }
    }

    public bool IsSortByName => PackSortBy == SortBy.Name;
    public bool IsSortByAuthor => PackSortBy == SortBy.Author;
    public bool IsSortByLastUpdate => PackSortBy == SortBy.LastUpdate;

    [RelayCommand]
    private void SetSortByName() => PackSortBy = SortBy.Name;
    [RelayCommand]
    private void SetSortByAuthor() => PackSortBy = SortBy.Author;
    [RelayCommand]
    private void SetSortByLastUpdate() => PackSortBy = SortBy.LastUpdate;


    private bool? isSortAscending = null;
    public bool IsSortAscending
    {
        get
        {
            if (isSortAscending is null)
                isSortAscending = AppConfig.SortByAscending;
            return isSortAscending.Value;
        }
        set
        {
            if (SetProperty(ref isSortAscending, value))
            {
                AppConfig.SortByAscending = value;
                OnPropertyChanged(nameof(IsSortByAscending));
                OnPropertyChanged(nameof(IsSortByDescending));
                OnPropertyChanged(nameof(FilteredPack));
            }
        }
    }

    public bool IsSortByAscending => IsSortAscending == true;
    public bool IsSortByDescending => IsSortAscending == false;

    [RelayCommand]
    private void SetSortByAscending() => IsSortAscending = true;

    [RelayCommand]
    private void SetSortByDescending() => IsSortAscending = false;

    string searchKeyword = string.Empty;
    public string SearchKeyword
    {
        get => searchKeyword;
        set
        {
            if (value.Length <= 1)
                return;
            if (SetProperty(ref searchKeyword, value))
            {
                if (!AppConfig.UseInstantSearch)
                    return;
                _debouncer.Debounce(AppConfig.InstantSearchDelayMS, () =>
                {
                    OnPropertyChanged(nameof(FilteredPack));
                });
            }
        }
    }

    public bool IsSearchKeywordSearchable => SearchKeyword is not null && SearchKeyword.Trim().Length > 2;

    [RelayCommand]
    private void StartSearch()
    {
        OnPropertyChanged(nameof(FilteredPack));
    }

    public async Task LoadPackList()
    {
        try
        {
            CurrentPackDisplayState = DisplayState.Loading;
            Task.Run(async () =>
            {
                AllPacks = new((await IconPack.Helper.PackHelper.GetPacks()));
            }).Await(() =>
            {
                OnPropertyChanged(nameof(FilteredPack));
                if (FilteredPack.Count < 1)
                    CurrentPackDisplayState = DisplayState.NoPack;
                else
                    CurrentPackDisplayState = DisplayState.Loaded;
            });
        }
        catch
        {
            IconPack.Helper.PackHelper.InitializeGitService(Git.Client);
            AllPacks = await IconPack.Helper.PackHelper.GetPacks();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPackDisplayLoading))]
    [NotifyPropertyChangedFor(nameof(IsPackDisplayHasNoPack))]
    [NotifyPropertyChangedFor(nameof(IsPackDisplayLoaded))]
    private DisplayState currentPackDisplayState = DisplayState.Loading;

    public Visibility IsPackDisplayLoading => CurrentPackDisplayState == DisplayState.Loading ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsPackDisplayHasNoPack => CurrentPackDisplayState == DisplayState.NoPack ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsPackDisplayLoaded => CurrentPackDisplayState == DisplayState.Loaded ? Visibility.Visible : Visibility.Collapsed;
}

public enum DisplayState
{
    Loading,
    NoPack,
    Loaded
}

public enum SortBy
{
    Name,
    Author,
    LastUpdate
}