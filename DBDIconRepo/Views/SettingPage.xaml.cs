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