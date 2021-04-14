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