using IconPack.Internal;
using IconPack.Model;

namespace IconPack.Helper
{
    public static class Info
    {
        public static Dictionary<string, Portrait> GetPortraits() => CSV.GetPortraits();

        private static Dictionary<string, Portrait> _portraits;
        public static Dictionary<string, Portrait> Portraits
        {
            get
            {
                if (_portraits is null)
                    _portraits = CSV.GetPortraits();
                return _portraits;
            }
        }

        public static Dictionary<string, DailyRitual> GetDailyRituals() => CSV.GetDailyRituals();
        private static Dictionary<string, DailyRitual> _dailyRituals;
        public static Dictionary<string, DailyRitual> DailyRituals
        {
            get
            {
                if (_dailyRituals is null)
                    _dailyRituals = CSV.GetDailyRituals();
                return _dailyRituals;
            }
        }

        public static Dictionary<string, Emblem> GetEmblems() => CSV.GetEmblems();
        private static Dictionary<string, Emblem> _emblems;
        public static Dictionary<string, Emblem> Emblems
        {
            get
            {
                if (_emblems is null)
                    _emblems = CSV.GetEmblems();
                return _emblems;
            }
        }

        public static Dictionary<string, StatusEffect> GetStatusEffects() => CSV.GetStatusEffects();
        private static Dictionary<string, StatusEffect> _statusEffects;
        public static Dictionary<string, StatusEffect> StatusEffects
        {
            get
            {
                if (_statusEffects is null)
                    _statusEffects = CSV.GetStatusEffects();
                return _statusEffects;
            }
        }

        public static Dictionary<string, Offering> GetOfferings() => CSV.GetOfferings();
        private static Dictionary<string, Offering> _offerings;
        public static Dictionary<string, Offering> Offerings
        {
            get
            {
                if (_offerings is null)
                    _offerings = CSV.GetOfferings();
                return _offerings;
            }
        }

        public static Dictionary<string, Item> GetItems() => CSV.GetItems();
        private static Dictionary<string, Item> _items;
        public static Dictionary<string, Item> Items
        {
            get
            {
                if (_items is null)
                    _items = CSV.GetItems();
                return _items;
            }
        }

        public static Dictionary<string, Power> GetPowers() => CSV.GetPowersFromCSV();
        private static Dictionary<string, Power> _powers;
        public static Dictionary<string, Power> Powers
        {
            get
            {
                if (_powers is null)
                    _powers = CSV.GetPowersFromCSV();
                return _powers;
            }
        }

        public static Dictionary<string, Addon> GetAddons() => CSV.GetAddonsFromCSV();
        private static Dictionary<string, Addon> _addons;
        public static Dictionary<string, Addon> Addons
        {
            get
            {
                if (_addons is null)
                    _addons = CSV.GetAddonsFromCSV();
                return _addons;
            }
        }

        public static Dictionary<string, Perk> GetPerks() => CSV.GetPerksFromCSV();
        private static Dictionary<string, Perk> _perks;
        public static Dictionary<string, Perk> Perks
        {
            get
            {
                if (_perks is null)
                    _perks = CSV.GetPerksFromCSV();
                return _perks;
            }
        }
    }
}
