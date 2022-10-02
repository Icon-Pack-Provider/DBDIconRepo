﻿using DBDIconRepo.Model;
using DBDIconRepo.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DBDIconRepo.Converters;

public class GitServiceToUsername : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OctokitService svc)
        {
            if (!svc.IsAnonymous)
                return SettingManager.Instance.GitUsername;
        }
        return "Anonymous";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
