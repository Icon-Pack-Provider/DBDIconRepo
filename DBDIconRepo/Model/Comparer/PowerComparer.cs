using IconInfo.Icon;
using System.Collections.Generic;

namespace DBDIconRepo.Model.Comparer;

public class PowerComparer : IComparer<Power>
{
    public int Compare(Power? x, Power? y)
    {
        if (x == null || y == null)
            return 0;
        if (x.Owner is null || y.Owner is null)
            return x.Name.CompareTo(y.Name);
        if (!Equals(x.Owner, y.Owner)) //Different owner
            return x.Owner.CompareTo(y.Owner); //Sort by owner
        return x.Name.CompareTo(y.Name); //Then sort by name
    }
}
