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