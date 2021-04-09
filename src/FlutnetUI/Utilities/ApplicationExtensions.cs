using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace FlutnetUI.Utilities
{
    internal static class ApplicationExtensions
    {
        public static Window GetMainWindow(this Application application)
        {
            return (application.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}
