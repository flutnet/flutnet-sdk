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
using Avalonia.Media;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia pen objects.
    /// </summary>
    internal sealed class PenAdapter : RPen
    {
        /// <summary>
        /// The actual Avalonia brush instance.
        /// </summary>
        private readonly IBrush _brush;

        /// <summary>
        /// the width of the pen
        /// </summary>
        private double _width;

        /// <summary>
        /// the dash style of the pen
        /// </summary>
        private IDashStyle _dashStyle;

        /// <summary>
        /// Init.
        /// </summary>
        public PenAdapter(IBrush brush)
        {
            _brush = brush;
        }

        public override double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public override RDashStyle DashStyle
        {
            set { DashStyles.TryGetValue(value, out _dashStyle); }
        }

        private static readonly Dictionary<RDashStyle, IDashStyle> DashStyles = new Dictionary<RDashStyle, IDashStyle>
        {
            {RDashStyle.Solid, null },
            {RDashStyle.Dash, Avalonia.Media.DashStyle.Dash },
            {RDashStyle.DashDot, Avalonia.Media.DashStyle.DashDot },
            {RDashStyle.DashDotDot, Avalonia.Media.DashStyle.DashDotDot },
            {RDashStyle.Dot, Avalonia.Media.DashStyle.Dot }
        };

        /// <summary>
        /// Create the actual Avalonia pen instance.
        /// </summary>
        public Pen CreatePen()
        {
            var pen = new Pen(_brush, _width, _dashStyle);
            return pen;
        }
    }
}