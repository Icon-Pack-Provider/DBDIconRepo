using DBDIconRepo.Model.History;
using DBDIconRepo.Views;
using IconPack.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace DBDIconRepo.Converters;

internal class HistoryPackInfoFromRepositoryIDConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string req)
            return string.Empty;
        if (value is not IHistoryItem history)
            return string.Empty;
        if (Application.Current.MainWindow is not RootPages root)
            return string.Empty;
        if (root.contentFrame.Content is not History page)
            return string.Empty;
        if (page.ViewModel.AvailablePacks.FirstOrDefault(p => p.Repository.ID == history.Victim) is not Pack pack)
            return string.Empty;
        switch (req)
        {
            case "TypeIcon": //
                if (history.Action == HistoryType.Install)
                    return "\uE118";
                else if (history.Action == HistoryType.ViewDetail)
                    return "\uE18B";
                return string.Empty;
            case "PackName":
                return pack.Name;
            case "PackOwner":
                return pack.Author;
            case "InstalledIconsCount":
                if (history is not HistoryInstallPack ih)
                    return string.Empty;
                return $"Installed {ih.InstalledIcons.Count} icons";
        }
        //page.ViewModel.AvailablePacks
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
