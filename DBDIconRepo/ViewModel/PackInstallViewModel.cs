using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Helper;
using DBDIconRepo.Model;
using DBDIconRepo.Model.Comparer;
using DBDIconRepo.Service;
using IconPack.Model;
using SelectionListing;
using SelectionListing.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DBDIconRepo.ViewModel;

public partial class PackInstallViewModel : ObservableObject
{
    [ObservableProperty]
    Pack? selectedPack;

    [ObservableProperty]
    ObservableCollection<IPackSelectionItem>? installableItems;

    [ObservableProperty]
    ObservableCollection<SelectionMenuItem> menu;

    public PackInstallViewModel() { }
    public PackInstallViewModel(Pack? selected)
    {
        SelectedPack = selected;
        Lists.Initialize(OctokitService.Instance.GitHubClientInstance, SettingManager.Instance.CacheAndDisplayDirectory);
        //Load selection menu helper
        Menu = ListingService.Instance.Listing;
        if (Menu.Count < 1)
        {
            rejectedMenuItemGUID = Guid.NewGuid().ToString();
            Menu.Add(new()
            {
                Name = rejectedMenuItemGUID,
                DisplayName = "Loading...",
                Selections = new() { rejectedMenuItemGUID }
            });
            ListingService.Instance.PropertyChanged += WaitingForListLoaded;
        }
        LoadListOfInstallableItems().Await(() =>
        {
            PreparingInstallableItems = false;
        });
    }

    [ObservableProperty]
    bool preparingInstallableItems = true;
    [ObservableProperty]
    int prepareProgress = 0;
    [ObservableProperty]
    int prepareTotal = 1;
    public async Task LoadListOfInstallableItems()
    {
        var selections = SelectedPack.ContentInfo.Files
            .Where(file => file.EndsWith(".png") && !file.StartsWith(".banner"))
            .Select(path => new PackSelectionFile(path))
            .OrderBy(i => i.Info, new IBasicComparer())
            .ToList();
        PrepareProgress = 0;
        PrepareTotal = selections.Count;

        InstallableItems = new();
        foreach (var item in selections)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                InstallableItems.Add(item);
                PrepareProgress = InstallableItems.Count;
            }, SettingManager.Instance.SacrificingAppResponsiveness ?
            System.Windows.Threading.DispatcherPriority.Send :
            System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private string? rejectedMenuItemGUID = null;
    private void WaitingForListLoaded(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ListingService.Instance.Listing.Count < 1)
            return;

        Menu = new(ListingService.Instance.Listing);
        ListingService.Instance.PropertyChanged -= WaitingForListLoaded;
    }


    private void SetAllItems(bool state)
    {
        foreach (var item in InstallableItems)
        {
            item.IsSelected = state;
        }
    }

    [RelayCommand]
    private void UnSelectAll(RoutedEventArgs? obj)
    {
        SetAllItems(false);
    }

    [RelayCommand]
    private void SelectAll(RoutedEventArgs? obj)
    {
        SetAllItems(true);
    }

    [RelayCommand]
    private void SelectSpecific(ObservableCollection<string?> args)
    {
        if (args is null)
            return;
        if (args.Contains(rejectedMenuItemGUID))
            return;
        //Press Shift to de-select
        bool setTo = true;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        { setTo = false; }

        foreach (var item in args)
        {
            if (item is null)
                continue;
            var nameonly = Path.GetFileNameWithoutExtension(item);
            foreach (var installable in InstallableItems)
            {
                if (installable.Info.File == nameonly)
                {
                    installable.IsSelected = setTo;
                }
            }
        }
    }
}
