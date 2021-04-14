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

namespace Flutnet.Cli.DTO
{
    /// <summary>
    /// Class that holds all the input arguments of the CLI 'exec' command
    /// for retrieving information about the installed version of Flutter
    /// and its compatibility with the current version of Flutnet SDK.
    /// </summary>
    public class FlutterInfoInArg : InArg
    {
    }

    /// <summary>
    /// Class that holds the result details of the CLI 'exec' command
    /// for retrieving information about the installed version of Flutter
    /// and its compatibility with the current version of Flutnet SDK.
    /// </summary>
    public class FlutterInfoOutArg : OutArg
    {
        public string InstalledVersion { get; set; }
        public FlutterCompatibility Compatibility { get; set; }
        public string NextSupportedVersion { get; set; }
        public string LatestSupportedVersion { get; set; }
    }
}