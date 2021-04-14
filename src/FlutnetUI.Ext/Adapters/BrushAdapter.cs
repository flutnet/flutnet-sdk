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

using Avalonia.Media;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia brush objects.
    /// </summary>
    internal sealed class BrushAdapter : RBrush
    {
        /// <summary>
        /// The actual Avalonia brush instance.
        /// </summary>
        private readonly IBrush _brush;

        /// <summary>
        /// Init.
        /// </summary>
        public BrushAdapter(IBrush brush)
        {
            _brush = brush;
        }

        /// <summary>
        /// The actual Avalonia brush instance.
        /// </summary>
        public IBrush Brush
        {
            get { return _brush; }
        }

        public override void Dispose()
        { }
    }
}