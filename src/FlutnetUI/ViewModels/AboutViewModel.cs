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
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Flutnet.Cli.DTO;
using FlutnetUI.Utilities;
using ReactiveUI;
using Splat;

namespace FlutnetUI.ViewModels
{
    public class AboutViewModel : PageViewModel, IActivatableViewModel
    {
        const string FlutnetHome = "https://www.flutnet.com/";

        public AboutViewModel(IScreen hostScreen) : base("about", hostScreen)
        {
            Title = "About";
            Description = "";
            IsDescriptionVisible = false;

            OpenFlutnetWebsite = ReactiveCommand.Create(() => Launcher.OpenURL(FlutnetHome));

            ContactFlutnetInfo = ReactiveCommand.Create(() => Launcher.MailTo("info@flutnet.com"));

            ContactFlutnetSupport = ReactiveCommand.Create(() => Launcher.MailTo("support@flutnet.com"));

            DownloadNewVersion = ReactiveCommand.Create(() => Launcher.OpenURL(_newVersionUrl));

            // Create the command that calls Flutnet CLI 
            // for contacting the server and checking for updates
            CheckForUpdates = ReactiveCommand.CreateFromTask(async ct =>
            {
                CommandLineCallResult callResult = await CommandLineTools.Call<UpdateCheckInArg, UpdateCheckOutArg>(ct);

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.
                if (callResult.Canceled || callResult.Failed)
                {
                    IsNewVersionAvailable = false;
                    FlutnetVersionMessage = "An unexpected error occured, please retry.";
                }
                else
                {
                    UpdateCheckOutArg result = (UpdateCheckOutArg) callResult.CommandResult;
                    if (!result.Success)
                    {
                        IsNewVersionAvailable = false;
                        FlutnetVersionMessage = result.ErrorMessage;
                    }
                    else if (result.UpToDate)
                    {
                        IsNewVersionAvailable = false;
                        FlutnetVersionMessage = "Your software is up to date.";
                    }
                    else
                    {
                        IsNewVersionAvailable = true;
                        FlutnetVersionMessage = $"A new version of Flutnet ({result.NewVersion}) is available.";
                        _newVersionUrl = result.DownloadUrl;
                    }
                }
            });

            CheckForUpdates.IsExecuting.ToProperty(this, x => x.IsCheckingForUpdates, out _isCheckingForUpdates);
            CheckForUpdates.IsExecuting.BindTo(this, x => x.IsBusy);

            this.WhenAnyValue(t => t.CheckForUpdatesAtStartup).Skip(1).Subscribe(value =>
            {
                AppSettings.Default.Preferences.CheckForUpdatesAtStartup = value;
                AppSettings.Default.Preferences.Save();
            });

            // Load user preferences when the view is activated
            this.WhenActivated((Action<IDisposable> disposables) =>
            {
                AppSettings.Default.Preferences.Reload();
                CheckForUpdatesAtStartup = AppSettings.Default.Preferences.CheckForUpdatesAtStartup;
                CurrentFlutnetVersion = Assembly.GetExecutingAssembly().GetProductVersion();
            });
        }

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public string CurrentFlutnetVersion
        {
            get => _currentFlutnetVersion;
            private set => this.RaiseAndSetIfChanged(ref _currentFlutnetVersion, value);
        }
        string _currentFlutnetVersion = string.Empty;

        public ReactiveCommand<Unit, Unit> OpenFlutnetWebsite { get; }

        public ReactiveCommand<Unit, Unit> ContactFlutnetInfo { get; }

        public ReactiveCommand<Unit, Unit> ContactFlutnetSupport { get; }

        public ReactiveCommand<Unit, Unit> DownloadNewVersion { get; }

        public ReactiveCommand<Unit, Unit> CheckForUpdates { get; }

        public bool IsCheckingForUpdates => _isCheckingForUpdates.Value;
        ObservableAsPropertyHelper<bool> _isCheckingForUpdates;

        public bool IsNewVersionAvailable
        {
            get => _isNewVersionAvailable;
            private set => this.RaiseAndSetIfChanged(ref _isNewVersionAvailable, value);
        }
        bool _isNewVersionAvailable;
        string _newVersionUrl = string.Empty;

        public string FlutnetVersionMessage
        {
            get => _flutnetVersionMessage;
            private set => this.RaiseAndSetIfChanged(ref _flutnetVersionMessage, value);
        }
        string _flutnetVersionMessage = string.Empty;

        public bool CheckForUpdatesAtStartup
        {
            get => _checkForUpdatesAtStartup;
            set => this.RaiseAndSetIfChanged(ref _checkForUpdatesAtStartup, value);
        }
        bool _checkForUpdatesAtStartup = true;
    }
}