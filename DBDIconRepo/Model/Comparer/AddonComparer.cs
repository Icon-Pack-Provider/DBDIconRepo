using IconInfo.Icon;
using System;
using System.Collections.Generic;

namespace DBDIconRepo.Model.Comparer;

public class AddonComparer : IComparer<Addon>
{
    public int Compare(Addon? x, Addon? y)
    {
        //Sort by survivor addons
        //Then by killer power name
        //TODO:Then by rarity??
        //Then by name for now
        if (x.Owner is null && y.Owner is null) //Survivor addons
        {
            //Survivor item addons
            //Sort by addon parent (item) first, then by name
            if (Equals(x.For, y.For))
                return x.Name.CompareTo(y.Name);
            return x.For.CompareTo(y.For);
        }
        if (x.Owner is not null && y.Owner is not null) //Killer addons
        {
            //For now, sort by name only
            if (Equals(x.For, y.For))
                return x.Name.CompareTo(y.Name);
            return x.For.CompareTo(y.For);
        }
        else if (x.Owner is null || y.Owner is null)
            return x.Owner is null ? 1 : -1; //If it doesn't have owner = survivor item addons
        return -1;
    }
}
