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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

namespace FlutnetUI.Ext.Extensions
{
    internal static class WindowExtensions
    {
        public static void SetStyle(this Window window, string styleUri)
        {
            if (string.IsNullOrEmpty(styleUri))
                return;

            try
            {
                StyleInclude style = new StyleInclude(new Uri(styleUri)) {Source = new Uri(styleUri)};
                window.Styles.Add(style);
            }
            catch
            {
                // ignored
            }
        }

        public static void SetIcon(this Window window, string iconUri)
        {
            if (string.IsNullOrEmpty(iconUri))
                return;

            try
            {
                IAssetLoader assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using (Bitmap bitmap = new Bitmap(assetLoader.Open(new Uri(iconUri))))
                {
                    window.Icon = new WindowIcon(bitmap);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}