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
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using FlutnetUI.Utilities;
using ReactiveUI;
using Splat;

namespace FlutnetUI.ViewModels
{
    public class SetupWelcomeViewModel : AdvancedWizardPageViewModel, IActivatableViewModel
    {
        public SetupWelcomeViewModel(IScreen screen = null) : base("setup_welcome", screen)
        {
            Title = "Welcome";

            Button1Visible = true;
            Button1Text = "Close";

            Button1Command = ReactiveCommand.CreateFromObservable(() =>
            {
                var closeWizard = (HostScreen as SetupWindowViewModel)?.CloseWizard;
                return closeWizard?.Handle(Unit.Default);
            });

            Button3Command = ReactiveCommand.CreateFromTask(async () =>
            {
                await HostScreen.Router.NavigateAndReset.Execute(new SetupCheckFlutterViewModel(HostScreen));
            }, canExecute: this.WhenAnyValue(page => page.Button3Enabled), outputScheduler: RxApp.MainThreadScheduler);

            // Update user preferences when value changes
            this.WhenAnyValue(t => t.AlwaysShowAtStartup).Skip(1).Subscribe(value =>
            {
                AppSettings.Default.Preferences.ShowSetupWizardAtStartup = value;
                AppSettings.Default.Preferences.Save();
            });

            // Load user preferences when the view is activated
            this.WhenActivated((Action<IDisposable> disposables) =>
            {
                AppSettings.Default.Preferences.Reload();
                AlwaysShowAtStartup = AppSettings.Default.Preferences.ShowSetupWizardAtStartup;
            });
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public string StyleSheet => Properties.Resources.Setup_Styles;

        public string Text => Properties.Resources.Setup_Welcome_Message;

        public bool AlwaysShowAtStartup
        {
            get => _alwaysShowAtStartup;
            set => this.RaiseAndSetIfChanged(ref _alwaysShowAtStartup, value);
        }
        bool _alwaysShowAtStartup = true;
    }
}