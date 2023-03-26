using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DBDIconRepo.Model;

public interface IRateInfo
{
    int? RequestPerHour { get; set; }

    int? RequestRemain { get; set; }

    string? ResetIn { get; set; }

    void CheckRateLimit();
    void DestructivelyCheckRateLimit();
}
