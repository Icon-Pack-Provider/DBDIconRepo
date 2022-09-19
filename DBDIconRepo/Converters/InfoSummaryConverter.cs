using System;
using System.Globalization;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class InfoSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IconPack.Model.IBaseItemInfo)
        {
            if (value is IconPack.Model.AddOnsInfo info)
            {
                if (info.Owner is not null) //Killer addons
                    return $"{info.Owner} | {info.For} | {info.Name}";
                return $"{info.For} | {info.Name}";
            }
            else if (value is IconPack.Model.KillerPowerInfo power)
            {
                return $"{power.PowerOwner} | {power.Name}";
            }
            else if (value is IconPack.Model.PerkInfo perk)
            {
                if (perk.PerkOwner is null)
                    return perk.Name;
                return $"{perk.PerkOwner} | {perk.Name}";
            }
            else if (value is Model.GenericItemInfo generic)
                return generic.Name;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
