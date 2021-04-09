using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OperatingSystem = FlutnetUI.Utilities.OperatingSystem;

namespace FlutnetUI.Utilities
{
    /// <summary>
    /// Class that holds all the relevant application settings.
    /// </summary>
    internal class AppSettings
    {
        public static readonly AppSettings Default = new AppSettings();

        const string UserPreferencesFilename = "ui.prefs";

#if DEBUG
        public AppSettings()
        {
            AppPath = AppDataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Preferences = new UserPreferences(Path.Combine(AppDataFolder, UserPreferencesFilename));
        }
#else
        public AppSettings()
        {
            //To get the location the assembly is executing from (not necessarily where the it normally resides on disk)
            //In case of shadow copies usage, for instance in NUnit tests, this will be in a temp directory.
            //string path = Assembly.GetExecutingAssembly().Location;

            //To get the location the assembly normally resides on disk or the install directory
            string path = Assembly.GetExecutingAssembly().CodeBase;
            if (path.StartsWith("file:///", StringComparison.InvariantCultureIgnoreCase))
                path = path.Substring(8);

            //once you have the path you get the directory with:
            AppPath = Path.GetDirectoryName(path);

            Assembly assembly = Assembly.GetExecutingAssembly();
            string company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Novagem Solutions";
            string product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Flutnet";
            if (OperatingSystem.IsMacOS())
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", company, product);
            }
            else
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company, product);
            }
            Preferences = new UserPreferences(Path.Combine(AppDataFolder, UserPreferencesFilename));
        }
#endif

        public string AppPath { get; set; }
        public string AppDataFolder { get; set; }

        public void Configure(IConfiguration configuration)
        {
            string path = configuration.GetValue("AppDataFolder", string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                AppDataFolder = path;
                Preferences = new UserPreferences(Path.Combine(AppDataFolder, UserPreferencesFilename));
            }
        }

        public UserPreferences Preferences { get; private set; }
    }

    /// <summary>
    /// Class that holds all the user preferences
    /// that are persisted on the file system.
    /// </summary>
    internal class UserPreferences
    {
        readonly string _path;

        public UserPreferences(string path)
        {
            _path = path;
        }

        [JsonConstructor]
        private UserPreferences()
        {

        }

        [Obfuscation(Exclude = true)]
        public bool CheckForUpdatesAtStartup { get; set; } = true;
        [Obfuscation(Exclude = true)]
        public string LastProjectLocation { get; set; }
        [Obfuscation(Exclude = true)]
        public bool ShowSetupWizardAtStartup { get; set; } = true;

        public void Reload()
        {
            UserPreferences data;
            if (File.Exists(_path))
            {
                byte[] bytes = File.ReadAllBytes(_path);
                string json = System.Text.Encoding.UTF8.GetString(bytes);
                data = JsonConvert.DeserializeObject<UserPreferences>(json);
            }
            else
            {
                data = new UserPreferences();
            }
            CheckForUpdatesAtStartup = data.CheckForUpdatesAtStartup;
            LastProjectLocation = data.LastProjectLocation;
            ShowSetupWizardAtStartup = data.ShowSetupWizardAtStartup;
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(_path, bytes);
        }
    }
}