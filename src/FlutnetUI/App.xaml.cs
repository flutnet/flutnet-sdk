using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlutnetUI.ViewModels;
using ReactiveUI;
using Splat;

namespace FlutnetUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            /*
            Window window = (Window) Locator.Current.GetService<IViewFor<MainWindowViewModel>>();
            window.DataContext = new MainWindowViewModel();
            */

            // NOTA: per usate la IViewFor la finestra (window) deve estendere ReactiveWindow<T>, dove T è il suo modello associato.
            Window window = (Window) Locator.Current.GetService<IViewFor<MainWindowViewModel>>();
            window.DataContext = new MainWindowViewModel();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = window;

            base.OnFrameworkInitializationCompleted();
        }
    }
}