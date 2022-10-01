using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Model;
using DBDIconRepo.Service;
using IconInfo.Icon;
using IconInfo.Internal;
using IconPack.Model;
using SelectionListing;
using SelectionListing.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using static System.Windows.Forms.AxHost;

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
        var selections = selected.ContentInfo.Files
            .Where(file => file.EndsWith(".png") && !file.StartsWith(".banner"))
            .Select(path => new PackSelectionFile(path))
            .OrderBy(i => i, new IconComparer());
        //Sort
        InstallableItems = new(selections);

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

public class IconComparer : IComparer<IPackSelectionItem>
{
    public int Compare(IPackSelectionItem? x, IPackSelectionItem? y)
    {
        //Sort by path string
        if (x.Info is null && y.Info is null)
            return x.FullPath.CompareTo(y.FullPath);

        //Then by type name
        if (x.Info.GetType().Name != y.Info.GetType().Name)
            return x.Info.GetType().Name.CompareTo(y.Info.GetType().Name);

        if (x.Info.GetType().Name == y.Info.GetType().Name)
        {
            //Then by indivitual type information
            switch (x.Info)
            {
                case Emblem xEmblem:
                    var yEmblem = y.Info as Emblem;
                    //Compare by category first
                    if (!Equals(xEmblem.Category, yEmblem.Category))
                    {
                        //Different category, sort by its
                        return xEmblem.Category.CompareTo(yEmblem.Category);
                    }
                    else
                        return xEmblem.Quality.CompareTo(yEmblem.Quality); //sort by quality
                case Addon xad:
                    var yad = y.Info as Addon;
                    //Sort by survivor addons
                    //Then by killer power name
                    //TODO:Then by rarity??
                    //Then by name for now
                    if (xad.Owner is null && yad.Owner is null) //Survivor addons
                    {
                        //Survivor item addons
                        //Sort by addon parent (item) first, then by name
                        if (Equals(xad.For, yad.For))
                            return xad.Name.CompareTo(yad.Name);
                        return xad.For.CompareTo(yad.For);
                    }                    
                    if (xad.Owner is not null && yad.Owner is not null) //Killer addons
                    {
                        //For now, sort by name only
                        if (Equals(xad.For, yad.For))
                            return xad.Name.CompareTo(yad.Name);
                        return xad.For.CompareTo(yad.For);
                    }
                    else if (xad.Owner is null || yad.Owner is null)
                        return xad.Owner is null ? 1 : -1; //If it doesn't have owner = survivor item addons
                    break;
                case Power xPow:
                    var yPow = y.Info as Power;
                    if (!Equals(xPow.Owner, xPow.Owner)) //Same owner
                        return xPow.Owner.CompareTo(yPow.Owner); //Sort by owner
                    return xPow.Name.CompareTo(yPow.Name); //Then sort by name
                case Perk xPerk:
                    var yPerk = y.Info as Perk;
                    //Sort by Folder (no folder stay at bottom)
                    //Then by owner name
                    if ((xPerk.Folder is not null && yPerk.Folder is not null) ||
                        (xPerk.Folder is null && yPerk.Folder is null)) //Either both of them null or not null
                    {
                        //Perk is within folder
                        if (!Equals(xPerk.Folder, yPerk.Folder)) //Different folder
                            return xPerk.Folder.CompareTo(yPerk.Folder); //Sort by folder
                        //Same folder
                        if (!Equals(xPerk.Owner, yPerk.Owner)) //Different owner
                            return xPerk.Owner.CompareTo(yPerk.Owner); //Sort by owner
                        return xPerk.Name.CompareTo(yPerk.Name); //Same owner, sort by name
                    }
                    else //One of them is null
                        return xPerk.Folder is null ? 1 : -1;
                case Portrait xPor:
                case Offering xoff:
                case Item ix:
                    string? xValue = (string?)x.Info.GetType().GetProperty("Folder").GetValue(x.Info);
                    string? yValue = (string?)y.Info.GetType().GetProperty("Folder").GetValue(y.Info);
                    if (xValue is not null && yValue is not null)
                    {
                        if (Equals(x.Info.GetType().GetProperty("Folder"), y.Info.GetType().GetProperty("Folder")))
                            return x.Info.Name.CompareTo(y.Info.Name);
                        return string.Compare(xValue, yValue);
                    }
                    else if (xValue is null && yValue is null)
                        return x.Info.Name.CompareTo(y.Info.Name);
                    else
                        return xValue is null ? 1 : -1;
                default: //Daily ritual & status effects sort only by name
                    if (x.Info is UnknownIcon || y.Info is UnknownIcon)
                        return 0;
                    return x.Info.Name.CompareTo(y.Info.Name);
            }
        }
        return 0;
    }
}