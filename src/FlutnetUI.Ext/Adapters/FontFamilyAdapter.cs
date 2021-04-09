using TheArtOfDev.HtmlRenderer.Adapters;

namespace FlutnetUI.Ext.Adapters
{
    /// <summary>
    /// Adapter for Avalonia Font family objects.
    /// </summary>
    internal sealed class FontFamilyAdapter : RFontFamily
    {
        public FontFamilyAdapter(string fontFamily)
        {
            Name = fontFamily;
        }
        
        public override string Name { get; }
    }
}