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

namespace TheArtOfDev.HtmlRenderer.Adapters
{
    /// <summary>
    /// Adapter for platform specific image object - used to render images.
    /// </summary>
    public abstract class RImage : IDisposable
    {
        /// <summary>
        /// Get the width, in pixels, of the image.
        /// </summary>
        public abstract double Width { get; }

        /// <summary>
        /// Get the height, in pixels, of the image.
        /// </summary>
        public abstract double Height { get; }

        public abstract void Dispose();
    }
}