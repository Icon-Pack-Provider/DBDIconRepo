using CommunityToolkit.Mvvm.DependencyInjection;
using IconPack.Helper;
using IconRepository.Dialog;
using IconRepository.Service;
using IconRepository.ViewModel;
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
        public AllPackViewModel? CodeViewModel => DataContext as AllPackViewModel;
        private IGit? Git;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<AllPackViewModel>();
            Git = Ioc.Default.GetService<IGit>();
        }

        private async void SwitchPage(NavigationView sender, NavigationViewItemInvokedEventArgs args)
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
                if (!string.IsNullOrEmpty($"{args.InvokedItemContainer.Tag}"))
                    tag = args.InvokedItemContainer.Tag.ToString();
            }
            switch (tag)
            {
                case "AllPack":
                    mainContentDisplay.Navigate(new View.AllPack());
                    break;
                case "Login":
                    GitLogin tokenGather = new();
                    var reply = await tokenGather.ShowAsync();
                    if (reply == ContentDialogResult.Primary)
                    {
                        string token = tokenGather.tokenInput.Text;
                        if (string.IsNullOrEmpty(token))
                            break;
                        CodeViewModel.AppConfig.GitToken = token;
                        //Try login
                        Git.ReLogin(token);
                        PackHelper.InitializeGitService(Git.Client);
                    }
                    break;
            }
        }

        private void StartupPageSelect(object sender, RoutedEventArgs e)
        {
            //TODO:Read from setting to load selected page
            //For now
            mainNavigation.SelectedItem = allPacks;
            SwitchPage(null, null);
        }
    }
}
