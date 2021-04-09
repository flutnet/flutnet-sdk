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