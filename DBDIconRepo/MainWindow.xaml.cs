﻿using DBDIconRepo.ViewModel;
using ModernWpf.Controls.Primitives;
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

namespace DBDIconRepo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public HomeViewModel ViewModel { get; } = new HomeViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += LoadPacklist;
            this.Unloaded += UnregisterStuff;
            DataContext = ViewModel;

        }

        private void UnregisterStuff(object sender, RoutedEventArgs e)
        {
            ViewModel.UnregisterMessages();
        }

        private void LoadPacklist(object sender, RoutedEventArgs e)
        {
            ViewModel.InitializeViewModel();
        }

        private void OpenAttatchedFlyout(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }

    public class IconPreviewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IconDisplay { get; set; }
        public DataTemplate BannerDisplay { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Model.IconDisplay)
                return IconDisplay;
            else
                return BannerDisplay;
        }
    }
}
