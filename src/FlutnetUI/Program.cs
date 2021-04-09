using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using FlutnetUI.Utilities;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using Splat;

namespace FlutnetUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile(("FlutnetUI.appsettings.json"))
                .Build();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Ex((Exception) e.ExceptionObject);

            RxApp.DefaultExceptionHandler = new RxUnhandledExceptionObserver();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views.
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                // Campi aggiunti per test build su MAC
                .With(new X11PlatformOptions { UseGpu = false })
                .With(new AvaloniaNativePlatformOptions { UseGpu = false })
                .With(new MacOSPlatformOptions { ShowInDock = false })
                //.With(new Win32PlatformOptions { UseDeferredRendering = false }) Commentato altrimenti in windows non si vedono le immagini
                .LogToDebug()
                .UseReactiveUI();
        }
    }
}