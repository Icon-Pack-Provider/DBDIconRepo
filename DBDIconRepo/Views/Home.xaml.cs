using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DBDIconRepo.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
        }

        private void StartupAction(object sender, RoutedEventArgs e)
        {
            homeSelection.IsSelected = true;
        }

        private void SwitchPage(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is null)
                return;
            switch ((args.SelectedItem as NavigationViewItemBase).Tag.ToString())
            {
                case "home":
                    contentFrame.Navigate(new MainWindow());
                    break;
            }
            ViewModel.CurrentPageName = (args.SelectedItem as NavigationViewItemBase).Content.ToString();
        }

    }
}
