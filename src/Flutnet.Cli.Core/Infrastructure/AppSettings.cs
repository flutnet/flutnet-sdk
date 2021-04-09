using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Flutnet.Cli.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Flutnet.Cli.Core.Infrastructure
{
    /// <summary>
    /// Class that holds all the relevant application settings.
    /// </summary>
    internal class AppSettings
    {
        public static readonly AppSettings Default = new AppSettings();
        
        const string AppUsageDataFilename = "cli.prefs";
        const string SdkTableFilename = "sdk.table.xml";

        public const string DefaultBinPath_Windows = @"C:\Program Files\Novagem Solutions\Flutnet\bin";
        public const string DefaultBinPath_macOS = "/Applications/Flutnet.app/Contents/macOS";

#if DEBUG
        public AppSettings()
        {
            AppPath = AppDataFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            UsageData = new AppUsageData(Path.Combine(AppDataFolder, AppUsageDataFilename));

            string repoRootFolder = Path.GetFullPath(Path.Combine(AppPath, "../../../../.."));
            TemplatesFolder = Path.Combine(repoRootFolder, "assets", "templates");
            SdkTableDefaultPath = Path.Combine(repoRootFolder, "assets", SdkTableFilename);

            SdkTableCurrentPath = Path.Combine(AppDataFolder, SdkTableFilename);
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
            if (Utilities.OperatingSystem.IsMacOS())
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", company, product);
                TemplatesFolder = Path.GetFullPath(Path.Combine(AppPath, "..", "Resources", "Templates"));
                SdkTableDefaultPath = Path.GetFullPath(Path.Combine(AppPath, "..", "Resources", SdkTableFilename));
            }
            else
            {
                AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), company, product);
                TemplatesFolder = Path.GetFullPath(Path.Combine(AppPath, "..", "resources", "templates"));
                SdkTableDefaultPath = Path.GetFullPath(Path.Combine(AppPath, "..", "resources", SdkTableFilename));
            }
            UsageData = new AppUsageData(Path.Combine(AppDataFolder, AppUsageDataFilename));
            SdkTableCurrentPath = Path.Combine(AppDataFolder, SdkTableFilename);
        }
#endif

        public string AppPath { get; set; }
        public string AppDataFolder { get; set; }
        public string SdkTableDefaultPath { get; set; }
        public string SdkTableCurrentPath { get; set; }
        public string TemplatesFolder { get; set; }

        public void Configure(IConfiguration configuration)
        {
            string path = configuration.GetValue("AppDataFolder", string.Empty);
            if (!string.IsNullOrEmpty(path))
            {
                AppDataFolder = path;
                UsageData = new AppUsageData(Path.Combine(AppDataFolder, AppUsageDataFilename));
            }
            path = configuration.GetValue("TemplatesFolder", string.Empty);
            if (!string.IsNullOrEmpty(path))
                TemplatesFolder = path;
        }

        public AppUsageData UsageData { get; private set; }

        public SdkTable SdkTable
        {
            get
            {
                if (_sdkTable != null)
                    return _sdkTable;
                
                SdkTable sdkTableDefault = File.Exists(SdkTableDefaultPath)
                    ? SerDes.XmlFileToObject<SdkTable>(SdkTableDefaultPath)
                    : new SdkTable();

                SdkTable sdkTableCurrent = File.Exists(SdkTableCurrentPath)
                    ? SerDes.XmlFileToObject<SdkTable>(SdkTableCurrentPath)
                    : new SdkTable();

                try
                {
                    _sdkTable = VersionUtils.Compare(sdkTableCurrent.Revision, sdkTableDefault.Revision) >= 0
                        ? sdkTableCurrent
                        : sdkTableDefault;
                }
                catch
                {
                    _sdkTable = !string.IsNullOrEmpty(sdkTableCurrent.Revision)
                        ? sdkTableCurrent
                        : sdkTableDefault;
                }

                return _sdkTable;
            }
            set => _sdkTable = value;
        }
        SdkTable _sdkTable;
    }

    /// <summary>
    /// Class that holds all the application usage data
    /// that are persisted encrypted on the file system.
    /// </summary>
    internal class AppUsageData
    {
        readonly string _path;

        public AppUsageData(string path)
        {
            _path = path;
        }

        [JsonConstructor]
        private AppUsageData()
        {

        }

        [Obfuscation(Exclude = true)]
        public DateTime? LastCheckedForUpdates { get; set; }
        [Obfuscation(Exclude = true)]
        public bool UpToDate { get; set; } = true;
        [Obfuscation(Exclude = true)]
        public string NewVersion { get; set; }
        [Obfuscation(Exclude = true)]
        public string DownloadUrl { get; set; }
        [Obfuscation(Exclude = true)]
        public Dictionary<string, string> AppKeys { get; set; } = new Dictionary<string, string>();

        public void Reload()
        {
            AppUsageData data;
            if (File.Exists(_path))
            {
                byte[] bytes = File.ReadAllBytes(_path);
#if DEBUG
                byte[] plainBytes = bytes;
#else
                byte[] plainBytes = IntelliLock.Licensing.DataSignHelper.DecryptData(bytes);
#endif
                string json = System.Text.Encoding.UTF8.GetString(plainBytes);

                data = JsonConvert.DeserializeObject<AppUsageData>(json);
            }
            else
            {
                data = new AppUsageData();
            }
            LastCheckedForUpdates = data.LastCheckedForUpdates;
            UpToDate = data.UpToDate;
            NewVersion = data.NewVersion;
            DownloadUrl = data.DownloadUrl;
            AppKeys = data.AppKeys;
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(json);
#if DEBUG
            byte[] bytes = plainBytes;
#else
            byte[] bytes = IntelliLock.Licensing.DataSignHelper.EncryptData(plainBytes);
#endif
            File.WriteAllBytes(_path, bytes);
        }
    }
}