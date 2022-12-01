using IconInfo.Icon;
using IconInfo.Internal;
using System.Collections.Generic;

namespace DBDIconRepo.Model.Comparer;

public class IBasicComparer : IComparer<IBasic?>
{
    public int Compare(IBasic? x, IBasic? y)
    {
        if (x.GetType() != y.GetType())
            return x.GetType().Name.CompareTo(y.GetType().Name);
        switch (x)
        {
            case Emblem:
                EmblemComparer ec = new();
                return ec.Compare((Emblem?)x, (Emblem?)y);
            case Addon:
                AddonComparer ac = new();
                return ac.Compare((Addon?)x, (Addon?)y);
            case Power:
                PowerComparer poc = new();
                return poc.Compare((Power?)x, (Power?)y);
            case Perk:
                PerkComparer pec = new();
                return pec.Compare((Perk?)x, (Perk?)y);
            case IFolder:
                string? xValue = (string?)x.GetType().GetProperty("Folder").GetValue(x);
                string? yValue = (string?)y.GetType().GetProperty("Folder").GetValue(y);

                if (x is UnknownIcon xu && y is UnknownIcon yu)
                {
                    if (!Equals(xu.Folder, yu.Folder))
                        return xu.Folder.CompareTo(yu.Folder);
                    return xu.Name.CompareTo(yu.Name);
                }
                else if (x is UnknownIcon xOnly && y is not UnknownIcon)
                    return xOnly.Folder.CompareTo(y.GetType().Name);
                else if (x is not UnknownIcon && y is UnknownIcon yOnly)
                    return x.GetType().Name.CompareTo(yOnly.Folder);

                if (xValue is not null && yValue is not null)
                {
                    if (Equals(x.GetType().GetProperty("Folder"), y.GetType().GetProperty("Folder"))) //Same folder
                        return x.Name.CompareTo(y.Name); //Compare filename
                    return string.Compare(xValue, yValue); //Compare folder name
                }
                else if (xValue is null && yValue is null)
                    return x.Name.CompareTo(y.Name);
                else
                    return xValue is null ? 1 : -1;
            default:
                return x.Name.CompareTo(y.Name);
        }
    }
}
