using IconInfo.Internal;
using IconInfo.Strings;
using System.IO;
using IconInfo;
using DBDIconRepo.Model.Preview;
using IconPack.Model;
using IconInfo.Icon;
using DBDIconRepo.Model;

namespace DBDIconRepo.Helper;

public static class IconTypeIdentify
{
    public static IBasic? FromPath(string path)
    {
        FileInfo fileInfo = new(path);
        if (path.StartsWith(Terms.Portrait))
            return Info.Portraits[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.DailyRitual))
            return Info.DailyRituals[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.Emblem))
            return Info.Emblems[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.Offering))
            return Info.Offerings[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.Addon))
        {
            if (fileInfo.Directory.Name != Terms.Addon)
                return Info.GetAddon(fileInfo.NameOnly(), fileInfo.Directory.Name);
            return Info.GetAddon(fileInfo.NameOnly());
        }
        else if (path.StartsWith(Terms.Item))
            return Info.Items[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.Power))
            return Info.Powers[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.Perk))
            return Info.Perks[fileInfo.NameOnly()];
        else if (path.StartsWith(Terms.StatusEffect))
            return Info.StatusEffects[fileInfo.NameOnly()];
        else
            return new UnknownIcon()
            {
                File = fileInfo.NameOnly(),
                Name = fileInfo.NameOnly()
            };
    }

    public static BasePreview FromBasicInfo(string path, PackRepositoryInfo repo)
    {
        IBasic? basic = null;
        try
        {
            basic = FromPath(path);
            switch (basic)
            {
                case Addon a:
                    return new AddonPreviewItem(path, repo)
                    {
                        Info = a
                    };
                case DailyRitual dr:
                    return new DailyRitualPreviewItem(path, repo)
                    {
                        Info = dr
                    };
                case Emblem e:
                    return new EmblemPreviewItem(path, repo)
                    {
                        Info = e
                    };
                case Item i:
                    return new ItemPreviewItem(path, repo)
                    {
                        Info = i
                    };
                case Offering o:
                    return new OfferingPreviewItem(path, repo)
                    {
                        Info = o
                    };
                case Perk perk:
                    return new PerkPreviewItem(path, repo)
                    {
                        Info = perk
                    };
                case Portrait portrait:
                    return new PortraitPreviewItem(path, repo)
                    {
                        Info = portrait
                    };
                case Power pow:
                    return new PowerPreviewItem(path, repo)
                    {
                        Info = pow
                    };
                case StatusEffect se:
                    return new StatusEffectPreviewItem(path, repo)
                    {
                        Info = se
                    };
            }
        }
        catch
        {
            //Unknown icon, or no info
        }
        return new BasePreview(path, repo)
        {
            Info = new UnknownIcon(path)
        };
    }

    public static IBasic UnknownIcon(string path)
    {
        if (path.StartsWith(Terms.Portrait))
            return new Portrait() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.DailyRitual))
            return new DailyRitual() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Emblem))
            return new Emblem() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Offering))
            return new Offering() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Addon))
            return new Addon() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Item))
            return new Item() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Power))
            return new Power() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.Perk))
            return new Perk() { File = path.NameOnly(), Name = path.NameOnly() };
        else if (path.StartsWith(Terms.StatusEffect))
            return new StatusEffect() { File = path.NameOnly(), Name = path.NameOnly() };
        else
            return new UnknownIcon(path);
    }
}
