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
        string name = fileInfo.NameOnly();
        string start = path.Substring(0, path.IndexOf('/'));
        switch (start)
        {
            case Terms.Portrait: return Info.Portraits.ContainsKey(name) ? Info.Portraits[name] : UnknownIcon(path);
            case Terms.DailyRitual: return Info.DailyRituals.ContainsKey(name) ? Info.DailyRituals[name] : UnknownIcon(path);
            case Terms.Emblem: return Info.Emblems.ContainsKey(name) ? Info.Emblems[name] : UnknownIcon(path);
            case Terms.Offering: return Info.Offerings.ContainsKey(name) ? Info.Offerings[name] : UnknownIcon(path);
            case Terms.Item: return Info.Items.ContainsKey(name) ? Info.Items[name] : UnknownIcon(path);
            case Terms.Power: return Info.Powers.ContainsKey(name) ? Info.Powers[name] : UnknownIcon(path);
            case Terms.Perk: return Info.Perks.ContainsKey(name) ? Info.Perks[name] : UnknownIcon(path);
            case Terms.StatusEffect: return Info.StatusEffects.ContainsKey(name) ? Info.StatusEffects[name] : UnknownIcon(path);
            case Terms.Addon:
                if (fileInfo.Directory.Name != Terms.Addon)
                    return Info.GetAddon(name, fileInfo.Directory.Name);
                return Info.GetAddon(name);
            default:
                return UnknownIcon(path);
        }
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
        FileInfo fileInfo = new(path);
        string name = fileInfo.NameOnly();
        string start = path.Substring(0, path.IndexOf('/'));
        switch (start)
        {
            case Terms.Portrait: return new Portrait() { File = name, Name = name };
            case Terms.DailyRitual: return new DailyRitual() { File = name, Name = name };
            case Terms.Emblem: return new Emblem() { File = name, Name = name };
            case Terms.Addon: return new Addon() { File = name, Name = name };
            case Terms.Offering: return new Offering() { File = name, Name = name };
            case Terms.Item: return new Item() { File = name, Name = name };
            case Terms.Power: return new Power() { File = name, Name = name };
            case Terms.Perk: return new Perk() { File = name, Name = name };
            case Terms.StatusEffect: return new StatusEffect() { File = name, Name = name };
            default: return new UnknownIcon(path);
        }
    }
}
