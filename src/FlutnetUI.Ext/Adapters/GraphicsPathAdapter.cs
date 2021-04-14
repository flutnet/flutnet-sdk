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

using Avalonia;
using Avalonia.Media;
using TheArtOfDev.HtmlRenderer.Adapters;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia graphic path objects.
    /// </summary>
    internal sealed class GraphicsPathAdapter : RGraphicsPath
    {
        /// <summary>
        /// The actual Avalonia graphics geometry instance.
        /// </summary>
        private readonly StreamGeometry _geometry = new StreamGeometry();

        /// <summary>
        /// The context used in Avalonia geometry to render path
        /// </summary>
        private readonly StreamGeometryContext _geometryContext;

        public GraphicsPathAdapter()
        {
            _geometryContext = _geometry.Open();
        }

        public override void Start(double x, double y)
        {
            _geometryContext.BeginFigure(new Point(x, y), true);
        }

        public override void LineTo(double x, double y)
        {
            _geometryContext.LineTo(new Point(x, y));
        }

        public override void ArcTo(double x, double y, double size, Corner corner)
        {
            _geometryContext.ArcTo(new Point(x, y), new Size(size, size), 0, false, SweepDirection.Clockwise);
        }

        /// <summary>
        /// Close the geometry to so no more path adding is allowed and return the instance so it can be rendered.
        /// </summary>
        public StreamGeometry GetClosedGeometry()
        {
            _geometryContext.EndFigure(true);
            _geometryContext.Dispose();
            return _geometry;
        }

        public override void Dispose()
        { }
    }
}