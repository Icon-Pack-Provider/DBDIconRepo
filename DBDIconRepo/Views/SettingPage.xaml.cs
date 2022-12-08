using System.Windows;
using System.Windows.Controls;

namespace DBDIconRepo.Views;

/// <summary>
/// Interaction logic for Setting.xaml
/// </summary>
public partial class SettingPage : Page
{
    public SettingPage()
    {
        InitializeComponent();
    }     

    private void ForcePickNewBackground(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is ListView list)
        {
            if (list.SelectedIndex == 0)
            {
                //Force select new background
                ViewModel.ChooseBackground();
            }
        }
    }

    private void SizeTriggers(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width > 930)
        {
            Grid.SetRow(aboutSection, 0);
            Grid.SetColumn(aboutSection, 1);
        }
        else
        {
            Grid.SetRow(aboutSection, 1);
            Grid.SetColumn(aboutSection, 0);
        }
    }
}

public class BackgroundOptionTemplator : DataTemplateSelector
{
    public DataTemplate CustomBackground { get; set; }
    public DataTemplate ExistingBackground { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is string content)
        {
            if (content == "Custom")
                return CustomBackground;
            return ExistingBackground;
        }
        return base.SelectTemplate(item, container);
    }
}