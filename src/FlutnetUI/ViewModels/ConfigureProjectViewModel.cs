using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Flutnet.Cli.DTO;
using ReactiveUI;
using Splat;
using OperatingSystem = FlutnetUI.Utilities.OperatingSystem;
using static FlutnetUI.Utilities.NamingConventions;
using FlutnetUI.Models;
using FlutnetUI.Utilities;

namespace FlutnetUI.ViewModels
{
    public class ConfigureProjectViewModel : WizardPageViewModel
    {
        FlutnetAppSettings _appSettings;

        public ConfigureProjectViewModel(FlutnetAppSettings appSettings, IScreen screen = null) : base("newprj_project", screen)
        {
            _appSettings = appSettings;

            Title = "New Project";

            Description = "Edit project settings: you'll see a preview in the right panel.\nPressing \"Create\" will generate the project folder and all relative files.";
            IsDescriptionVisible = true;

            NextText = "Create";
            IsFinishPage = false;
            
            BrowseLocation = ReactiveCommand.CreateFromTask(BrowseLocationAsync);
            BrowseLocation.Where(t => !string.IsNullOrEmpty(t)).BindTo(this, t => t.Location);

            AppSettings.Default.Preferences.Reload();
            _location = !string.IsNullOrEmpty(AppSettings.Default.Preferences.LastProjectLocation)
                ? AppSettings.Default.Preferences.LastProjectLocation
                : OperatingSystem.DefaultProjectsLocation();

            if (appSettings != null)
            {
                string projectName = AppNameToProjectName(appSettings.AppName);
                if (!string.IsNullOrEmpty(projectName))
                    _projectName = projectName;
            }
            else
            {
                _projectName = DefaultProjectName;
                _solutionName = DefaultSolutionName;
                _flutterModuleName = DefaultFlutterModuleName;
            }

            this.WhenAnyValue(t => t.ProjectName)
                .Do(t => 
                {
                    this.RaisePropertyChanged(nameof(SolutionName));
                    this.RaisePropertyChanged(nameof(FlutterModuleName));
                })
                .Select(t => ToSafeFilename(t, DefaultAppName))
                .ToProperty(this,t => t.OutputProjectName, out _outputProjectName, initialValue: ProjectName);

            this.WhenAnyValue(t => t.OutputProjectName)
                .Select(t => $"{t}.Android")
                .ToProperty(this, t => t.OutputProjectDroidName, out _outputProjectDroidName, initialValue: $"{OutputProjectName}.Android");

            this.WhenAnyValue(t => t.OutputProjectName)
                .Select(t => $"{t}.iOS")
                .ToProperty(this, t => t.OutputProjectIosName, out _outputProjectIosName, initialValue: $"{OutputProjectName}.iOS");

            this.WhenAnyValue(t => t.OutputProjectName)
                .Select(t => $"{t}.ServiceLibrary")
                .ToProperty(this, t => t.OutputProjectServiceLibName, out _outputProjectServiceLibName, initialValue: $"{OutputProjectName}.ServiceLibrary");

            this.WhenAnyValue(t => t.SolutionName)
                .Select(t => ToSafeFilename(t, DefaultSolutionName))
                .ToProperty(this, t => t.OutputSolutionName, out _outputSolutionName, initialValue: SolutionName ?? DefaultSolutionName);

            this.WhenAnyValue(t => t.FlutterModuleName)
                .Select(ToSafeDartPackageName)
                .ToProperty(this, t => t.OutputFlutterModuleName, out _outputFlutterModuleName, initialValue: FlutterModuleName ?? DefaultFlutterModuleName);

            this.WhenAnyValue(t => t.OutputFlutterModuleName)
                .Select(t => $"{t}_bridge")
                .ToProperty(this, t => t.OutputFlutterPackageName, out _outputFlutterPackageName, initialValue: $"{OutputFlutterModuleName}_bridge");

            this.WhenAnyValue(
                    t => t.ProjectName, t => t.SolutionName, t => t.FlutterModuleName,
                    (prj, sln, flutter) => !string.IsNullOrWhiteSpace(prj) && !string.IsNullOrWhiteSpace(sln) && !string.IsNullOrWhiteSpace(flutter))
                .BindTo(this, t => t.NextEnabled);

            BuildProjectTree();

            // Command to check if the installed version of Flutter is supported
            CheckFlutterVersion = ReactiveCommand.CreateFromTask(async ct =>
            {
                CommandLineCallResult callResult = await CommandLineTools.Call<FlutterInfoInArg, FlutterInfoOutArg>(ct);

                if (callResult.Canceled || callResult.Failed)
                {
                    FlutterVersionHasIssues = true;
                    FlutterVersionNotes = "Unable to detect installed Flutter version. Compatibility with Flutnet unknown.";
                    return;
                }

                FlutterInfoOutArg result = (FlutterInfoOutArg) callResult.CommandResult;
                if (!result.Success)
                {
                    FlutterVersionHasIssues = true;
                    FlutterVersionNotes = "Unable to detect installed Flutter version. Compatibility with Flutnet unknown.";
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Installed Flutter version: {result.InstalledVersion}");

                switch (result.Compatibility)
                {
                    case FlutterCompatibility.Supported:
                        FlutterVersionHasIssues = false;
                        break;

                    case FlutterCompatibility.SupportNotGuaranteed:
                        FlutterVersionHasIssues = true;
                        sb.AppendLine("This version is NOT officially compatible with Flutnet and the resulting projects may not work or may exhibit unexpected behaviors.");
                        if (!string.IsNullOrEmpty(result.NextSupportedVersion))
                            sb.AppendLine($"We recommend that you update Flutter to the latest supported version ({result.LatestSupportedVersion}).");
                        else
                            sb.AppendLine($"The latest supported version is {result.LatestSupportedVersion}.");
                        break;

                    case FlutterCompatibility.NotSupported:
                        FlutterVersionHasIssues = true;
                        NextEnabled = false;
                        sb.AppendLine("Unfortunately this version is NOT compatible with Flutnet.");
                        sb.AppendLine($"Please update Flutter to the latest supported version ({result.LatestSupportedVersion}).");
                        break;
                }

                FlutterVersion = result.InstalledVersion;
                FlutterVersionNotes = sb.ToString();
            });
            CheckFlutterVersion.IsExecuting.BindTo(this, x => x.IsBusy);

            this.WhenAnyValue(t => t.IsBusy).Select(t => !t).BindTo(this, p => p.NextEnabled);

            FlutterVersionNotes = "Retrieving information about the installed version of Flutter...";
        }

        #region IActivatableViewModel implementation

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        #endregion

        public override IRoutableViewModel GetNextPage()
        {
            return new CreateProjectProgressViewModel(_appSettings, BuildSettings(), HostScreen as NewProjectViewModel);
        }

        public override Task OnNext()
        {
            AppSettings.Default.Preferences.Reload();
            AppSettings.Default.Preferences.LastProjectLocation = Location;
            AppSettings.Default.Preferences.Save();

            return base.OnNext();
        }

        public ReactiveCommand<Unit, Unit> CheckFlutterVersion { get; }

        #region Xamarin (.NET) Projects settings

        /// <summary>
        /// The (base) name of Xamarin projects chosen by the user.
        /// </summary>
        public string ProjectName
        {
            get => _projectName;
            set => this.RaiseAndSetIfChanged(ref _projectName, value);
        }
        string _projectName;

        public string OutputProjectName => _outputProjectName.Value;
        ObservableAsPropertyHelper<string> _outputProjectName;

        public string OutputProjectDroidName => _outputProjectDroidName.Value;
        ObservableAsPropertyHelper<string> _outputProjectDroidName;

        public string OutputProjectIosName => _outputProjectIosName.Value;
        ObservableAsPropertyHelper<string> _outputProjectIosName;

        public string OutputProjectServiceLibName => _outputProjectServiceLibName.Value;
        ObservableAsPropertyHelper<string> _outputProjectServiceLibName;

        /// <summary>
        /// The name of Visual Studio solution chosen by the user.
        /// </summary>
        public string SolutionName
        {
            get => _solutionName ?? _projectName;
            set => this.RaiseAndSetIfChanged(ref _solutionName, value);
        }
        string _solutionName;

        public string OutputSolutionName => _outputSolutionName.Value;
        ObservableAsPropertyHelper<string> _outputSolutionName;

        #endregion

        #region Destination Folder

        public string Location
        {
            get => _location;
            set => this.RaiseAndSetIfChanged(ref _location, value);
        }
        string _location;

        public ReactiveCommand<Unit, string> BrowseLocation { get; }

        public async Task<string> BrowseLocationAsync()
        {
            try
            {
                Window window = (Window) Locator.Current.GetService<IViewFor<MainWindowViewModel>>();
                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Directory = Location
                };
                return await dialog.ShowAsync(window);
            }
            catch (Exception ex)
            {
                Log.Ex(ex);
                return string.Empty;
            }
        }

        #endregion

        #region Flutter Projects settings

        /// <summary>
        /// Indicates whether a directory containing Flutter-related projects
        /// should be created within the solution directory.
        /// </summary>
        public bool CreateFlutterSubfolder
        {
            get => _createFlutterSubfolder;
            set => this.RaiseAndSetIfChanged(ref _createFlutterSubfolder, value);
        }
        bool _createFlutterSubfolder = true;

        /// <summary>
        /// The name of the (sub)directory within the solution directory
        /// containing Flutter-related projects. 
        /// </summary>
        public string FlutterSubfolderName => "Flutter";

        /// <summary>
        /// The name of the Flutter module chosen by the user.
        /// It will contain the Dart code that will be integrated into Xamarin native applications.
        /// </summary>
        public string FlutterModuleName
        {
            get => _flutterModuleName ?? AppNameToDartPackageName(_projectName);
            set => this.RaiseAndSetIfChanged(ref _flutterModuleName, value);
        }
        string _flutterModuleName;

        /// <summary>
        /// The name of the Flutter module containg the Dart code 
        /// that will be integrated into Xamarin native applications.
        /// </summary>
        public string OutputFlutterModuleName => _outputFlutterModuleName.Value;
        ObservableAsPropertyHelper<string> _outputFlutterModuleName;

        /// <summary>
        /// The name of the Flutter package containing the Dart code 
        /// auto-generated by Flutnet tools.
        /// </summary>
        public string OutputFlutterPackageName => _outputFlutterPackageName.Value;
        ObservableAsPropertyHelper<string> _outputFlutterPackageName;

        /// <summary>
        /// The installed version of Flutter.
        /// </summary>
        public string FlutterVersion
        {
            get => _flutterVersion;
            private set => this.RaiseAndSetIfChanged(ref _flutterVersion, value);
        }
        string _flutterVersion;

        /// <summary>
        /// Notes about the installed version of Flutter
        /// and its compatibility with the current version of Flutnet SDK.
        /// </summary>
        public string FlutterVersionNotes
        {
            get => _flutterVersionNotes;
            private set => this.RaiseAndSetIfChanged(ref _flutterVersionNotes, value);
        }
        string _flutterVersionNotes;

        /// <summary>
        /// Indicates whether there are compatibility issues
        /// between Flutnet SDK and the installed version of Flutter.
        /// </summary>
        public bool FlutterVersionHasIssues
        {
            get => _flutterVersionHasIssues;
            private set => this.RaiseAndSetIfChanged(ref _flutterVersionHasIssues, value);
        }
        bool _flutterVersionHasIssues;

        #endregion

        #region Project Tree preview

        public ObservableCollection<TreeItem> ProjectTree { get; private set; }

        TreeItem _nodeLocation;
        TreeItem _nodeSolutionFolder;
        TreeItem _nodeSolutionFile;

        TreeItem _nodeProjectDroidFolder;
        TreeItem _nodeProjectDroidFile;
        TreeItem _nodeProjectIosFolder;
        TreeItem _nodeProjectIosFile;
        TreeItem _nodeProjectServiceLibFolder;
        TreeItem _nodeProjectServiceLibFile;

        TreeItem _nodeFlutterFolder;
        TreeItem _nodeFlutterModuleFolder;
        TreeItem _nodeFlutterPackageFolder;

        private void BuildProjectTree()
        {
            string folderIcon = "VSCodeDark.folder-opened";
            string xamarinIcon = "Material.XamarinOutline";
            string flutterIcon = "SimpleIcons.Flutter_Blue";
            string visualStudioIcon = "Material.VisualStudio";


            _nodeLocation = new FolderTreeItem(Location)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeSolutionFolder = new FolderTreeItem(OutputSolutionName)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeSolutionFile = new FileTreeItem($"{OutputSolutionName}.sln")
            {
                Image = LoadImage(visualStudioIcon)
            };
            _nodeLocation.AddChild(_nodeSolutionFolder);

            _nodeProjectDroidFolder = new FolderTreeItem(OutputProjectDroidName)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeProjectDroidFile = new FileTreeItem($"{OutputProjectDroidName}.csproj")
            {
                Image = LoadImage("Material.XamarinOutline_Green")
            };
            _nodeProjectDroidFolder.AddChild(_nodeProjectDroidFile);
            _nodeProjectIosFolder = new FolderTreeItem(OutputProjectIosName)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeProjectIosFile = new FileTreeItem($"{OutputProjectIosName}.csproj")
            {
                Image = LoadImage("Material.XamarinOutline_Gray")
            };
            _nodeProjectIosFolder.AddChild(_nodeProjectIosFile);
            _nodeProjectServiceLibFolder = new FolderTreeItem(OutputProjectServiceLibName)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeProjectServiceLibFile = new FileTreeItem($"{OutputProjectServiceLibName}.csproj")
            {
                Image = LoadImage("Modern.FrameworkNet")
            };
            _nodeProjectServiceLibFolder.AddChild(_nodeProjectServiceLibFile);

            _nodeFlutterFolder = new FolderTreeItem(FlutterSubfolderName)
            {
                Image = LoadImage(folderIcon)
            };
            _nodeFlutterModuleFolder = new FolderTreeItem(OutputFlutterModuleName)
            {
                Image = LoadImage(flutterIcon)
            };
            _nodeFlutterPackageFolder = new FolderTreeItem(OutputFlutterPackageName)
            {
                Image = LoadImage(flutterIcon)
            };
            _nodeFlutterFolder.AddChild(_nodeFlutterModuleFolder);
            _nodeFlutterFolder.AddChild(_nodeFlutterPackageFolder);

            _nodeSolutionFolder.AddChild(_nodeProjectDroidFolder);
            _nodeSolutionFolder.AddChild(_nodeProjectIosFolder);
            _nodeSolutionFolder.AddChild(_nodeProjectServiceLibFolder);
            _nodeSolutionFolder.AddChild(_nodeFlutterFolder);
            _nodeSolutionFolder.AddChild(_nodeSolutionFile);


            this.WhenAnyValue(t => t.Location).BindTo(_nodeLocation, t => t.Text);
            this.WhenAnyValue(t => t.OutputSolutionName).BindTo(_nodeSolutionFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputSolutionName).Select(t => $"{t}.sln").BindTo(_nodeSolutionFile, t => t.Text);

            this.WhenAnyValue(t => t.OutputProjectDroidName).BindTo(_nodeProjectDroidFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputProjectDroidName).Select(t => $"{t}.csproj").BindTo(_nodeProjectDroidFile, t => t.Text);
            this.WhenAnyValue(t => t.OutputProjectIosName).BindTo(_nodeProjectIosFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputProjectIosName).Select(t => $"{t}.csproj").BindTo(_nodeProjectIosFile, t => t.Text);
            this.WhenAnyValue(t => t.OutputProjectServiceLibName).BindTo(_nodeProjectServiceLibFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputProjectServiceLibName).Select(t => $"{t}.csproj").BindTo(_nodeProjectServiceLibFile, t => t.Text);

            this.WhenAnyValue(t => t.CreateFlutterSubfolder).Subscribe(b =>
            {
                if (b)
                {
                    if (_nodeFlutterModuleFolder.Parent == _nodeFlutterFolder)
                        return;

                    //NOTE Matteo: Nota per Michele:
                    // A runtime scoppia quando modifichi
                    // la checkbox la seconda volta. Dice chiave duplicata (my_app), e non capisco come mai. 
                    // Michele: sì, lo so, è legato a questo bug: https://github.com/AvaloniaUI/Avalonia/pull/3216

                    _nodeSolutionFolder.RemoveChild(_nodeFlutterModuleFolder);
                    _nodeSolutionFolder.RemoveChild(_nodeFlutterPackageFolder);

                    //-----------------------------------------------

                    // SOLUZIONE AL PROBLEMA SOPRA INDICATO: rialloco i nodi e li riaggiancio agli eventi.
                    // Potrebbe presentarsi un memory leak, ma per una app così semplice, per adesso è accettabile come soluzione.

                    // Reallocate
                    // - flutter_folder/
                    //    - flutter_module
                    //    - flutter_bridge
                    _nodeFlutterFolder = new FolderTreeItem(FlutterSubfolderName)
                    {
                        Image = LoadImage(folderIcon)
                    };
                    _nodeFlutterModuleFolder = new FolderTreeItem(OutputFlutterModuleName)
                    {
                        Image = LoadImage(flutterIcon)
                    };
                    _nodeFlutterPackageFolder = new FolderTreeItem(OutputFlutterPackageName)
                    {
                        Image = LoadImage(flutterIcon)
                    };

                    // Reconnect the events to the nodes
                    this.WhenAnyValue(t => t.FlutterSubfolderName).BindTo(_nodeFlutterFolder, t => t.Text);
                    this.WhenAnyValue(t => t.OutputFlutterModuleName).BindTo(_nodeFlutterModuleFolder, t => t.Text);
                    this.WhenAnyValue(t => t.OutputFlutterPackageName).BindTo(_nodeFlutterPackageFolder, t => t.Text);

                    //-----------------------------------------------


                    // Riaggiungo la struttura ad albero aggiornata

                    _nodeFlutterFolder.AddChild(_nodeFlutterModuleFolder);
                    _nodeFlutterFolder.AddChild(_nodeFlutterPackageFolder);

                    _nodeSolutionFolder.AddChild(_nodeFlutterFolder);

                }
                else
                {
                    if (_nodeFlutterModuleFolder.Parent == _nodeSolutionFolder)
                        return;

                    _nodeFlutterFolder.RemoveChild(_nodeFlutterModuleFolder);
                    _nodeFlutterFolder.RemoveChild(_nodeFlutterPackageFolder);
                    _nodeSolutionFolder.RemoveChild(_nodeFlutterFolder);

                    _nodeSolutionFolder.AddChild(_nodeFlutterModuleFolder);
                    _nodeSolutionFolder.AddChild(_nodeFlutterPackageFolder);
                }
            });
            this.WhenAnyValue(t => t.FlutterSubfolderName).BindTo(_nodeFlutterFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputFlutterModuleName).BindTo(_nodeFlutterModuleFolder, t => t.Text);
            this.WhenAnyValue(t => t.OutputFlutterPackageName).BindTo(_nodeFlutterPackageFolder, t => t.Text);

            ProjectTree = new ObservableCollection<TreeItem>(new[] {_nodeLocation});
        }

        private static Drawing LoadImage(string resource)
        {
            return (Drawing) Application.Current.Styles.FindResource(resource);
        }

        #endregion

        private FlutnetProjectSettings BuildSettings()
        {
            FlutnetProjectSettings settings = new FlutnetProjectSettings
            {
                ProjectName = OutputProjectName,
                SolutionName = OutputSolutionName,
                Location = Location,
                CreateFlutterSubfolder = CreateFlutterSubfolder,
                FlutterModuleName = OutputFlutterModuleName,
                FlutterPackageName = OutputFlutterPackageName,
                FlutterVersion = FlutterVersion
            };
            return settings;
        }
    }
}