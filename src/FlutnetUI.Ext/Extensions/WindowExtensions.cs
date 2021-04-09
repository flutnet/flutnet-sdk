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