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

namespace FlutnetSite.WebApi.DTO
{
    /// <summary>
    /// The object that is sent back by the REST API when errors occur
    /// while processing a request.
    /// </summary>
    public class ApiError
    {
        /// <summary>
        /// Any code that identifies the type of error.
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// A more complete detail of the error and the associated code.
        /// </summary>
        public string Message { get; set; }
    }

    public class CheckForUpdatesRequest
    {
        public string CurrentVersion { get; set; }
        public int OS { get; set; }
        public bool Is64BitOS { get; set; }
        public string SdkTableRevision { get; set; }
    }

    public class CheckForUpdatesResponse
    {
        public bool UpToDate { get; set; }
        public string NewVersion { get; set; }
        public string DownloadUrl { get; set; }
        public string NewSdkTable { get; set; }
    }
}
