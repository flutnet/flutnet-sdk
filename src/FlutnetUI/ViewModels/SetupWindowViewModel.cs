using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class SetupWindowViewModel : ReactiveObject, IScreen, IActivatableViewModel
    {
        public SetupWindowViewModel()
        {
            IObservable<IAdvancedWizardPageViewModel> pageObservable = Router.CurrentViewModel
                .Where(vm => vm is IAdvancedWizardPageViewModel)
                .Select(vm => vm as IAdvancedWizardPageViewModel);

            pageObservable.Select(p => p.Title).BindTo(this, t => t.Title);
            pageObservable.Subscribe(p => p.WhenAnyValue(page => page.IsBusy).BindTo(this, t => t.IsBusy));

            pageObservable.Select(p => p.Button1Text).ToProperty(this, t => t.Button1Text, out _button1Text);
            pageObservable.Select(p => p.Button2Text).ToProperty(this, t => t.Button2Text, out _button2Text);
            pageObservable.Select(p => p.Button3Text).ToProperty(this, t => t.Button3Text, out _button3Text);

            pageObservable.Select(p => p.Button1Visible).ToProperty(this, t => t.HasButton1, out _hasButton1);
            pageObservable.Select(p => p.Button2Visible).ToProperty(this, t => t.HasButton2, out _hasButton2);
            pageObservable.Select(p => p.Button3Visible).ToProperty(this, t => t.HasButton3, out _hasButton3);

            //pageObservable.Select(p => p.Button3Enabled).ToProperty(this, t => t.Button3Enabled, out _button3Enabled);
            pageObservable.Subscribe(p => p.WhenAnyValue(page => page.Button3Enabled).BindTo(this, t => t.Button3Enabled));

            pageObservable.Select(p => p.Button1Command).ToProperty(this, t => t.Button1Command, out _button1Command);
            pageObservable.Select(p => p.Button2Command).ToProperty(this, t => t.Button2Command, out _button2Command);
            pageObservable.Select(p => p.Button3Command).ToProperty(this, t => t.Button3Command, out _button3Command);

            //TODO
            /*
             *
   
            pageObservable
                .Select(p =>
                {
                    ReactiveCommand<Unit, Unit> cmdDoSomething = ReactiveCommand.CreateFromTask(p.OnNext, outputScheduler: RxApp.MainThreadScheduler);

                    ReactiveCommand<Unit, Unit> cmdNavigateOrClose;
                    if (p.IsFinishPage)
                    {
                        cmdNavigateOrClose = ReactiveCommand.CreateFromTask(async () => 
                        {
                            await cmdDoSomething.Execute();
                            await Router.NavigateAndReset.Execute(new ConfigureAppViewModel(this));
                        }, canExecute: p.WhenAnyValue(page => page.NextEnabled), outputScheduler: RxApp.MainThreadScheduler);
                    }
                    else
                    {
                        cmdNavigateOrClose = ReactiveCommand.CreateFromTask(async () =>
                        {
                            await cmdDoSomething.Execute();
                            await Router.Navigate.Execute(p.GetNextPage());
                        }, canExecute: p.WhenAnyValue(page => page.NextEnabled), outputScheduler: RxApp.MainThreadScheduler);
                    }

                    cmdNavigateOrClose
                        .ThrownExceptions
                        //.FirstAsync()
                        .Throttle(TimeSpan.FromMilliseconds(250), RxApp.MainThreadScheduler)
                        .Subscribe(async ex =>
                        {
                            await ShowError.Handle(ex);
                        });

                    return cmdNavigateOrClose;
                })
                .ToProperty(this, t => t.GoNext, out _goNext);

             *         
             */

            this.WhenActivated(disposables =>
            {
                if (Router.NavigationStack.Count > 0)
                {
                    IRoutableViewModel fistPage = Router.NavigationStack[0];
                    Router.NavigateAndReset.Execute(fistPage);
                    Router.NavigateAndReset.Execute(fistPage).Subscribe().DisposeWith(disposables);
                }
                else
                {
                    Router.Navigate.Execute(new SetupWelcomeViewModel(this)).Subscribe().DisposeWith(disposables);
                }
            });
        }

        #region IScreen implementation

        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        #endregion

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public string Title
        {
            get => _title;
            private set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        string _title;

        public bool IsBusy
        {
            get => _isBusy;
            private set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        bool _isBusy;

        public string Button1Text => _button1Text.Value;
        ObservableAsPropertyHelper<string> _button1Text;
        
        public bool HasButton1 => _hasButton1.Value;
        ObservableAsPropertyHelper<bool> _hasButton1;

        public string Button2Text => _button2Text.Value;
        ObservableAsPropertyHelper<string> _button2Text;

        public bool HasButton2 => _hasButton2.Value;
        ObservableAsPropertyHelper<bool> _hasButton2;

        public string Button3Text => _button3Text.Value;
        ObservableAsPropertyHelper<string> _button3Text;

        public bool HasButton3 => _hasButton3.Value;
        ObservableAsPropertyHelper<bool> _hasButton3;

        //public bool Button3Enabled => _button3Enabled.Value;
        //ObservableAsPropertyHelper<bool> _button3Enabled;

        public bool Button3Enabled
        {
            get => _button3Enabled;
            private set => this.RaiseAndSetIfChanged(ref _button3Enabled, value);
        }
        bool _button3Enabled;

        public ReactiveCommand<Unit, Unit> Button1Command => _button1Command.Value;
        ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>> _button1Command;

        public ReactiveCommand<Unit, Unit> Button2Command => _button2Command.Value;
        ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>> _button2Command;

        public ReactiveCommand<Unit, Unit> Button3Command => _button3Command.Value;
        ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>> _button3Command;

        public Interaction<Exception, Unit> ShowError { get; } = new Interaction<Exception, Unit>();

        public Interaction<Unit, Unit> CloseWizard { get; } = new Interaction<Unit, Unit>();

        internal static string ReplaceText(string text, string flutterVersionCurrent, string flutterVersionTarget)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string downloadUrl;
            string officialInstallUrl;
            string officialUpdatePathUrl;
            string flutnetInstallUrl;

            if (Utilities.OperatingSystem.IsMacOS())
            {
                downloadUrl = $"https://storage.googleapis.com/flutter_infra/releases/stable/macos/flutter_macos_{flutterVersionTarget}-stable.zip";
                officialInstallUrl = "https://flutter.dev/docs/get-started/install/macos";
                officialUpdatePathUrl = "https://flutter.dev/docs/get-started/install/macos#update-your-path";
                flutnetInstallUrl = "https://www.flutnet.com/Documentation/Getting-Started/Install-on-macOS";
            }
            else
            {
                downloadUrl = $"https://storage.googleapis.com/flutter_infra/releases/stable/windows/flutter_windows_{flutterVersionTarget}-stable.zip";
                officialInstallUrl = "https://flutter.dev/docs/get-started/install/windows";
                officialUpdatePathUrl = "https://flutter.dev/docs/get-started/install/windows#update-your-path";
                flutnetInstallUrl = "https://www.flutnet.com/Documentation/Getting-Started/Install-on-Windows";
            }

            string str = text
                .Replace("@FLUTTER_SDK_DOWNLOAD_URL@", downloadUrl)
                .Replace("@FLUTTER_INSTALL_PAGE@", officialInstallUrl)
                .Replace("@FLUTTER_INSTALL_UPDATE_PATH_LINK@", officialUpdatePathUrl)
                .Replace("@FLUTNET_INSTALL_PAGE@", flutnetInstallUrl)
                .Replace("@FLUTTER_VERSION_CURRENT@", flutterVersionCurrent)
                .Replace("@FLUTTER_VERSION_TARGET@", flutterVersionTarget);
            return str;
        }
    }
}