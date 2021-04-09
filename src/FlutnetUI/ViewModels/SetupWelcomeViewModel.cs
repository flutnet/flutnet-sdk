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