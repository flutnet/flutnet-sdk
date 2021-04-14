// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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