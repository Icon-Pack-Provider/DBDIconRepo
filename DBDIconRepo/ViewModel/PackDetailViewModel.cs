using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Comparer;
using DBDIconRepo.Model.Preview;
using IconInfo.Icon;
using IconPack;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace DBDIconRepo.ViewModel;

public partial class PackDetailViewModel : ObservableObject
{
    Pack? _selected;
    public Pack? SelectedPack
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public PackDetailViewModel() { }
    public PackDetailViewModel(Pack? selected)
    {
        PrepareCommands();
        SelectedPack = selected;
        PrepareDisplayData();
    }

    public async void PrepareDisplayData()
    {
        //Banner
        var bannerExist = await Packs.IsPackBannerExist(SelectedPack);
        if (bannerExist)
        {
            BannerURL = await Packs.GetPackBannerURL(SelectedPack);
        }

        //Get any previewable icon
        if (SelectedPack.Overrides is not null && SelectedPack.Overrides.DisplayFiles is not null)
        {
            HeroIconURL = Packs.GetPackItemOnGit(SelectedPack, SelectedPack.Overrides.DisplayFiles[0]);
        }
        else
        {
            foreach (var item in SettingManager.Instance.PerkPreviewSelection)
            {
                if (SelectedPack.ContentInfo.Files.FirstOrDefault(icon => icon.ToLower().Contains(item.File.ToLower())) is string match)
                {
                    HeroIconURL = Packs.GetPackItemOnGit(SelectedPack, match);
                    break;
                }
            }
        }

        //Readme.md
        var readmeExist = await Packs.IsPackReadmeExist(SelectedPack);
        if (readmeExist)
        {
            var localReadme = await Packs.GetPackReadme(SelectedPack);
            MdXaml.Markdown translator = new();
            ReadmeMDContent = translator.Transform(localReadme);
        }

        List<IBasePreview> identifiedItems = new();
        SelectedPack.ContentInfo.Files
            .Select(file => IconTypeIdentify.FromBasicInfo(file, SelectedPack.Repository))
            .AsParallel()
            .ToList()
            .ForEach(i => identifiedItems.Add(i));

        //Filter identified items into its own observableCollection
        if (SelectedPack.ContentInfo.HasPerks)
        {
            await Task.Run(async () =>
            {
                var perks = identifiedItems.Where(i => i.Info is Perk)
                .OrderBy(i => i.Info as Perk, new PerkComparer());
                foreach (var perk in perks)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        PerksPreview.Add((PerkPreviewItem)perk);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
            SortingPerkList();
        }

        //Portrait icons
        if (SelectedPack.ContentInfo.HasPortraits)
        {
            await Task.Run(async () =>
            {
                var portraits = identifiedItems.Where(i => i.Info is Portrait);
                foreach (var portrait in portraits)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        PortraitPreview.Add((PortraitPreviewItem)portrait);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //Powers
        if (SelectedPack.ContentInfo.HasPowers)
        {
            await Task.Run(async () =>
            {
                var powers = identifiedItems.Where(i => i.Info is Power);
                foreach (var power in powers)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        PowerPreview.Add((PowerPreviewItem)power);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //Items
        if (SelectedPack.ContentInfo.HasItems)
        {
            await Task.Run(async () =>
            {
                var items = identifiedItems.Where(i => i.Info is Item);
                foreach (var item in items)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ItemsPreview.Add((ItemPreviewItem)item);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //Status
        if (SelectedPack.ContentInfo.HasStatus)
        {
            await Task.Run(async () =>
            {
                var status = identifiedItems.Where(i => i.Info is StatusEffect);
                foreach (var st in status)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        StatusEffectsPreview.Add((StatusEffectPreviewItem)st);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //Offerings
        if (SelectedPack.ContentInfo.HasOfferings)
        {
            await Task.Run(async () =>
            {
                var offerings = identifiedItems.Where(i => i.Info is Offering);
                foreach (var offering in offerings)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (OfferingsPreview is null)
                            OfferingsPreview = new ObservableCollection<OfferingPreviewItem>();
                        OfferingsPreview.Add((OfferingPreviewItem)offering);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //Addons
        if (SelectedPack.ContentInfo.HasAddons)
        {
            await Task.Run(async () =>
            {
                var addons = identifiedItems.Where(i => i.Info is Addon);
                foreach (var addon in addons)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (AddonsPreview is null)
                            AddonsPreview = new ObservableCollection<AddonPreviewItem>();
                        AddonsPreview.Add((AddonPreviewItem)addon);
                    }, SettingManager.Instance.SacrificingAppResponsiveness ?
                    System.Windows.Threading.DispatcherPriority.Send :
                    System.Windows.Threading.DispatcherPriority.Background);
                }
            });
        }

        //TODO:When showing emblems sort it by name, then by type (none, silver, gold, iri etc.)
        IsPreparing = false;
    }

    [ObservableProperty]
    bool isPreparing = true;

    DetailFocusMode _dm = DetailFocusMode.Overview;
    public DetailFocusMode CurrentDisplayMode
    {
        get => _dm;
        set => SetProperty(ref _dm, value);
    }

    bool _hasReadme;
    public bool HasReadmeMD
    {
        get => _hasReadme;
        set => SetProperty(ref _hasReadme, value);
    }

    FlowDocument _readme = new();
    public FlowDocument ReadmeMDContent
    {
        get => _readme;
        set => SetProperty(ref _readme, value);
    }

    bool _hasBanner;
    public bool HasBanner
    {
        get => _hasBanner;
        set => SetProperty(ref _hasBanner, value);
    }

    string? _bannerURL;
    public string? BannerURL
    {
        get => _bannerURL;
        set => SetProperty(ref _bannerURL, value);
    }

    string? _heroIconURL = null;
    public string? HeroIconURL
    {
        get => _heroIconURL;
        set => SetProperty(ref _heroIconURL, value);
    }

    //Perks display
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortedPerks))]
    ObservableCollection<PerkPreviewItem>? perksPreview = new();

    public async Task SortPerks()
    {
        if (SortedPerks is null)
            SortedPerks = new();
        else
            SortedPerks.Clear();
        var perks = new List<PerkPreviewItem>(PerksPreview);
        switch (CurrentPerkSortingMethod)
        {
            case PerkSortBy.Name:
                if (IsPerkSortByAscending)
                    perks = perks.OrderBy(p => p.Info.Name).ToList();
                else
                    perks = perks.OrderByDescending(p => p.Info.Name).ToList();
                break;
            case PerkSortBy.Owner:
                if (IsPerkSortByAscending)
                    perks = perks.OrderBy(p => ((Perk)p.Info).Owner).ToList();
                else
                    perks = perks.OrderByDescending(p => ((Perk)p.Info).Owner).ToList();
                break;
            case PerkSortBy.Random:
                perks = perks.Shuffle().ToList();
                break;
        }
        foreach (var perk in perks)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SortedPerks.Add(perk);
            });
        }
    }

    //Portraits display
    ObservableCollection<PortraitPreviewItem>? _portrait = new();
    public ObservableCollection<PortraitPreviewItem>? PortraitPreview
    {
        get => _portrait;
        set => SetProperty(ref _portrait, value);
    }

    //Powers display
    ObservableCollection<PowerPreviewItem>? _power = new();
    public ObservableCollection<PowerPreviewItem>? PowerPreview
    {
        get => _power;
        set => SetProperty(ref _power, value);
    }

    //Items display
    ObservableCollection<ItemPreviewItem>? _items = new();
    public ObservableCollection<ItemPreviewItem>? ItemsPreview
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    //Addons
    ObservableCollection<AddonPreviewItem>? _addons = new();
    public ObservableCollection<AddonPreviewItem>? AddonsPreview
    {
        get => _addons;
        set => SetProperty(ref _addons, value);
    }

    //Emblems
    ObservableCollection<EmblemPreviewItem>? _emblem = new();
    public ObservableCollection<EmblemPreviewItem>? EmblemPreview
    {
        get => _emblem;
        set => SetProperty(ref _emblem, value);
    }

    //Daily ritual
    ObservableCollection<DailyRitualPreviewItem>? _dailyRitual = new();
    public ObservableCollection<DailyRitualPreviewItem>? DailyRitualPreview
    {
        get => _dailyRitual;
        set => SetProperty(ref _dailyRitual, value);
    }

    //Offering
    ObservableCollection<OfferingPreviewItem>? _offerings = new();
    public ObservableCollection<OfferingPreviewItem>? OfferingsPreview
    {
        get => _offerings;
        set => SetProperty(ref _offerings, value);
    }

    //Status effects
    ObservableCollection<StatusEffectPreviewItem>? _statusEffects = new();
    public ObservableCollection<StatusEffectPreviewItem>? StatusEffectsPreview
    {
        get => _statusEffects;
        set => SetProperty(ref _statusEffects, value);
    }

    private ObservableCollection<PerkPreviewItem> _sorted = new();
    public ObservableCollection<PerkPreviewItem>? SortedPerks
    {
        get => _sorted;
        set => SetProperty(ref _sorted, value);
    }

    public void SortingPerkList()
    {
        CanSort = false;
        SortPerks().Await(() =>
        {
            CanSort = true;
        });
    }

    [ObservableProperty]
    bool canSort;

    PerkSortBy _perkSortBy = PerkSortBy.Name;
    public PerkSortBy CurrentPerkSortingMethod
    {
        get => _perkSortBy;
        set
        {
            if (SetProperty(ref _perkSortBy, value))
                SortingPerkList();
        }
    }

    bool _sortPerkAscending = true;
    public bool IsPerkSortByAscending
    {
        get => _sortPerkAscending;
        set
        {
            if (SetProperty(ref _sortPerkAscending, value))
                SortingPerkList();
        }
    }

    public ICommand? SetDisplayMode { get; private set; }
    public ICommand? SetPerkSortingMethod { get; private set; }
    public ICommand? SetPerkSortingAscendingMethod { get; private set; }
    public ICommand? OpenPackURL { get; private set; }
    public ICommand? OpenOwnerURL { get; private set; }
    private void PrepareCommands()
    {
        SetDisplayMode = new RelayCommand<string?>(SetDisplayModeAction);
        SetPerkSortingMethod = new RelayCommand<string?>(SetPerkSortingMethodAction);
        SetPerkSortingAscendingMethod = new RelayCommand<string?>(SetPerkSortingAscendingMethodAction);
        OpenPackURL = new RelayCommand<RoutedEventArgs>(OpenPackURLAction);
        OpenOwnerURL = new RelayCommand<RoutedEventArgs>(OpenOwnerURLAction);
    }

    private void OpenOwnerURLAction(RoutedEventArgs? obj)
    {
        URL.OpenURL($"https://www.github.com/{SelectedPack.Repository.Owner}");
    }

    private void OpenPackURLAction(RoutedEventArgs? obj)
    {
        URL.OpenURL(SelectedPack.URL);
    }

    private void SetPerkSortingAscendingMethodAction(string? str)
    {
        if (str is null)
            return;
        IsPerkSortByAscending = str == "true";
    }

    private void SetPerkSortingMethodAction(string? obj)
    {
        if (obj is null)
            return;
        CurrentPerkSortingMethod = Enum.Parse<PerkSortBy>(obj);
    }

    private void SetDisplayModeAction(string? obj)
    {
        if (string.IsNullOrEmpty(obj))
            return;
        CurrentDisplayMode = Enum.Parse<DetailFocusMode>(obj);
    }
}

public enum DetailFocusMode
{
    Overview,
    Readme,
    Perks,
    Portraits,
    Powers,
    Items,
    Addons,
    Status,
    Offerings
}

public enum PerkSortBy
{
    Name,
    Owner,
    Random
}
