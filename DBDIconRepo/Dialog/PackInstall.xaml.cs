using IconPack.Model;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace DBDIconRepo.Dialog;

/// <summary>
/// Interaction logic for PackInstall.xaml
/// </summary>
public partial class PackInstall : Window
{
    public PackInstall()
    {
        InitializeComponent();
    }

    public PackInstall(Pack? selected)
    {
        InitializeComponent();
        DataContext = new ViewModel.PackInstallViewModel(selected);
    }

    private void OpenAttatchedFlyout(object sender, RoutedEventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
    }

    private void ReplyInstall(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        this.Close();
    }

    private void ReplyCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        this.Close();
    }
}

public class ShowInfoOrNoInfo : DataTemplateSelector
{
    public DataTemplate HasInfo { get; set; }
    public DataTemplate NoInfo { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Model.IPackSelectionItem selectable)
        {
            if (selectable.Info is null)
                return NoInfo;
            else if (string.IsNullOrEmpty(selectable.Info.Name))
                return NoInfo;
            else if (selectable.Info.Name.Contains('_'))
                return NoInfo;
            return HasInfo;
        }
        return base.SelectTemplate(item, container);
    }
}
