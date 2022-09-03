using IconPack.Internal;
using IconPack.Model.Icon;

namespace IconPack.Helper
{
    public static class Info
    {
        public static Dictionary<string, Portrait> GetPortraits() => CSV.GetPortraits();

        private static Lazy<Dictionary<string, Portrait>> _portraits;
        public static Dictionary<string, Portrait> Portraits
        {
            get
            {
                if (_portraits is null)
                    _portraits = new(CSV.GetPortraits());
                return _portraits.Value;
            }
        }

        public static Dictionary<string, DailyRitual> GetDailyRituals() => CSV.GetDailyRituals();
        private static Lazy<Dictionary<string, DailyRitual>> _dailyRituals;
        public static Dictionary<string, DailyRitual> DailyRituals
        {
            get
            {
                if (_dailyRituals is null)
                    _dailyRituals = new(CSV.GetDailyRituals());
                return _dailyRituals.Value;
            }
        }

        public static Dictionary<string, Emblem> GetEmblems() => CSV.GetEmblems();
        private static Lazy<Dictionary<string, Emblem>> _emblems;
        public static Dictionary<string, Emblem> Emblems
        {
            get
            {
                if (_emblems is null)
                    _emblems = new(CSV.GetEmblems());
                return _emblems.Value;
            }
        }

        public static Dictionary<string, StatusEffect> GetStatusEffects() => CSV.GetStatusEffects();
        private static Lazy<Dictionary<string, StatusEffect>> _statusEffects;
        public static Dictionary<string, StatusEffect> StatusEffects
        {
            get
            {
                if (_statusEffects is null)
                    _statusEffects = new(CSV.GetStatusEffects());
                return _statusEffects.Value;
            }
        }

        public static Dictionary<string, Offering> GetOfferings() => CSV.GetOfferings();
        private static Lazy<Dictionary<string, Offering>> _offerings;
        public static Dictionary<string, Offering> Offerings
        {
            get
            {
                if (_offerings is null)
                    _offerings = new(CSV.GetOfferings());
                return _offerings.Value;
            }
        }

        public static Dictionary<string, Item> GetItems() => CSV.GetItems();
        private static Lazy<Dictionary<string, Item>> _items;
        public static Dictionary<string, Item> Items
        {
            get
            {
                if (_items is null)
                    _items = new(CSV.GetItems());
                return _items.Value;
            }
        }

        public static Dictionary<string, Power> GetPowers() => CSV.GetPowersFromCSV();
        private static Lazy<Dictionary<string, Power>> _powers;
        public static Dictionary<string, Power> Powers
        {
            get
            {
                if (_powers is null)
                    _powers = new(CSV.GetPowersFromCSV());
                return _powers.Value;
            }
        }

        public static Dictionary<string, Addon> GetAddons() => CSV.GetAddonsFromCSV();
        private static Lazy<Dictionary<string, Addon>> _addons;
        public static Dictionary<string, Addon> Addons
        {
            get
            {
                if (_addons is null)
                    _addons = new(CSV.GetAddonsFromCSV());
                return _addons.Value;
            }
        }

        public static Dictionary<string, Perk> GetPerks() => CSV.GetPerksFromCSV();
        private static Lazy<Dictionary<string, Perk>> _perks;
        public static Dictionary<string, Perk> Perks
        {
            get
            {
                if (_perks is null)
                    _perks = new(CSV.GetPerksFromCSV());
                return _perks.Value;
            }
        }
    }
}
