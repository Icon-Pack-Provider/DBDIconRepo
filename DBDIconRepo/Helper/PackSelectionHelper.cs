using DBDIconRepo.Model;
using IconPack.Helper;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DBDIconRepo.Helper
{
    public static class PackSelectionHelper
    {
        public static IBasic? GetItemInfo(string path)
        {
            try
            {
                string file = GetPathWithoutExtension(path);
                if (path.StartsWith("CharPortraits"))
                    return Info.Portraits[file];
                else if (path.StartsWith("DailyRituals"))
                    return Info.DailyRituals[file];
                else if (path.StartsWith("Emblems"))
                    return Info.Emblems[GetPathWithoutExtension(path)];
                else if (path.StartsWith("Favors"))
                    return Info.Offerings[GetPathWithoutExtension(path)];
                else if (path.StartsWith("ItemAddons"))
                    return Info.Addons[file];
                else if (path.StartsWith("Items"))
                    return Info.Items[GetPathWithoutExtension(path)];
                else if (path.StartsWith("Powers"))
                    return Info.Powers[GetPathWithoutExtension(path)];
                else if (path.StartsWith("Perks"))
                    return Info.Perks[GetPathWithoutExtension(path)];
                else if (path.StartsWith("StatusEffects"))
                    return Info.StatusEffects[GetPathWithoutExtension(path)];
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetPathWithoutExtension(string path)
        {
            //Take last part from / and remove .png
            ReadOnlySpan<char> split = path;
            int firstSplit = split.LastIndexOf('/') + 1;
            int lastSplit = split.LastIndexOf('.');
            int splitLength = lastSplit - firstSplit;
            return split.Slice(firstSplit, splitLength).ToString();
        }

        /*
        public static void Sort(ref ObservableCollection<IPackSelectionItem> collection)
        {
            //Sort root
            collection = new ObservableCollection<IPackSelectionItem>(collection.OrderBy(i => i.Name));

            //Sort childs
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i] is PackSelectionFolder)
                {
                    collection[i] = SortChild(collection[i] as PackSelectionFolder);
                }
            }
        }

        private static IPackSelectionItem? SortChild(PackSelectionFolder root)
        {
            //Go as deep as possible
            for (int i = 0; i < root.Childs.Count; i++)
            {
                if (root.Childs[i] is PackSelectionFolder)
                {
                    root.Childs[i] = SortChild(root.Childs[i] as PackSelectionFolder);
                }
            }

            if (root.Name == "Perks" || (root.Parent != null && root.Parent.Name == "Perks"))
            {
                //Custom perk sort order?
                root.Childs = new ObservableCollection<IPackSelectionItem>(
                    root.Childs.OrderBy(i => i is not PackSelectionFolder).
                    ThenBy(i => PerkInfoKillerThenSurvivor(i)).
                    ThenBy(i => PerkOwnerNameOrNothing(i)).
                    ThenBy(i => PerkInfoNameOrNothing(i)));

                return root;
            }

            //Then sort from there
            root.Childs = new ObservableCollection<IPackSelectionItem>(
                root.Childs.OrderBy(i => i is not PackSelectionFolder).
                ThenBy(i => i.Name));
            return root;
        }

        private static object PerkInfoKillerThenSurvivor(IPackSelectionItem i)
        {
            if (i is PackSelectionFolder)
                return false;

            if (i.Info is null)
                return false;
            
            if (i.Info is not PerkInfo)
                return false;

            if ((i.Info as PerkInfo).PerkOwner is null)
                return false;

            if (i.Info is PerkInfo perk)
                return !perk.PerkOwner.StartsWith("The");

            return false;
        }

        private static string? PerkOwnerNameOrNothing(IPackSelectionItem i)
        {
            if (i is PackSelectionFolder)
                return (i as PackSelectionFolder).Name;

            if (i.Info is null)
                return "";
            
            if (i.Info is not PerkInfo)
                return "";

            if ((i.Info as PerkInfo).PerkOwner is null)
                return "";

            if (i.Info is PerkInfo perk)
                return perk.PerkOwner;

            return "";
        }

        private static string? PerkInfoNameOrNothing(IPackSelectionItem i)
        {
            if (i is PackSelectionFolder)
                return "";

            if (i.Info is PerkInfo)
                return i.Info.Name;
            return null;
        }
        */
    }
}
