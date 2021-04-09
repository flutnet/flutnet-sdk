using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class MainPageViewModel : PageViewModel, IScreen, IActivatableViewModel
    {
        bool _firstActivation = true;

        public MainPageViewModel(IScreen hostScreen) : base("main", hostScreen)
        {
            // Initialize all the page view models
            NewProjectViewModel newProjectPage = new NewProjectViewModel(this);
            AboutViewModel aboutPage = new AboutViewModel(this);
    
            // Initialize the commands for navigating to these pages
            OpenNewProjectPage = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(newProjectPage));
            OpenAboutPage = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(aboutPage));

            // Create the command for displaying the setup wizard at startup
            ReactiveCommand<Unit, Unit> cmdOpenSetupWizard = ReactiveCommand.CreateFromObservable<Unit, Unit>(sender =>
            {
                return ShowSetupWizard.Handle(Unit.Default);
            });

            // Check license status and optionally check for updates when the view is activated
            this.WhenActivated(disposables =>
            {
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
                .Select(vm => vm is AboutViewModel)
                .ToProperty(this, t => t.IsAboutPageVisible, out _isAboutPageVisible);
        }

        #region IScreen implementation

        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        #endregion

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        // Command to open the new project page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenNewProjectPage { get; }

        // Command to open the about page
        public ReactiveCommand<Unit, IRoutableViewModel> OpenAboutPage { get; }

        // The current page is new project page
        public bool IsNewProjectPageVisible => _isNewProjectPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isNewProjectPageVisible;

        // The current page is about page
        public bool IsAboutPageVisible => _isAboutPageVisible.Value;
        ObservableAsPropertyHelper<bool> _isAboutPageVisible;

        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value);
        }
        bool _isUpdateAvailable;

        public Interaction<Unit, Unit> ShowSetupWizard { get; } = new Interaction<Unit, Unit>();
    }
}