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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IconRepository
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SwitchPage(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            string? tag = string.Empty;
            if (args is null)
            {
                //Set tag to startup page
                tag = "AllPack";
            }
            else
            {
                if (args.IsSettingsInvoked)
                {
                    mainContentDisplay.Navigate(new View.Settings());
                    return;
                }
                tag = args.InvokedItemContainer.Tag.ToString();
            }
            switch (tag)
            {
                case "AllPack":
                    mainContentDisplay.Navigate(new View.AllPack());
                    break;
            }
        }

        private async void StartupPageSelect(object sender, RoutedEventArgs e)
        {
            //TODO:Read from setting to load selected page
            //For now
            mainNavigation.SelectedItem = allPacks;
            SwitchPage(null, null);
        }
    }
}
