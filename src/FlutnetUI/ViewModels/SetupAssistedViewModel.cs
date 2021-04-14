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

using Avalonia.Controls;
using Flutnet.Cli.DTO;
using FlutnetUI.Utilities;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace FlutnetUI.ViewModels
{
    public class SetupAssistedViewModel : AdvancedWizardPageViewModel
    {
        public SetupAssistedViewModel(FlutterDiagOutArg checkResult, IScreen screen = null) : base("setup_assisted", screen)
        {
            Title = "Step-by-step Configuration";

            Button2Visible = true;

            Button3Command = ReactiveCommand.CreateFromTask(async () =>
            {
                await HostScreen.Router.Navigate.Execute(new SetupAssistedProgressViewModel(new FlutnetSetupInArg
                {
                    FlutterSdkLocation = FlutterLocation,
                    AndroidSdkLocation = !SkipAndroidConfiguration ? AndroidLocation : string.Empty,
                    JavaSdkLocation = !SkipAndroidConfiguration ? JavaLocation : string.Empty
                }, HostScreen));
            });

            CurrentShell = checkResult.CurrentShell;
            CurrentShellConfigurationFile = checkResult.CurrentShellConfigurationFile;

            FlutterLocation = checkResult.FlutterSdkLocation;
            BrowseFlutterLocation = ReactiveCommand.CreateFromTask(BrowseFlutterLocationAsync);

            AndroidLocation = checkResult.AndroidSdkLocation;
            BrowseAndroidLocation = ReactiveCommand.CreateFromTask(BrowseAndroidLocationAsync);

            JavaLocation = checkResult.JavaSdkLocation;
            BrowseJavaLocation = ReactiveCommand.CreateFromTask(BrowseJavaLocationAsync);

            this.WhenAnyValue(
                t => t.FlutterLocation, t => t.AndroidLocation, t => t.JavaLocation, t => t.SkipAndroidConfiguration,
                (p1, p2, p3, p4) => !string.IsNullOrEmpty(p1) && (!string.IsNullOrEmpty(p2) && !string.IsNullOrEmpty(p3) || p4))
                .BindTo(this, t => t.Button3Enabled);
        }

        public bool ShellInfoVisible => OperatingSystem.IsMacOS();

        public string CurrentShell { get; }

        public string CurrentShellConfigurationFile { get; }

        public string FlutterLocation
        {
            get => _flutterLocation;
            private set => this.RaiseAndSetIfChanged(ref _flutterLocation, value);
        }
        string _flutterLocation;

        public ReactiveCommand<Unit, Unit> BrowseFlutterLocation { get; }

        public async Task BrowseFlutterLocationAsync()
        {
            string location;
            try
            {
                Window window = (Window) Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Directory = FlutterLocation
                };
                location = await dialog.ShowAsync(window);
            }
            catch
            {
                location = string.Empty;
            }
            if (!string.IsNullOrEmpty(location))
                FlutterLocation = location;
        }

        public string AndroidLocation
        {
            get => _androidLocation;
            private set => this.RaiseAndSetIfChanged(ref _androidLocation, value);
        }
        string _androidLocation;

        public ReactiveCommand<Unit, Unit> BrowseAndroidLocation { get; }

        public async Task BrowseAndroidLocationAsync()
        {
            string location;
            try
            {
                Window window = (Window) Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Directory = AndroidLocation
                };
                location = await dialog.ShowAsync(window);
            }
            catch
            {
                location = string.Empty;
            }
            if (!string.IsNullOrEmpty(location))
                AndroidLocation = location;
        }

        public string JavaLocation
        {
            get => _javaLocation;
            private set => this.RaiseAndSetIfChanged(ref _javaLocation, value);
        }
        string _javaLocation;

        public ReactiveCommand<Unit, Unit> BrowseJavaLocation { get; }

        public async Task BrowseJavaLocationAsync()
        {
            string location;
            try
            {
                Window window = (Window)Locator.Current.GetService<IViewFor<SetupWindowViewModel>>();
                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Directory = JavaLocation
                };
                location = await dialog.ShowAsync(window);
            }
            catch
            {
                location = string.Empty;
            }
            if (!string.IsNullOrEmpty(location))
                JavaLocation = location;
        }

        public bool SkipAndroidConfiguration
        {
            get => _skipAndroidConfiguration;
            set => this.RaiseAndSetIfChanged(ref _skipAndroidConfiguration, value);
        }
        bool _skipAndroidConfiguration;
    }
}