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