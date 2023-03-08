using DBDIconRepo.Model.Uploadable;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for UploadPack.xaml
/// </summary>
public partial class UploadPack : Page
{
    public UploadPack()
    {
        InitializeComponent();
    }

    private void OpenAttachedFlyout(object sender, RoutedEventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
    }

    private void OpenAttachFlyoutOnHover(object sender, System.Windows.Input.MouseEventArgs e)
    {
        FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
    }
}

public class UploadItemTemplator : DataTemplateSelector
{
    public DataTemplate UploadableFolderTemplator { get; set; }
    public DataTemplate UploadableItemTemplator { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is UploadableFolder)
            return UploadableFolderTemplator;
        else
            return UploadableItemTemplator;
    }
}