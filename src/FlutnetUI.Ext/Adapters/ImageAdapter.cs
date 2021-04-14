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

using Avalonia.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia image objects.
    /// </summary>
    internal sealed class ImageAdapter : RImage
    {
        public ImageAdapter(Bitmap image)
        {
            Image = image;
        }

        /// <summary>
        /// The underlying Avalonia image.
        /// </summary>
        public Bitmap Image { get; }

        public override double Width => Image.PixelSize.Width;

        public override double Height => Image.PixelSize.Height;

        public override void Dispose()
        {
            Image.Dispose();
        }
    }
}