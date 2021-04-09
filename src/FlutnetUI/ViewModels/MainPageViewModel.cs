using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Flutnet.Cli.DTO;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class MainPageViewModel : PageViewModel, IScreen, IActivatableViewModel
    {
        bool _firstActivation = true;

        public MainPageViewModel(IScreen hostScreen) : base("main", hostScreen)
        {
            // Create the command that calls Flutnet CLI 
            // for retrieving current license information
            ReactiveCommand<Unit, LicenseInfo> cmdGetLicenseInfo = ReactiveCommand.CreateFromTask(async ct =>
            {
                CommandLineCallResult callResult = await CommandLineTools.Call<LicInfoInArg, LicInfoOutArg>(ct);

                if (callResult.Canceled || callResult.Failed)
                {
                    return new LicenseInfo
                    {
                        LoadError = true,
                        LoadErrorMessage = "Unable to retrieve license information on this machine."
                    };
                }

                LicInfoOutArg result = (LicInfoOutArg)callResult.CommandResult;
                if (!result.Success)
                {
                    return new LicenseInfo
                    {
                        LoadError = true,
                        LoadErrorMessage = result.ErrorMessage
                    };
                }

                return new LicenseInfo
                {
                    LicenseStatus = result.LicenseStatus,
                    LicenseOwner = result.LicenseOwner,
                    ProductKey = result.ProductKey,
                    LicenseType = result.LicenseType
                };
            });

            cmdGetLicenseInfo.IsExecuting.ToProperty(this, x => x.IsLicenseLoading, out _isLicenseLoading);
            cmdGetLicenseInfo.ToProperty(this, x => x.LicenseInfo, out _licenseInfo);

            // Initialize all the page view models
            NewProjectViewModel newProjectPage = new NewProjectViewModel(this);
            AppKeysViewModel appKeysPage = new AppKeysViewModel(this);
            AboutViewModel aboutPage = new AboutViewModel(this);
    
            // Initialize the commands for navigating to these pages
            OpenNewProjectPage = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(newProjectPage));
            OpenAppKeysPage = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(appKeysPage), canExecute: cmdGetLicenseInfo.IsExecuting.Select(t => !t));
            OpenLicensingPage = ReactiveCommand.CreateFromObservable(
                execute: () => Router.Navigate.Execute(new LicensingViewModel(LicenseInfo, this)), 
                canExecute: cmdGetLicenseInfo.IsExecuting.Select(t => !t));
            OpenAboutPage = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(aboutPage));

            // Create the command for displaying the setup wizard at startup
            ReactiveCommand<Unit, Unit> cmdOpenSetupWizard = ReactiveCommand.CreateFromObservable<Unit, Unit>(sender =>
            {
                return ShowSetupWizard.Handle(Unit.Default);
            });

            // Check license status and optionally check for updates when the view is activated
            this.WhenActivated(disposables =>
            {
                cmdGetLicenseInfo.Execute(Unit.Default).Subscribe().DisposeWith(disposables);

                AppSettings.Default.Preferences.Reload();
                if (AppSettings.Default.Preferences.CheckForUpdatesAtStartup)
                    aboutPage.CheckForUpdates.Execute().Subscribe().DisposeWith(disposables);

                // Set the default (initial) page to the new project page
                Router.Navigate.Execute(newProjectPage).Subscribe().DisposeWith(disposables);
            });

            newProjectPage.WhenActivated(disposables =>
            {
                if (_firstActivation && AppSettings.Default.Preferences.ShowSetupWizardAtStartup)
                {
                    cmdOpenSetupWizard.Execute().Subscribe().DisposeWith(disposables);
                    _firstActivation = false;
                }
            });

            //// Sample of a command executed periodically
            //TimeSpan interval = TimeSpan.FromHours(1);
            //Observable.Timer(interval, interval)
            //    .Select(time => Unit.Default)
            //    .InvokeCommand(aboutPage, p => p.CheckForUpdates);

            aboutPage.WhenAnyValue(p => p.IsNewVersionAvailable).BindTo(this, t => t.IsUpdateAvailable);

            Router.CurrentViewModel
                .Where(vm => vm is IPageViewModel)
                .Select(vm => (IPageViewModel) vm)
                .Subscribe(page =>
                {
                    page.WhenAnyValue(p => p.Title).BindTo(this, t => t.Title);
                    page.WhenAnyValue(p => p.IsBusy).BindTo(this, t => t.IsBusy);
                });

            Router.CurrentViewModel
                .Select(vm => vm is NewProjectViewModel)
                .ToProperty(this, t => t.IsNewProjectPageVisible, out _isNewProjectPageVisible);

            Router.CurrentViewModel
                .Select(vm => vm is AppKeysViewModel)
                .ToProperty(this, t => t.IsAppKeysPageVisible, out _isAppKeysPageVisible);

            Router.CurrentViewModel
                .Select(vm => vm is LicensingViewModel)
                .ToProperty(this, t => t.IsLicensingPageVisible, out _isLicensingPageVisible);

            Router.CurrentViewModel
                .Where(vm => vm is LicensingViewModel)
                .Select(vm => (LicensingViewModel) vm)
                .Subscribe(page =>
                {
                    page.WhenAnyValue(p => p.LicenseInfo).ToProperty(this, t => t.LicenseInfo, out _licenseInfo);
                });

            Router.CurrentViewModel
                .Select(vm => vm is AboutViewModel)
                .ToProperty(this, t => t.IsAboutPageVisible, out _isAboutPageVisible);

            this.WhenAnyValue(t => t.LicenseInfo)
                .Select(t => t == null || t.LoadError || t.LicenseStatus == LicenseStatus.Trial || t.LicenseStatus == LicenseStatus.Invalid)
                .BindTo(appKeysPage, p => p.TrialMode);

            this.WhenAnyValue(t => t.LicenseInfo)
                .Select(t => t == null || t.LoadError || t.LicenseStatus == LicenseStatus.Trial || t.LicenseStatus == LicenseStatus.Invalid)
                .BindTo(newProjectPage, p => p.TrialMode);

            TrialMode = true;

            this.WhenAnyValue(t => t.LicenseInfo)
                .Select(t => t == null || t.LoadError || t.LicenseStatus == LicenseStatus.Trial || t.LicenseStatus == LicenseStatus.Invalid)
                .BindTo(this, p => p.TrialMode);
        }

        #region IScreen implementation

        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        #endregion

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public bool IsLicenseLoading => _isLicenseLoading.Value;
        ObservableAsPropertyHelper<bool> _isLicenseLoading;

        public LicenseInfo LicenseInfo => _licenseInfo.Value;
        ObservableAsPropertyHelper<LicenseInfo> _licenseInfo;

        // Command to open the new project page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenNewProjectPage { get; }

        // Command to open the app keys page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenAppKeysPage { get; }

        // Command to open the licensing page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenLicensingPage { get; }

        // Command to open the about page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenAboutPage { get; }

        // The current page is new project page
        public bool IsNewProjectPageVisible => _isNewProjectPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isNewProjectPageVisible;

        // The current page is app keys page
        public bool IsAppKeysPageVisible => _isAppKeysPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isAppKeysPageVisible;

        // The current page is licensing page
        public bool IsLicensingPageVisible => _isLicensingPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isLicensingPageVisible;

        // The current page is about page
        public bool IsAboutPageVisible => _isAboutPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isAboutPageVisible;

        public bool TrialMode
        {
            get => _trialMode;
            set => this.RaiseAndSetIfChanged(ref _trialMode, value);
        }
        bool _trialMode;

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value);
        }

        private bool _isUpdateAvailable = false;

        public Interaction<Unit, Unit> ShowSetupWizard { get; } = new Interaction<Unit, Unit>();
    }
}