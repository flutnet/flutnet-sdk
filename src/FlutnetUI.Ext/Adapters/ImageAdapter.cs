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