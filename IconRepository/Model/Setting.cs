#pragma warning disable CS8766 // STFU about nullable value
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        string gitToken;
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
                return (false, "");
            return (true, path);
        }

        public static Setting Load()
        {
            var existance = IsConfigExist();
            if (existance.exist == false)
                return new();
            using StreamReader reader = new(File.OpenRead(existance.path));
            string json = reader.ReadToEnd();
            if (string.IsNullOrEmpty(json))
                return new();
            if (json == "{}")
            {
                File.Delete(existance.path);
                return new();
            }

            return JsonSerializer.Deserialize<Setting>(json);
        }

        public static void Save(this Setting local)
        {
            var existance = IsConfigExist();
            if (existance.exist == false)
                return;
            using StreamWriter writer = new(File.OpenWrite(existance.path));
            writer.Write(JsonSerializer.Serialize(local));
        }
    }

    public interface IConfig { }
}