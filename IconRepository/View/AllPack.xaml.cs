using CommunityToolkit.Mvvm.DependencyInjection;
using IconRepository.ViewModel;
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

namespace IconRepository.View
{
    /// <summary>
    /// Interaction logic for AllPack.xaml
    /// </summary>
    public partial class AllPack : Page
    {

        public AllPack()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<AllPackViewModel>();
        }

        private void FindSubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            (DataContext as AllPackViewModel).StartSearchCommand.Execute(null);
        }
    }
}
