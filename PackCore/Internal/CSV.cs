using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Globalization;
using IconPack.Model;

namespace IconPack.Internal
{
    internal static class CSV
    {
        private static CsvHelper.Configuration.CsvConfiguration GetConfig()
            => new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",", // Enforce ',' as delimiter
                PrepareHeaderForMatch = header => header.Header.ToLower(), // Ignore casing
                MissingFieldFound = null,
                Quote = ';',
                BadDataFound = null
            };

        private static Stream GetCSVStream(string key)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (!key.EndsWith(".csv"))
                key += ".csv";
            string name = $"IconPack.Resource.{key}";

            return assembly.GetManifestResourceStream(name);
            //System.Resources
            //if (!key.EndsWith(".csv"))
            //    key += ".csv";
            //var eProv = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
            //return eProv.GetFileInfo(key).CreateReadStream();
        }

        private const string PortraitCSVFile = "portrait.csv";
        private const string DailyRitualCSVFile = "dailyritual.csv";
        private const string StatusEffectCSVFile = "statuseffect.csv";
        private static Dictionary<string, T> GetGenerics<T>(string key) where T : IBasic
        {
            using (var csv = new CsvHelper.CsvReader(new StreamReader(GetCSVStream(key)), GetConfig()))
            {
                csv.Context.RegisterClassMap<GenericMapper<T>>();
                var records = csv.GetRecords<T>();
                return records.ToDictionary(i => i.File);
            }
        }

        public static Dictionary<string, T> GetGenericsWithFolder<T>(string key) where T : IBasic, IFolder
        {
            using (var csv = new CsvHelper.CsvReader(new StreamReader(GetCSVStream(key)), GetConfig()))
            {
                csv.Context.RegisterClassMap<GenericWithFolderMapper<T>>();
                var records = csv.GetRecords<T>();
                return records.ToDictionary(i => i.File);
            }
        }

        #region Generic File name > Name items
        public static Dictionary<string, Portrait> GetPortraits()
            => GetGenerics<Portrait>(PortraitCSVFile);

        public static Dictionary<string, DailyRitual> GetDailyRituals()
            => GetGenerics<DailyRitual>(DailyRitualCSVFile);

        public static Dictionary<string, Emblem> GetEmblems()
            => GetGenerics<Emblem>(StatusEffectCSVFile);

        public static Dictionary<string, StatusEffect> GetStatusEffects()
            => GetGenerics<StatusEffect>(StatusEffectCSVFile);
        #endregion

        private const string OfferingCSVFile = "offering.csv";
        public static Dictionary<string, Offering> GetOfferings()
            => GetGenericsWithFolder<Offering>(OfferingCSVFile);

        private const string ItemCSVFile = "item.csv";
        public static Dictionary<string, Item> GetItems()
            => GetGenericsWithFolder<Item>(ItemCSVFile);

        public static Dictionary<string, Power> GetPowersFromCSV()
        {
            using (var csv = new CsvHelper.CsvReader(new StreamReader(GetCSVStream("power.csv")), GetConfig()))
            {
                csv.Context.RegisterClassMap<PowerMapper>();
                var records = csv.GetRecords<Power>();
                return records.ToDictionary(i => i.File);
            }
        }

        public static Dictionary<string, Addon> GetAddonsFromCSV()
        {
            using (var csv = new CsvHelper.CsvReader(new StreamReader(GetCSVStream("addon.csv")), GetConfig()))
            {
                csv.Context.RegisterClassMap<AddonMapper>();
                var records = csv.GetRecords<Addon>();
                return records.ToDictionary(i => $"{i.Folder}/{i.File}");
            }
        }

        public static Dictionary<string, Perk> GetPerksFromCSV()
        {
            using (var csv = new CsvHelper.CsvReader(new StreamReader(GetCSVStream("perk.csv")), GetConfig()))
            {
                csv.Context.RegisterClassMap<PerkMapper>();
                var records = csv.GetRecords<Perk>();
                return records.ToDictionary(i => i.File);
            }
        }
    }
}
