using DBDIconRepo.Model;
using IconPack.Model;
using System;
using PackInfo = IconPack.Helper.Info;

namespace DBDIconRepo.Helper
{
    public static class PackSelectionHelper
    {
        public static IBaseItemInfo? GetItemInfo(string path)
        {
            try
            {
                if (path.StartsWith("CharPortraits"))
                    return new GenericItemInfo(PackInfo.Portraits[GetPathWithoutExtension(path)]);
                //PackInfo.Emblems
                else if (path.StartsWith("DailyRituals"))
                    return new GenericItemInfo(PackInfo.DailyRituals[GetPathWithoutExtension(path)]);
                else if (path.StartsWith("Emblems"))
                    return new GenericItemInfo(PackInfo.Emblems[GetPathWithoutExtension(path)]);
                else if (path.StartsWith("Favors"))
                    return new GenericItemInfo(PackInfo.Offerings[GetPathWithoutExtension(path)]);
                else if (path.StartsWith("ItemAddons"))
                    return PackInfo.GetItemAddonsInfo(path);
                else if (path.StartsWith("Items"))
                    return new GenericItemInfo(PackInfo.Items[GetPathWithoutExtension(path)]);
                else if (path.StartsWith("Powers"))
                    return PackInfo.Powers[GetPathWithoutExtension(path)];
                else if (path.StartsWith("Perks"))
                    return PackInfo.Perks[GetPathWithoutExtension(path)];
                else if (path.StartsWith("StatusEffects"))
                    return new GenericItemInfo(PackInfo.StatusEffects[GetPathWithoutExtension(path)]);
                else
                    return null;
            }
            catch (Exception ex)
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
