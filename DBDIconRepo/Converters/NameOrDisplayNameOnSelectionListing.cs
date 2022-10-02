using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class NameOrDisplayNameOnSelectionListing : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SelectionListing.Model.SelectionMenuItem menu)
        {
            if (menu.DisplayName is not null)
                return menu.DisplayName;
            return menu.Name;
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
