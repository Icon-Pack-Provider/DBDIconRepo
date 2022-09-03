using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace IconRepository.Converter
{
    internal class IconBasedOnLink : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string link)
            {
                return new ModernWpf.Controls.BitmapIcon()
                {
                    UriSource = new Uri(link, UriKind.Absolute)
                };
            }
            else
                return new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Contact);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}