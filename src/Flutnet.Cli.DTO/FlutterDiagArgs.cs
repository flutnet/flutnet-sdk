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

using System.Collections.Generic;

namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for retrieving a diagnostic report about the current installation of Flutnet.
    /// </summary>
    public class FlutterDiagInArg : InArg
    {
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for retrieving a diagnostic report about the current installation of Flutnet.
    /// </summary>
    public class FlutterDiagOutArg : OutArg
    {
        public FlutterIssues Issues { get; set; }
        public string FlutterSdkLocation { get; set; }
        public string AndroidSdkLocation { get; set; }
        public string JavaSdkLocation { get; set; }

        public string InstalledVersion { get; set; }
        public FlutterCompatibility Compatibility { get; set; }
        public string NextSupportedVersion { get; set; }
        public string LatestSupportedVersion { get; set; }

        public Dictionary<string, string> DoctorErrors { get; set; }
        public Dictionary<string, string> DoctorWarnings { get; set; }

        public string CurrentShell { get; set; }
        public string CurrentShellConfigurationFile { get; set; }
    }
}