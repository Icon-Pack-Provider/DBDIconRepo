using IconInfo.Icon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Model.Comparer;

public class PowerComparer : IComparer<Power>
{
    public int Compare(Power? x, Power? y)
    {
        if (!Equals(x.Owner, y.Owner)) //Same owner
            return x.Owner.CompareTo(y.Owner); //Sort by owner
        return x.Name.CompareTo(y.Name); //Then sort by name
    }
}
