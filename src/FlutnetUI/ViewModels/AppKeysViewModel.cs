using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Flutnet.Cli.DTO;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class AppKeysViewModel : PageViewModel, IActivatableViewModel
    {
        public AppKeysViewModel(IScreen screen = null) : base("appkeys", screen)
        {
            Title = "App Keys";
            Description = "View or Edit your Android and iOS application Keys and activate the Flutnet Library in this projects.\nRemember that IDs are associated with your specific Licence key.";
            IsDescriptionVisible = true;

            Items = new ObservableCollection<AppKeyItem>();
            TrialMode = true;

            // Create the command that calls Flutnet CLI 
            // for retrieving registered application IDs and their app key
            GetAppKeys = ReactiveCommand.CreateFromTask(async ct =>
            {
                CommandLineCallResult callResult = await CommandLineTools.Call<AppKeysInArg, AppKeysOutArg>(ct);

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.
                if (callResult.Canceled || callResult.Failed)
                {
                    await ShowError.Handle("An unexpected error occured, please retry.");
                }
                else
                {
                    AppKeysOutArg result = (AppKeysOutArg) callResult.CommandResult;
                    if (!result.Success)
                    {
                        await ShowError.Handle(result.ErrorMessage);
                    }
                    else
                    {
                        Items.Clear();
                        foreach (KeyValuePair<string, string> item in result.Items)
                            Items.Add(new AppKeyItem(item.Key, item.Value));
                    }
                }
            });

            GetAppKeys.IsExecuting.ToProperty(this, x => x.IsLoading, out _isLoading);

            //// Execute the command when the View is activated
            //this.WhenActivated(disposables =>
            //{
            //    GetAppKeys.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            //});

            // Create the command that calls Flutnet CLI 
            // for registering an application ID
            Add = ReactiveCommand.CreateFromTask(async ct =>
            {
                RegisterAppInArg args = new RegisterAppInArg { ApplicationId = NewApplicationId };
                CommandLineCallResult callResult = await CommandLineTools.Call<RegisterAppOutArg>(args, ct);

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.
                if (callResult.Canceled || callResult.Failed)
                {
                    await ShowError.Handle("An unexpected error occured, please retry.");
                }
                else
                {
                    RegisterAppOutArg result = (RegisterAppOutArg) callResult.CommandResult;
                    if (!result.Success)
                    {
                        await ShowError.Handle(result.ErrorMessage);
                    }
                    else
                    {
                        Items.Add(new AppKeyItem(args.ApplicationId, result.AppKey));
                        NewApplicationId = string.Empty;
                    }
                }
            }, canExecute: this.WhenAnyValue(t => t.NewApplicationId, id => !string.IsNullOrWhiteSpace(id) && id.Contains('.')));

            GetAppKey = ReactiveCommand.CreateFromObservable<Button, Unit>(sender =>
            {
                AppKeyItem item = (AppKeyItem) sender.DataContext;
                return ShowAppKey.Handle(item);
            });

            Remove = ReactiveCommand.CreateFromTask<Button, Unit>(async (sender, ct) =>
            {
                AppKeyItem item = (AppKeyItem) sender.DataContext;

                bool confirm = await ConfirmRemove.Handle(item.ApplicationId);
                if (!confirm)
                    return Unit.Default;

                UnregisterAppInArg args = new UnregisterAppInArg { ApplicationId = item.ApplicationId };
                CommandLineCallResult callResult = await CommandLineTools.Call<UnregisterAppOutArg>(args, ct);

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.
                if (callResult.Canceled || callResult.Failed)
                {
                    await ShowError.Handle("An unexpected error occured, please retry.");
                }
                else
                {
                    if (!callResult.CommandResult.Success)
                    {
                        await ShowError.Handle(callResult.CommandResult.ErrorMessage);
                    }
                    else
                    {
                        Items.Remove(item);
                    }
                }
                return Unit.Default;
            });
        }

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public ReactiveCommand<Unit, Unit> GetAppKeys { get; }

        public bool IsLoading => _isLoading.Value;
        ObservableAsPropertyHelper<bool> _isLoading;

        public ObservableCollection<AppKeyItem> Items { get; }

        public bool TrialMode
        {
            get => _trialMode;
            set => this.RaiseAndSetIfChanged(ref _trialMode, value);
        }
        bool _trialMode;

        public string NewApplicationId
        {
            get => _newApplicationId;
            set => this.RaiseAndSetIfChanged(ref _newApplicationId, value);
        }
        string _newApplicationId;

        public ReactiveCommand<Unit, Unit> Add { get; }

        public ReactiveCommand<Button, Unit> GetAppKey { get; }

        public ReactiveCommand<Button, Unit> Remove { get; }

        public Interaction<string, bool> ConfirmRemove { get; } = new Interaction<string, bool>();

        public Interaction<AppKeyItem, Unit> ShowAppKey { get; } = new Interaction<AppKeyItem, Unit>();

        public Interaction<string, Unit> ShowError { get; } = new Interaction<string, Unit>();



    }
}
