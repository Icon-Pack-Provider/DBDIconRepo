using IconInfo.Icon;
using System.Collections.Generic;

namespace DBDIconRepo.Model.Comparer;

public class EmblemComparer : IComparer<Emblem>
{
    public int Compare(Emblem? x, Emblem? y)
    {
        if (x is null || y is null)
            return -1;
        //Compare by category first
        if (!Equals(x.Category, y.Category))
        {
            //Different category, sort by its
            return x.Category.CompareTo(y.Category);
        }
        else //Same category
            return x.Quality.CompareTo(y.Quality); //then sort by quality
    }
}
