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

using System.Reactive;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FlutnetUI.Ext;
using FlutnetUI.Utilities;
using FlutnetUI.ViewModels;
using ReactiveUI;
using Splat;

namespace FlutnetUI.Views
{
    public class NewProjectView : ReactiveUserControl<NewProjectViewModel>
    {
        public NewProjectView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                if (ViewModel == null)
                    return;

                ViewModel
                    .ShowSetupWizard
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        Window wizard = (Window) Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                        wizard.DataContext = new SetupWindowViewModel();
                        wizard.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        await wizard.ShowDialog(window);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);

                ViewModel
                    .ShowError
                    .RegisterHandler(async interaction =>
                    {
                        Window window = Application.Current.GetMainWindow();
                        await MessageBox.Show(window, "An unexpected error occured, please retry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                        interaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}