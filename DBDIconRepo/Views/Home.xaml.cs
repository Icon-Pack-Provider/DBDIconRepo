using DBDIconRepo.Model;
using ModernWpf.Controls;
using System.Windows;
using Messenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

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
            Messenger.Default.Register<Home, SwitchToOtherPageMessage, string>(this, MessageToken.RequestMainPageChage,
                SwitchPageHandler);
        }

        private void SwitchPageHandler(Home recipient, SwitchToOtherPageMessage message)
        {
            SwitchPage(message.Page);
        }

        private void StartupAction(object sender, RoutedEventArgs e)
        {
            homeSelection.IsSelected = true;
        }

        private void SwitchPage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is null)
                return;
            if (args.IsSettingsSelected)
            {
                SwitchPage("setting");
                return;
            }
            string page = (args.SelectedItem as NavigationViewItemBase).Tag.ToString();
            SwitchPage(page);
        }

        public void SwitchPage(string page)
        {
            switch (page)
            {
                case "home":
                    contentFrame.Navigate(new MainWindow());
                    ViewModel.CurrentPageName = "Home";
                    break;
                case "login":
                    contentFrame.Navigate(new PleaseLogin());
                    ViewModel.CurrentPageName = "Anonymous";
                    break;
                case "loggedIn":
                    contentFrame.Navigate(new LetMeOut());
                    ViewModel.CurrentPageName = SettingManager.Instance.GitUsername;
                    break;
                case "setting":
                    contentFrame.Navigate(new SettingPage());
                    ViewModel.CurrentPageName = "Settings";
                    break;
            }
        }
    }
}
