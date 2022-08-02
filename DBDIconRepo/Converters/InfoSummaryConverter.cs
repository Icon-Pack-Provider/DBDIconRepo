using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DBDIconRepo.Converters
{
    public class InfoSummaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                default:
                    return string.Empty;
                case Addon addon:
                    if (addon.Owner is not null)
                        return $"{addon.Owner} | {addon.For} | {addon.Name}";
                    return $"{addon.For} | {addon.Name}";
                case Power power:
                    return $"{power.Owner} | {power.Name}";
                case Perk perk:
                    if (string.IsNullOrEmpty(perk.Owner))
                        return $"{perk.Name}";
                    return $"{perk.Owner} | {perk.Name}";
                case IBasic basic:
                    return $"{basic.Name}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
