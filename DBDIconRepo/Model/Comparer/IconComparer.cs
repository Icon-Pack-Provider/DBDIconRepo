using IconInfo.Internal;
using System.Collections.Generic;

namespace DBDIconRepo.Model.Comparer;

public class IconComparer : IComparer<IPackSelectionItem>
{
    public int Compare(IPackSelectionItem? x, IPackSelectionItem? y)
    {
        //Sort by path string
        if (x.Info is null && y.Info is null)
            return x.FullPath.CompareTo(y.FullPath);

        //Then by type name
        if (x.Info.GetType().Name != y.Info.GetType().Name)
            return x.Info.GetType().Name.CompareTo(y.Info.GetType().Name);

        if (x.Info is IBasic xbs && y.Info is IBasic ybs)
        {
            IBasicComparer basicCompare = new();
            return basicCompare.Compare(xbs, ybs);            
        }
        return 0;
    }
}