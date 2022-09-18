#pragma warning disable CS8766 // STFU about nullable value
using CommunityToolkit.Mvvm.ComponentModel;
using IconRepository.Attribute;
using IconRepository.ViewModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static IconRepository.String.Terms;

namespace IconRepository.Model
{
    public partial class Setting : ObservableObject, IConfig
    {
        private DebounceThrottle.DebounceDispatcher _debouncer;
        public Setting()
        {
            _debouncer = new(250);

            PropertyChanged += SaveSetting;
        }

        private void SaveSetting(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _debouncer.Debounce(() =>
            {
                this.Save();
            });
        }

        [ObservableProperty]
        [Configurable]
        string? gitToken;

        #region Search and Filters
        [ObservableProperty]
        [Configurable]
        bool useInstantSearch = true;

        [ObservableProperty]
        [Configurable]
        int instantSearchDelayMS = 250;

        [ObservableProperty]
        [Configurable]
        SortBy sortOption = SortBy.Name;

        [ObservableProperty]
        [Configurable]
        bool sortByAscending = true;

        #endregion
    }

    public static class SettingHelper
    {
        static (bool exist, string path) IsConfigExist()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConfigFolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Join(path, ConfigFile);
            if (!File.Exists(path))
                return (false, path);
            return (true, path);
        }

        public static Setting FromText(string input)
        {
            var reads = input.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var dict = new Dictionary<string, string>();
            foreach (var line in reads)
            {
                var info = line.Split('=');
                if (info.Length < 2)
                    continue;
                dict.Add(info[0], info[1]);
            }
            
            Setting converse = new();
            var fields = converse.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
            foreach (var item in fields)
            {
                bool isConfigurable = item.GetCustomAttribute(typeof(ConfigurableAttribute)) is not null;
                if (!isConfigurable)
                    continue;
                if (!dict.ContainsKey(item.Name))
                    continue;
                switch (item.FieldType.Name)
                {
                    case nameof(String):
                        item.SetValue(converse, dict[item.Name]);
                        continue;
                    case nameof(Boolean):
                        item.SetValue(converse, dict[item.Name] == "true");
                        continue;
                    case nameof(Int32):
                        item.SetValue(converse, int.Parse(dict[item.Name]));
                        continue;
                    case nameof(SortBy):
                        item.SetValue(converse, Enum.Parse<SortBy>(dict[item.Name]));
                        continue;
                    default:
                        throw new NotImplementedException($"The field {item.Name} with type {item.FieldType.Name} isn't support yet");
                }
            }
            return converse;
        }

        public static string ToText(this Setting source)
        {
            StringBuilder bd = new();
            var fields = source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var item in fields)
            {
                bool isConfigurable = item.GetCustomAttribute(typeof(ConfigurableAttribute)) is not null;
                if (!isConfigurable)
                    continue;
                bd.Append($"{item.Name}={item.GetValue(source)}\r\n");
            }
            return bd.ToString().Trim('\r', '\n');
        }

        public static Setting Load()
        {
            var existance = IsConfigExist();
            if (existance.exist == false)
                return new();
            using StreamReader reader = new(File.OpenRead(existance.path));
            string data = reader.ReadToEnd();
            if (string.IsNullOrEmpty(data))
                return new();
            if (data == "{}")
            {
                File.Delete(existance.path);
                return new();
            }

            Setting read = new();
            try
            {
                read = FromText(data);
            }
            catch
            {

            }
            return read;
        }

        public static void Save(this Setting local)
        {
            var existance = IsConfigExist();
            using StreamWriter writer = new(existance.exist ? File.OpenWrite(existance.path) : File.Create(existance.path));
            writer.Write(local.ToText());
        }
    }

    public interface IConfig { }
}