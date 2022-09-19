using DBDIconRepo.ViewModel;
using IconInfo.Icon;
using IconPack.Model;
using System.Windows;
using System.Windows.Controls;

namespace DBDIconRepo.Dialog;

/// <summary>
/// Interaction logic for PackDetail.xaml
/// </summary>
public partial class PackDetail : Window
{
    public PackDetail(Pack? selected) => Initialize(selected);

    private void Initialize(Pack? selected)
    {
        InitializeComponent();
        this.Loaded += WindowLoaded;
        this.Unloaded += WindowUnloaded;
        DataContext = null;
        DataContext = new PackDetailViewModel(selected);
    }

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        
    }

    private void WindowUnloaded(object sender, RoutedEventArgs e)
    {

    }

    private void DetermineYScroll(object sender, ScrollChangedEventArgs e)
    {
        if ((DataContext as PackDetailViewModel)?.CurrentDisplayMode != DetailFocusMode.Overview)
        {
            topDownloadButton.Visibility = Visibility.Visible;
            return;
        }
        topDownloadButton.Visibility =
            mainContentScroll.ContentVerticalOffset > packDetailPanel.RenderSize.Height ? 
            Visibility.Visible : Visibility.Collapsed;
        System.Diagnostics.Debug.WriteLine($"Item height: {packDetailPanel.RenderSize.Height}" +
            $"\r\nCurrent position? {mainContentScroll.ContentVerticalOffset} || {mainContentScroll.VerticalOffset}");
    }

    private void SetDetailFocusModeViaTab(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0) 
            if (e.AddedItems[0] is TabItem tab)
                if (tab.Tag is string str)
                {
                    if (topDownloadButton != null)
                        topDownloadButton.Visibility = str != "Overview" ? Visibility.Visible :
                            (mainContentScroll.ContentVerticalOffset > packDetailPanel.RenderSize.Height ?
                            Visibility.Visible : Visibility.Collapsed);
                    if ((DataContext as PackDetailViewModel)?.CurrentDisplayMode.ToString() == str)
                        return;
                    mainContentScroll.ScrollToVerticalOffset(0);
                    switch (str)
                    {
                        case "Overview":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Overview;
                            break;
                        case "Readme":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Readme;
                            break;
                        case "Perks":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Perks;
                            break;
                        case "Portraits":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Portraits;
                            break;
                        case "Powers":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Powers;
                            break;
                        case "Items":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Items;
                            break;
                        case "Addons":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Addons;
                            break;
                        case "Status":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Status;
                            break;
                        case "Offerings":
                            (DataContext as PackDetailViewModel).CurrentDisplayMode = DetailFocusMode.Offerings;
                            break;
                    }
                }
    }
}

public class AddonsPreviewTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SurvivorItemAddons { get; set; }
    public DataTemplate? KillerItemAddons { get; set; }
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is Model.Preview.AddonPreviewItem info)
        {
            return (info.Info is Addon ad) && (ad.Owner is null || string.IsNullOrEmpty(ad.Owner)) ? 
                SurvivorItemAddons : KillerItemAddons;
        }
        return base.SelectTemplate(item, container);
    }
}
