using scdUnpack.Extensions;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace scdUnpack
{
    public class ScdUnpackSettings
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true,
        };
        public string ModsPath { get; set; }
        public string GamePath { get; set; }

        public string GetPathByExt(string ext)
        {
            switch (ext.ToLower())
            {
                case ".scd":
                    return GamePath;
                case ".sc2":
                    return ModsPath;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void SetPathByName(string path, string name)
        {
            var property = GetType().GetProperty(name);
            property?.SetValue(this, path);
        }

        public ScdUnpackSettings()
        {
            ModsPath = @"C:\SC2MODS";
            GamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Supreme Commander 2\gamedata";
        }
        public static string GetSettingsFile()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ApplicationException("Could not locate AssemblyProductAttribute");
            }
            var appFolder = Path.Combine(appDataFolder, appName);
            appFolder.EnsureDirectoryExists();
            var settingsFilePath = Path.Combine(appFolder, $"{typeof(ScdUnpackSettings).Name}.json");
            return settingsFilePath;
        }

        public static ScdUnpackSettings Load()
        {
            var settingsFile = GetSettingsFile();
            if (File.Exists(settingsFile))
            {
                var json = File.ReadAllText(settingsFile);
                var instance = JsonSerializer.Deserialize<ScdUnpackSettings>(json);
                Debug.Assert(instance != null);
                return instance;
            }
            return new ScdUnpackSettings();
        }
        public void Save()
        {
            var settingsFile = GetSettingsFile();
            var json = JsonSerializer.Serialize(this, jsonSerializerOptions);
            File.WriteAllText(settingsFile, json);
        }
    }
}