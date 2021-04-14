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

namespace FlutnetUI.Models
{
    public class FlutnetAppSettings
    {
        public string AppName { get; set; }
        public string OrganizationId { get; set; }
        /// <summary>
        /// A proper Android application ID (https://developer.android.com/studio/build/application-id)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        public string AndroidAppId { get; set; }
        /// <summary>
        /// A proper iOS bundle ID (https://developer.apple.com/documentation/bundleresources/information_property_list/cfbundleidentifier)
        /// built upon the app name and organization identifier provided by the user.
        /// </summary>
        public string IosAppId { get; set; }
        /// <summary>
        /// Indicates whether a Xamarin.Android application should be created.
        /// </summary>
        public bool TargetAndroid { get; set; }
        /// <summary>
        /// Indicates whether a Xamarin.iOS application should be created.
        /// </summary>
        public bool TargetIos { get; set; }
    }
}
