using System;
using System.Reflection;
using Flutnet.Cli.Core.Utilities;
using FlutnetSite.WebApi.DTO;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class UpdateManager
    {
        public static CheckForUpdatesResponse CheckForUpdates(bool verbose = false)
        {
            try
            {
                CheckForUpdatesResponse response;

                AppUsageData data = AppSettings.Default.UsageData;
                data.Reload();
                if (data.LastCheckedForUpdates.GetValueOrDefault() < DateTime.Now.AddHours(-1))
                {
                    response = WebApiClient.CheckForUpdates(AppSettings.Default.SdkTable.Revision);

                    if (!string.IsNullOrEmpty(response.NewSdkTable) && SerDes.TryXmlStringToObject(response.NewSdkTable, out SdkTable table))
                    {
                        SerDes.ObjectToXmlFile(table, AppSettings.Default.SdkTableCurrentPath);
                        AppSettings.Default.SdkTable = table;
                    }

                    data.LastCheckedForUpdates = DateTime.Now;
                    data.UpToDate = response.UpToDate;
                    data.NewVersion = response.NewVersion;
                    data.DownloadUrl = response.DownloadUrl;
                    data.Save();
                }
                else
                {
                    // prevent new Flutnet installations to report 
                    // old server responses
                    if (!data.UpToDate && !string.IsNullOrEmpty(data.NewVersion))
                    {
                        string productVersion = Assembly.GetEntryAssembly().GetProductVersion();
                        int compare = VersionUtils.Compare(productVersion, data.NewVersion);
                        if (compare >= 0)
                        {
                            data.UpToDate = true;
                            data.Save();
                        }
                    }

                    response = new CheckForUpdatesResponse
                    {
                        UpToDate = data.UpToDate,
                        NewVersion = data.NewVersion,
                        DownloadUrl = data.DownloadUrl
                    };
                }
                return response;
            }
            catch (Exception ex)
            {
                Log.Ex(ex);
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}