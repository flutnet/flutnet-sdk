using System.Reactive;
using ReactiveUI;
using FlutnetUI.Models;
using FlutnetUI.Utilities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Flutnet.Cli.DTO;

namespace FlutnetUI.ViewModels
{
    public class CreateProjectProgressViewModel : WizardPageViewModel, IActivatableViewModel
    {
        FlutnetAppSettings _appSettings;
        FlutnetProjectSettings _projectSettings;


        public CreateProjectProgressViewModel(FlutnetAppSettings appSettings, FlutnetProjectSettings projectSettings, NewProjectViewModel screen = null) : base("newprj_progress", screen)
        {
            _appSettings = appSettings;
            _projectSettings = projectSettings;

            Title = "New Project";
            Description = "The project will take some time to generate.\nWait until the procedure finish.\n";
            IsDescriptionVisible = false;

            NextText = "Finish";
            BackVisible = false;
            IsFinishPage = true;

            OutputLines = new ObservableCollection<string>();
            // Observe any changes in the observable collection.
            // Note that the property has no public setters, so we 
            // assume the collection is mutated by using the Add(), 
            // Delete(), Clear() and other similar methods.
            OutputLines
                // Convert the collection to a stream of chunks,
                // so we have IObservable<IChangeSet<TKey, TValue>>
                // type also known as the DynamicData monad.
                .ToObservableChangeSet()
                // Each time the collection changes, we get
                // all updated items at once.
                .ToCollection()
                // Aggregate all the elements in the collection
                // into a multi-line string.
                .Select(lines => string.Join(Environment.NewLine, lines))
                // Then, we convert the multi-line string to the
                // property a multi-line TextBox can bind.
                .ToProperty(this, x => x.Output, out _output, scheduler: RxApp.MainThreadScheduler);

            // Create the command that calls Flutnet CLI 
            CreateProject = ReactiveCommand.CreateFromTask(async ct =>
            {
                NewProjectInArg arguments = BuildCommandLineArg();

                CommandLineCallResult callResult = await CommandLineTools.Call<NewProjectOutArg>(arguments, ct, line =>
                {
                    OutputLines.Add(line);
                });

                // This is not the proper way to change property values and raise property change notifications:
                // we should return a public object and subscribe to the command observable
                // so that we can use ReactiveUI framework methods such as ToProperty, BindTo etc.
                if (callResult.Canceled)
                {
                    IsCanceled = true;
                }
                else if (callResult.Failed)
                {
                    IsFailed = true;
                }
                else
                {
                    OutArg result = callResult.CommandResult;
                    if (!result.Success)
                    {
                        IsFailed = true;
                    }
                    else
                    {
                        IsCompletedSuccessfully = true;
                    }
                }
            });
            
            CreateProject.IsExecuting.ToProperty(this, x => x.IsInProgress, out _isInProgress);
            CreateProject.IsExecuting.BindTo(this, x => x.IsBusy);

            BrowseProject = ReactiveCommand.Create(
                execute: () => Launcher.OpenFolder(Path.Combine(projectSettings.Location, projectSettings.SolutionName)), 
                canExecute: this.WhenAnyValue(t => t.IsCompletedSuccessfully));

            // Execute the command when the View is activated
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                CreateProject.Execute(Unit.Default).Subscribe().DisposeWith(disposables);
            });

            this.WhenAnyValue(t => t.IsInProgress, t => !t).BindTo(this, t => t.NextEnabled);
        }

        public ViewModelActivator Activator { get; }

        private NewProjectInArg BuildCommandLineArg()
        {
            NewProjectInArg arg = new NewProjectInArg
            {
                AppName = _appSettings.AppName,
                OrganizationId = _appSettings.OrganizationId,
                AndroidAppId = _appSettings.AndroidAppId,
                IosAppId = _appSettings.IosAppId,
                TargetAndroid = _appSettings.TargetAndroid,
                TargetIos = _appSettings.TargetIos,
                ProjectName = _projectSettings.ProjectName,
                SolutionName = _projectSettings.SolutionName,
                Location = _projectSettings.Location,
                CreateFlutterSubfolder = _projectSettings.CreateFlutterSubfolder,
                FlutterModuleName = _projectSettings.FlutterModuleName,
                FlutterPackageName = _projectSettings.FlutterPackageName,
                FlutterVersion = _projectSettings.FlutterVersion
            };
            return arg;
        }

        public ReactiveCommand<Unit, Unit> CreateProject { get; }

        ObservableAsPropertyHelper<bool> _isInProgress;
        public bool IsInProgress => _isInProgress.Value;

        public bool IsCanceled
        {
            get => _isCanceled;
            set => this.RaiseAndSetIfChanged(ref _isCanceled, value);
        }
        bool _isCanceled;

        public bool IsFailed
        {
            get => _isFailed;
            private set => this.RaiseAndSetIfChanged(ref _isFailed, value);
        }
        bool _isFailed;

        public bool IsCompletedSuccessfully
        {
            get => _isCompletedSuccessfully;
            private set => this.RaiseAndSetIfChanged(ref _isCompletedSuccessfully, value);
        }
        bool _isCompletedSuccessfully;

        public ObservableCollection<string> OutputLines { get; }

        public string Output => _output.Value;
        ObservableAsPropertyHelper<string> _output;

        public ReactiveCommand<Unit, Unit> BrowseProject { get; }
    }
}