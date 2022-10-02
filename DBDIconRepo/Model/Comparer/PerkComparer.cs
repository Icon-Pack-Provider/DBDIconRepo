using IconInfo.Icon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Model.Comparer;

public class PerkComparer : IComparer<Perk>
{
    public int Compare(Perk? x, Perk? y)
    {
        //Sort by Folder (no folder stay at bottom)
        //Then by owner name
        if (x.Folder is not null && y.Folder is not null ||
            x.Folder is null && y.Folder is null) //Either both of them null or not null
        {
            //Perk is within folder
            if (!Equals(x.Folder, y.Folder)) //Different folder
                return x.Folder.CompareTo(y.Folder); //Sort by folder
                                                             //Same folder
            if (!Equals(x.Owner, y.Owner)) //Different owner
                return x.Owner.CompareTo(y.Owner); //Sort by owner
            return x.Name.CompareTo(y.Name); //Same owner, sort by name
        }
        else //One of them is null
            return x.Folder is null ? 1 : -1;
    }
}
