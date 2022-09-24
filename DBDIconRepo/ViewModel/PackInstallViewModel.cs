using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DBDIconRepo.Model;
using IconPack.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DBDIconRepo.ViewModel;

public partial class PackInstallViewModel : ObservableObject
{
    Pack? _selected;
    public Pack? SelectedPack
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    ObservableCollection<IPackSelectionItem>? _selections;
    public ObservableCollection<IPackSelectionItem>? InstallableItems
    {
        get => _selections;
        set => SetProperty(ref _selections, value);
    }

    ObservableCollection<SelectionMenuItem> _menu;
    public ObservableCollection<SelectionMenuItem> Menu
    {
        get => _menu;
        set => SetProperty(ref _menu, value);
    }

    public ICommand? InstallPack { get; private set; }
    public ICommand? SelectAll { get; private set; }
    public ICommand? UnSelectAll { get; private set; }
    public ICommand? SelectSpecifics { get; private set; }
    public PackInstallViewModel() { }
    public PackInstallViewModel(Pack? selected)
    {
        SelectAll = new RelayCommand<RoutedEventArgs>(SelectAllAction);
        UnSelectAll = new RelayCommand<RoutedEventArgs>(UnSelectAllAction);

        SelectedPack = selected;
        InstallableItems = new ObservableCollection<IPackSelectionItem>(selected.ContentInfo.Files.Select(path => new PackSelectionFile(path)));
                    
        //Load selection menu helper
        Menu = new ObservableCollection<SelectionMenuItem>();
        //Load from Json somehow

    }

    private void SetAllItems(bool state)
    {
        foreach (var item in InstallableItems)
        {
            item.IsSelected = state;
        }
    }

    private void UnSelectAllAction(RoutedEventArgs? obj)
    {
        SetAllItems(false);
    }

    private void SelectAllAction(RoutedEventArgs? obj)
    {
        SetAllItems(true);
    }

    
}
