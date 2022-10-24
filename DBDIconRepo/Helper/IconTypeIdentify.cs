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
        var icon = Info.GetIcon(path);
        if (icon is not null)
            return icon;
        return UnknownIcon(path);
    }

    public static BasePreview FromBasicInfo(string path, PackRepositoryInfo repo)
    {
        try
        {
            IBasic? basic = FromPath(path);
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
            Info = UnknownIcon(path)
        };
    }

    public static IBasic UnknownIcon(string path)
    {
        FileInfo fileInfo = new(path);
        string name = fileInfo.NameOnly();
        string start = path[..path.IndexOf('/')];
        return start switch
        {
            Terms.Portrait => new Portrait() { File = name, Name = name },
            Terms.DailyRitual => new DailyRitual() { File = name, Name = name },
            Terms.Emblem => new Emblem() { File = name, Name = name },
            Terms.Addon => new Addon() { File = name, Name = name },
            Terms.Offering => new Offering() { File = name, Name = name },
            Terms.Item => new Item() { File = name, Name = name },
            Terms.Power => new Power() { File = name, Name = name },
            Terms.Perk => new Perk() { File = name, Name = name },
            Terms.StatusEffect => new StatusEffect() { File = name, Name = name },
            _ => new UnknownIcon(path),
        };
    }
}
