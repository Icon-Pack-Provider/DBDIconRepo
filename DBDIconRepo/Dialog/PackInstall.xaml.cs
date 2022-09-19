using ModernWpf.Controls.Primitives;
using System.Windows;

namespace DBDIconRepo.Dialog
{
    /// <summary>
    /// Interaction logic for PackInstall.xaml
    /// </summary>
    public partial class PackInstall : Window
    {
        public PackInstall()
        {
            InitializeComponent();
        }

        public PackInstall(IconPack.Model.Pack? selected)
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
}
