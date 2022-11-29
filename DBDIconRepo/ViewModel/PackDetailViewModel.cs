using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Comparer;
using DBDIconRepo.Model.Preview;
using IconInfo.Icon;
using IconPack;
using IconPack.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace DBDIconRepo.ViewModel;

public partial class PackDetailViewModel : ObservableObject
{
    [ObservableProperty]
    Pack? selectedPack;

    public PackDetailViewModel() { }
    public PackDetailViewModel(Pack? selected)
    {
        SelectedPack = selected;
        PrepareDisplayData();
    }

    public async void PrepareDisplayData()
    {
        List<IBasePreview> identifiedItems = new();
        SelectedPack.ContentInfo.Files
            .Select(file => IconTypeIdentify.FromBasicInfo(file, SelectedPack.Repository))
            .AsParallel()
            .ToList()
            .ForEach(i => identifiedItems.Add(i));

        //Banner
        var bannerTask = Task.Run(async () =>
        {
            var bannerExist = await Packs.IsPackBannerExist(SelectedPack);
            if (bannerExist)
            {
                BannerURL = await Packs.GetPackBannerURL(SelectedPack);
            }
        });        

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
        var readmeTask = Task.Run(async () =>
        {
            var readmeExist = await Packs.IsPackReadmeExist(SelectedPack);
            if (readmeExist)
            {
                var localReadme = await Packs.GetPackReadme(SelectedPack);
                MdXaml.Markdown translator = new();
                ReadmeMDContent = translator.Transform(localReadme);
            }
        });

        //Filter identified items into its own observableCollection
        var perkListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasPerks)
                return;
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
            SortingPerkList();
        });

        //Portrait icons
        var portraitListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasPortraits)
                return;
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

        //Powers
        var powerListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasPowers)
                return;
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

        //Items
        var itemListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasItems)
                return;
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

        //Status
        var statusListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasStatus)
                return;
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

        //Offerings
        var offeringListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasOfferings)
                return;
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

        //Addons
        var addonListingTask = Task.Run(async () =>
        {
            if (!SelectedPack.ContentInfo.HasAddons)
                return;
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

        await Task.WhenAll(bannerTask,
            readmeTask,
            perkListingTask,
            portraitListingTask,
            powerListingTask,
            itemListingTask,
            statusListingTask,
            offeringListingTask,
            addonListingTask);
        //TODO:When showing emblems sort it by name, then by type (none, silver, gold, iri etc.)
        IsPreparing = false;
    }

    [ObservableProperty]
    bool isPreparing = true;

    [ObservableProperty]
    DetailFocusMode currentDisplayMode = DetailFocusMode.Overview;

    [RelayCommand] private void GoToOverviewPage() => CurrentDisplayMode = DetailFocusMode.Overview;
    [RelayCommand] private void GoToReadmePage() => CurrentDisplayMode = DetailFocusMode.Readme;
    [RelayCommand] private void GoToPerksPage() => CurrentDisplayMode = DetailFocusMode.Perks;
    [RelayCommand] private void GoToPortraitsPage() => CurrentDisplayMode = DetailFocusMode.Portraits;
    [RelayCommand] private void GoToPowersPage() => CurrentDisplayMode = DetailFocusMode.Powers;
    [RelayCommand] private void GoToItemsPage() => CurrentDisplayMode = DetailFocusMode.Items;
    [RelayCommand] private void GoToAddonsPage() => CurrentDisplayMode = DetailFocusMode.Addons;
    [RelayCommand] private void GoToStatusPage() => CurrentDisplayMode = DetailFocusMode.Status;
    [RelayCommand] private void GoToOfferingsPage() => CurrentDisplayMode = DetailFocusMode.Offerings;

    [ObservableProperty]
    bool hasReadmeMD;

    [ObservableProperty]
    FlowDocument readmeMDContent = new();

    [ObservableProperty]
    bool hasBanner;

    [ObservableProperty]
    string? bannerURL;

    [ObservableProperty]
    string? heroIconURL = null;

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
    [ObservableProperty]
    ObservableCollection<PortraitPreviewItem>? portraitPreview = new();

    //Powers display
    [ObservableProperty]
    ObservableCollection<PowerPreviewItem>? powerPreview = new();

    //Items display
    [ObservableProperty]
    ObservableCollection<ItemPreviewItem>? itemsPreview = new();

    //Addons
    [ObservableProperty]
    ObservableCollection<AddonPreviewItem>? addonsPreview = new();

    //Emblems
    [ObservableProperty]
    ObservableCollection<EmblemPreviewItem>? emblemPreview = new();

    //Daily ritual
    [ObservableProperty]
    ObservableCollection<DailyRitualPreviewItem>? dailyRitualPreview = new();

    //Offering
    [ObservableProperty]
    ObservableCollection<OfferingPreviewItem>? offeringsPreview = new();

    //Status effects
    [ObservableProperty]
    ObservableCollection<StatusEffectPreviewItem>? statusEffectsPreview = new();

    [ObservableProperty]
    private ObservableCollection<PerkPreviewItem> sortedPerks = new();

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

    
    [ObservableProperty]
    PerkSortBy currentPerkSortingMethod = PerkSortBy.Name;

    [RelayCommand] private void SetPerkSortByName() => CurrentPerkSortingMethod = PerkSortBy.Name;
    [RelayCommand] private void SetPerkSortByOwner() => CurrentPerkSortingMethod = PerkSortBy.Owner;
    [RelayCommand] private void SetPerkSortByRandom() => CurrentPerkSortingMethod = PerkSortBy.Random;

    partial void OnCurrentPerkSortingMethodChanged(PerkSortBy value)
    {
        SortingPerkList();
    }

    [ObservableProperty]
    bool isPerkSortByAscending = true;

    [RelayCommand] private void SetPerkSortAscending() => IsPerkSortByAscending = true;
    [RelayCommand] private void SetPerkSortDescending() => IsPerkSortByAscending = false;

    partial void OnIsPerkSortByAscendingChanged(bool value)
    {
        SortingPerkList();
    }

    [RelayCommand]
    private void OpenOwnerURL()
    {
        URL.OpenURL($"https://www.github.com/{SelectedPack.Repository.Owner}");
    }

    [RelayCommand]
    private void OpenPackURL()
    {
        URL.OpenURL(SelectedPack.URL);
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
