﻿using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Flutnet.Cli.DTO;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class SetupCheckFlutterViewModel : AdvancedWizardPageViewModel, IActivatableViewModel
    {
        public SetupCheckFlutterViewModel(IScreen screen = null) : base("setup_check_flutter", screen)
        {
            Title = "Check Flutter";

            Button1Visible = false;
            Button2Visible = false;
            Button3Visible = false;

            // Create the command that calls Flutnet CLI 
            RunDiagnostic = ReactiveCommand.CreateFromTask(async ct =>
            {
                CommandLineCallResult callResult = await CommandLineTools.Call<FlutterDiagInArg, FlutterDiagOutArg>(ct);

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.

                SetupResult overallResult = null;
                FlutterDiagOutArg checkResult = null;

                if (callResult.Canceled)
                {
                    // Jump to finish page and notify the user that the operation has been canceled.
                    overallResult = new SetupResult { OperationCanceled = true };
                }
                else if (callResult.Failed)
                {
                    // Jump to finish page and notify the user that an unexpected error occured.
                    overallResult = new SetupResult { OperationFailed = true };
                }
                else
                {
                    FlutterDiagOutArg result = (FlutterDiagOutArg) callResult.CommandResult;
                    if (!result.Success)
                    {
                        // Jump to finish page and notify the user that an unexpected error occured.
                        overallResult = new SetupResult { OperationFailed = true };
                    }
                    else if (result.Issues == FlutterIssues.None || result.Issues == FlutterIssues.CompatibilityIssues)
                    {
                        // Jump to finish page and notify the user if we detected a compatibility issue.
                        overallResult = new SetupResult { OperationResult = result };
                    }
                    else
                    {
                        checkResult = result;
                    }
                }

                if (overallResult != null)
                {
                    await HostScreen.Router.Navigate.Execute(new SetupFinishViewModel(overallResult, this, HostScreen));
                }
                else
                {
                    await HostScreen.Router.Navigate.Execute(new SetupCheckFlutterErrorViewModel(checkResult, HostScreen));
                }
            });

            RunDiagnostic.IsExecuting.ToProperty(this, x => x.IsInProgress, out _isInProgress);
            RunDiagnostic.IsExecuting.BindTo(this, x => x.IsBusy);

            // Execute the command when the View is activated
            this.WhenActivated(disposables =>
            {
                RunDiagnostic.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            });
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ReactiveCommand<Unit, Unit> RunDiagnostic { get; }

        ObservableAsPropertyHelper<bool> _isInProgress;
        public bool IsInProgress => _isInProgress.Value;
    }
}