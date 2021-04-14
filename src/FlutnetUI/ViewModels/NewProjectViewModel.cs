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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class NewProjectViewModel : PageViewModel, IScreen, IActivatableViewModel
    {
        public NewProjectViewModel(IScreen hostScreen) : base("newprj", hostScreen)
        {

            var configureAppModel = new ConfigureAppViewModel(this);

            Router.Navigate.Execute(configureAppModel);

            OpenSetupWizard = ReactiveCommand.CreateFromObservable<Unit, Unit>(sender =>
            {
                return ShowSetupWizard.Handle(Unit.Default);
            });

            IObservable<WizardPageViewModel> pageObservable = Router.CurrentViewModel
                .Where(vm => vm is WizardPageViewModel)
                .Select(vm => vm as WizardPageViewModel);

            pageObservable.Select(p => p.Title).BindTo(this, t => t.Title);
            pageObservable.Select(p => p.Description).BindTo(this, t => t.Description);
            pageObservable.Select(p => p.IsDescriptionVisible).BindTo(this, t => t.IsDescriptionVisible);
            pageObservable.Subscribe(p => p.WhenAnyValue(page => page.IsBusy).BindTo(this, t => t.IsBusy));

            pageObservable.Select(p => p.NextText).ToProperty(this, t => t.NextText, out _nextText);
            pageObservable.Select(p => p.BackText).ToProperty(this, t => t.BackText, out _backText);
            pageObservable.Select(p => p.BackVisible).ToProperty(this, t => t.HasBack, out _hasBack);

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

            this.WhenActivated(disposables =>
            {
                if (Router.NavigationStack.Count > 0)
                {
                    IRoutableViewModel fistPage = Router.NavigationStack[0];
                    Router.NavigateAndReset.Execute(fistPage);
                    Router.NavigateAndReset.Execute(fistPage).Subscribe().DisposeWith(disposables);
                }
                //else
                //{
                //    Router.Navigate.Execute(new ConfigureAppViewModel(this)).Subscribe().DisposeWith(disposables);
                //}
            });
        }

        #region IScreen implementation

        // The Router associated with this Screen.
        public RoutingState Router { get; } = new RoutingState();

        #endregion

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public ReactiveCommand<Unit, Unit> GoNext => _goNext.Value;
        ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>> _goNext;

        public string NextText => _nextText.Value;

        ObservableAsPropertyHelper<string> _nextText;

        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

        public string BackText => _backText.Value;
        ObservableAsPropertyHelper<string> _backText;

        public bool HasBack => _hasBack.Value;
        ObservableAsPropertyHelper<bool> _hasBack;

        public Interaction<Exception, Unit> ShowError { get; } = new Interaction<Exception, Unit>();

        public ReactiveCommand<Unit, Unit> OpenSetupWizard { get; }
        public Interaction<Unit, Unit> ShowSetupWizard { get; } = new Interaction<Unit, Unit>();
    }
}