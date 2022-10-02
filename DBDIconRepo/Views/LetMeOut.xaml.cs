using DBDIconRepo.Helper;
using System.Windows.Controls;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for LetMeOut.xaml
/// </summary>
public partial class LetMeOut : Page
{
    public LetMeOut()
    {
        InitializeComponent();
        this.Loaded += LoadAsyncTask;
    }

    private void LoadAsyncTask(object sender, System.Windows.RoutedEventArgs e)
    {
        ViewModel.GetProfilePic().Await(() => { });
    }
}
