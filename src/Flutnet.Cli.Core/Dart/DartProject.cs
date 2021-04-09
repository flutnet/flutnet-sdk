using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.Core.Utilities;
using SharpYaml.Serialization;
using Path = System.IO.Path;
using TextWriter = System.IO.TextWriter;
using YamlDocument = SharpYaml.Serialization.YamlDocument;
using YamlNode = SharpYaml.Serialization.YamlNode;
using YamlStream = SharpYaml.Serialization.YamlStream;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartProject
    {

        // String used to serapate folders in dart package
        internal static char DartSeparator = '/';

        // pubspec file
        private FileInfo _pubspecFile;
        private YamlStream _pubspecStream;
        private YamlDocument _pubspecDocument;

        private FileInfo _metadataFile;
        private YamlStream _metadataStream;
        private YamlDocument _metadataDocument;


        readonly object _loadLock = new object();

        // Dart prj name;
        private string _name;

        public string Name
        {
            get
            {
                lock (_loadLock)
                {
                    return _name ?? string.Empty;
                }
            }
        }

        // Dart prj description
        private string _description;

        public string Description
        {
            get
            {
                lock (_loadLock)
                {
                    return _description ?? string.Empty;
                }
            }
        }

        // Dart prj organization
        private string _author;

        public string Author
        {
            get
            {
                lock (_loadLock)
                {
                    return _author ?? string.Empty;
                }
            }
        }


        private string _homepage;

        public string Homepage
        {
            get
            {
                lock (_loadLock)
                {
                    return _homepage ?? string.Empty;
                }
            }
        }


        private string _version;

        public string Version
        {
            get
            {
                lock (_loadLock)
                {
                    return _version ?? string.Empty;
                }
            }
        }


        DirectoryInfo _libFolder;
        
        /// <summary>
        /// Lib folder for the dart project.
        /// </summary>
        public DirectoryInfo LibFolder
        {
            get
            {
                lock (_loadLock)
                {
                    return _libFolder;
                }
            }
        }

        DirectoryInfo _testFolder;

        public DirectoryInfo TestFolder
        {
            get
            {
                lock (_loadLock)
                {
                    return _testFolder;
                }
            }
        }


        private string _sdk;

        public string Sdk
        {
            get
            {
                lock (_loadLock)
                {
                    return _sdk ?? string.Empty;
                }
            }
        }

        DartProjectType? _type;
        public DartProjectType Type
        {
            get
            {
                if (_type == null)
                {
                    throw new Exception("You must load the project before get all the property");
                }

                return _type.Value;
            }
        }

        /// <summary>
        /// Returns the path of the Android AAR created when building a Flutter module
        /// and used for integrating Flutter into a native Android app.
        /// </summary>
        public string GetAndroidArchivePath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (_type)
            {
                case DartProjectType.Module:
                    return GetAndroidArchivePath(WorkingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }
        }


        /// <summary>
        /// Returns the path of the iOS XCFramework created when building a Flutter 2.* module
        /// and used for integrating Flutter into a native iOS app.
        /// </summary>
        public string GetIosXCFrameworkPath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (Type)
            {
                case DartProjectType.Module:
                    return GetIosXCFrameworkPath(WorkingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }

        }

        /// <summary>
        /// Returns the path of the iOS Framework created when building a Flutter 1.* module
        /// and used for integrating Flutter into a native iOS app.
        /// </summary>
        public string GetIosFrameworkPath(FlutterModuleBuildConfig buildConfig)
        {
            Load();
            switch (Type)
            {
                case DartProjectType.Module:
                    return GetIosFrameworkPath(WorkingDir.FullName, _pubspecDocument, buildConfig);
                default:
                    throw new InvalidOperationException("This method is designed for Flutter modules only.");
            }

        }

        // Dart dependencies
        List<DartProjectDependency> _dependencies;

        public List<DartProjectDependency> Dependencies
        {
            get
            {
                lock (_loadLock)
                {
                    return _dependencies ?? new List<DartProjectDependency>(0);
                }
            }
        }

        // Dart dev_dependencies
        List<DartProjectDependency> _devDependencies;

        public List<DartProjectDependency> DevDependencies
        {
            get
            {
                lock (_loadLock)
                {
                    return _devDependencies ?? new List<DartProjectDependency>(0);
                }
            }
        }

        // The project working dir
        public readonly DirectoryInfo WorkingDir;

        public DartProject(DirectoryInfo workingDir)
        {
            WorkingDir = workingDir;
        }

        /// <summary>
        /// Load dart project YAML file.
        /// </summary>
        /// <exception cref = "DartProjectException" ></exception>
        public void Load()
        {
            lock (_loadLock)
            {
                if (_isLoad == false)
                {
                    _loadProjectFolders(WorkingDir);
                    _loadMetadata();
                    _loadPubspecYaml();
                    _loadAllProjectInformation();
                    _isLoad = true;
                }
            }
        }

        bool _isLoad = false;
        bool IsLoad
        {
            get
            {
                lock (_loadLock)
                {
                    return _isLoad;
                }
            }
            set
            {
                lock (_loadLock)
                {
                    _isLoad = value;
                }
            }
        }

        void _loadProjectFolders(DirectoryInfo dartPrjDirectory)
        {

            if (dartPrjDirectory.Exists == false)
            {
                throw new DartProjectException("The dart prj directory not exists!", DartProjectError.PrjFolderNotExists);
            }

            // Check if the directory is real flutter prj: exists "pubspac.yaml" file
            //FileInfo[] yamlFiles = dartPrjDirectory.GetFiles("*.yaml");
            FileInfo[] yamlFiles = dartPrjDirectory.GetFiles("pubspec.yaml");

            if (yamlFiles.Length == 0)
            {
                throw new DartProjectException("The directory is not a flutter prj. \"pubspec.yaml\" FILE not found!!!", 
                    DartProjectError.PubspecNotExists);
            }
            if (yamlFiles.Length != 1)
            {
                // TODO Warning that this prj is strange. Only 1 pubspec.yaml shoud exists!!
            }

            //
            // Load the pubspec.yaml file
            //
            _pubspecFile = yamlFiles[0];


            FileInfo[] metadataFiles = dartPrjDirectory.GetFiles(".metadata");

            if (metadataFiles.Length == 0)
            {
                throw new DartProjectException("The directory is not a flutter prj. \".metadata\" FILE not found!!!",
                    DartProjectError.MetadataFileNotExists);
            }
            if (metadataFiles.Length != 1)
            {
                // TODO Warning that this prj is strange. Only 1 .metadata shoud exists!!
            }

            //
            // Load the .metadata file
            //
            _metadataFile = metadataFiles[0];


            // Find lib folder
            DirectoryInfo[] libDirs = dartPrjDirectory.GetDirectories("lib");
            if (libDirs.Length == 0)
            {
                throw new DartProjectException("The directory is not a dart prj. \"lib\" FOLDER not found!!!",
                    DartProjectError.LibFolderNotExists);
            }
            if (libDirs.Length != 1)
            {
                // TODO Warning that this prj is strange. Only 1 lib folder shoud exists!!
            }

            _libFolder = libDirs[0];


            // Find test folder
            DirectoryInfo[] testDirs = dartPrjDirectory.GetDirectories("test");

            if (testDirs.Length >= 1)
            {
                _testFolder = testDirs[0];
            }
            else
            {
                string testFolderPath = Path.Combine(dartPrjDirectory.FullName,"test") ;
                if (Directory.Exists(testFolderPath) == false)
                {
                    _testFolder = Directory.CreateDirectory(testFolderPath);
                }
            }

        }

        void _loadMetadata()
        {
            // YAML parser
            _metadataStream = new YamlStream();

            using (var reader = _metadataFile.OpenText())
            {
                _metadataStream.Load(reader);
            }

            /*
               # This file tracks properties of this Flutter project.
               # Used by Flutter tool to assess capabilities and perform upgrades etc.
               #
               # This file should be version controlled and should not be manually edited.
               
               version:
               revision: 27321ebbad34b0a3fafe99fac037102196d655ff
               channel: stable
               
               project_type: module
            */

            if (_metadataStream.Documents.Count != 1)
            {
                throw new DartProjectException("Invalid .metadata structure!!", DartProjectError.MetadataInvalidFormat);
            }

            _metadataDocument = _metadataStream.Documents[0];

        }

        void _loadPubspecYaml()
        {
            // YAML parser
            _pubspecStream = new YamlStream();
            
            using (var reader = _pubspecFile.OpenText())
            {
                _pubspecStream.Load(reader);
            }

            /*
             * name: flutter_xamarin_protocol
               description: A new Flutter package project.
               version: 0.0.1
               author:
               homepage:
               
               environment:
               sdk: ">=2.1.0 <3.0.0"
               
               dependencies:
               flutter:
               sdk: flutter
               # Your other regular dependencies here
               json_annotation: ^2.0.0
               
               dev_dependencies:
               flutter_test:
               sdk: flutter
               # Your other dev_dependencies here
               build_runner: ^1.0.0
               json_serializable: ^2.0.0
             */


            if (_pubspecStream.Documents.Count != 1)
            {
                throw new DartProjectException("Invalid YAML structure!!", DartProjectError.PubspecInvalidFormat);
            }

            _pubspecDocument = _pubspecStream.Documents[0];

        }

        void _loadAllProjectInformation()
        {

            // Load all the info in the metadata file
            string projectType = _metadataDocument.GetScalarValue(new[] { "project_type" });

            switch (projectType)
            {
                case "app":
                    _type = DartProjectType.App;
                    break;
                case "module":
                    _type = DartProjectType.Module;
                    break;
                case "package":
                    _type = DartProjectType.Package;
                    break;
                default:
                    throw new ArgumentException("Unrecognized dart project type:");
            }

            // Check che correctness for the pubspec yaml document
            CheckPubspecDocument(_pubspecDocument, _type.Value);

            _name = _pubspecDocument.GetScalarValue(new[] { _nameYaml });
            _description = _pubspecDocument.GetScalarValue(new[] { _descriptionYaml });
            _author = _pubspecDocument.GetScalarValue(new[] { _authorYaml });
            _homepage = _pubspecDocument.GetScalarValue(new[] { _homepageYaml });
            _version = _pubspecDocument.GetScalarValue(new[] { _versionYaml });

            _dependencies = GetDependencies(_pubspecDocument);
            _devDependencies = GetDevDependencies(_pubspecDocument);
            _sdk = _pubspecDocument.GetScalarValue(new[] { _environmentYaml, _sdkYaml });

        }

        bool _isDirty = false;
        bool IsDirty
        {
            get
            {
                lock (_loadLock)
                {
                    return _isDirty;
                }
            }
            set
            {
                lock (_loadLock)
                {
                    _isDirty = value;
                }
            }
        }

        /// <summary>
        /// Add a dart dependecy into the project.
        /// To apply the depecency call 
        /// </summary>
        /// <param name="dependency"></param>
        /// <param name="overrideVersion"></param>
        /// <exception cref = "DartProjectException" ></exception>
        public void AddDependency(DartProjectDependency dependency, bool overrideVersion=false)
        {
            if (IsLoad == false)
            {
                Load();
            }

            DartProjectDependency existingDependency = _dependencies.FirstOrDefault(d => d.Name == dependency.Name);

            if (existingDependency == null)
            {
                _dependencies.Add(dependency);
                AddDependencyToDoc(_pubspecDocument,dependency);
                IsDirty = true;
            }
            else if (overrideVersion && (existingDependency.Type != dependency.Type || existingDependency.Value != dependency.Value))
            {
                // Replace the different version
                _dependencies.Remove(existingDependency);
                _dependencies.Add(dependency);
                AddDependencyToDoc(_pubspecDocument, dependency, true);
                IsDirty = true;
            }
        }

        /// <summary>
        /// Find the current Dependency Configured in the project with the same name and type.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public DartProjectDependency GetCurrentDependency(DartProjectDependency dependency)
        {
            if (IsLoad == false)
            {
                Load();
            }

            DartProjectDependency existingDependency = _dependencies.FirstOrDefault(d => d.Name == dependency.Name && d.Type == dependency.Type);

            return existingDependency;
        }

        /// <summary>
        /// Find the current Dev Dependency Configured in the project with the same name and type.
        /// </summary>
        /// <param name="devDependency"></param>
        /// <returns></returns>
        public DartProjectDependency GetCurrentDevDependency(DartProjectDependency devDependency)
        {
            if (IsLoad == false)
            {
                Load();
            }

            DartProjectDependency existingDependency = _devDependencies.FirstOrDefault(d => d.Name == devDependency.Name && devDependency.Type == d.Type);

            return existingDependency;
        }

        /// <summary>
        /// Add a dart dev_dependecy into the project.
        /// To apply the depecency call 
        /// </summary>
        /// <param name="dependency"></param>
        /// <param name="overrideVersion"></param>
        /// <exception cref = "DartProjectException" ></exception>
        public void AddDevDependency(DartProjectDependency dependency, bool overrideVersion= false)
        {
            if (IsLoad == false)
            {
                Load();
            }

            DartProjectDependency existingDependency = _devDependencies.FirstOrDefault(d => d.Name == dependency.Name);

            if (existingDependency == null)
            {
                _devDependencies.Add(dependency);
                AddDevDependencyToDoc(_pubspecDocument, dependency);
                IsDirty = true;
            }
            else if (overrideVersion && (existingDependency.Type != dependency.Type || existingDependency.Value != dependency.Value))
            {
                // Replace the different version
                _devDependencies.Remove(existingDependency);
                _devDependencies.Add(dependency);
                AddDevDependencyToDoc(_pubspecDocument, dependency, true);
                IsDirty = true;
            }
        }

        /// <summary>
        /// Set the new enviroment sdk for this Dart Project.
        /// </summary>
        /// <param name="sdkValue"></param>
        public void SetEnvironmentSdk(string sdkValue)
        {
            if (IsLoad == false)
            {
                Load();
            }

            if (_sdk != sdkValue)
            {
                _pubspecDocument.SetScalarValue(new[] { _environmentYaml, _sdkYaml }, sdkValue);
                _sdk = _pubspecDocument.GetScalarValue(new []{_environmentYaml,_sdkYaml});
                IsDirty = true;
            }

        }

        /// <summary>
        /// Apply the changes to the pubspec.yaml file.
        /// </summary>
        /// <exception cref="Exception"> Error during pubspec.yaml modification</exception>
        public bool ApplyChanges()
        {
            if(IsDirty == false || IsLoad == false)
            {
                return false; // Nothing to apply
            }

            FileInfo[]tmpsFiles = WorkingDir.GetFiles("*.tmp");
            foreach (FileInfo tmp in tmpsFiles)
            {
                tmp.Delete();
            }

            string suffix = Guid.NewGuid() + ".tmp";
            string tmpPath = _pubspecFile.FullName + suffix;

            using (TextWriter writer = File.CreateText(tmpPath))
            {
                _pubspecStream.Save(writer);
            }

            string yamlPath = _pubspecFile.FullName;


            int errorCount = 0;

            while (true)
            {
                try
                {
                    // Delete the existing file if exists
                    _pubspecFile.Delete();

                    // Rename the oldFileName into newFileName
                    File.Move(tmpPath, yamlPath);

                    _pubspecFile = new FileInfo(yamlPath);

                    IsDirty = false;

                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;

                    if (errorCount > 3)
                        throw ex;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets all the dependencies listed in the pubspec.yaml file of this Dart project,
        /// as well as their transitive dependencies.
        /// </summary>
        public void GetDependencies(bool verbose = false)
        {
            FlutterTools.GetDependencies(WorkingDir.FullName, verbose);
        }

        #region YAML Utility for Dart Project

        // All the yaml node present in a Dart Project
        static readonly string _nameYaml = "name";
        static readonly string _descriptionYaml = "description";
        static readonly string _authorYaml = "author";
        static readonly string _homepageYaml = "homepage";
        static readonly string _versionYaml = "version";


        static readonly string _dependeciesYaml = "dependencies";
        static readonly string _devDependeciesYaml = "dev_dependencies";
        //static readonly string _pathYaml = "path";

        static readonly string _environmentYaml = "environment";
        static readonly string _sdkYaml = "sdk";

        static void CheckPubspecDocument(YamlDocument doc, DartProjectType type)
        {

            // Exists "name"
            YamlScalarNode name = doc.GetScalarNode(new[] { _nameYaml });
            if (name == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_nameYaml}' !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "description"
            YamlScalarNode description = doc.GetScalarNode(new[] { _descriptionYaml });
            if (description == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_descriptionYaml}' !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "author"
            YamlScalarNode author = doc.GetScalarNode(new[] { _authorYaml });
            if (author == null && type == DartProjectType.Package)
            {
                throw new DartProjectException($"Dart project error: missing '{_authorYaml}' !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "homepage"
            YamlScalarNode homepage = doc.GetScalarNode( new[] { _homepageYaml });
            if (homepage == null && type == DartProjectType.Package)
            {
                throw new DartProjectException($"Dart project error: missing '{_homepageYaml}' !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "version"
            YamlScalarNode version = doc.GetScalarNode( new[] { _versionYaml });
            if (version == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_versionYaml}' !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "dependencies"
            YamlMappingNode dependencies = doc.GetMappingNode( new[] { _dependeciesYaml });
            if (dependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_dependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "dev_dependencies"
            YamlMappingNode devDependencies = doc.GetMappingNode( new[] { _devDependeciesYaml });
            if (devDependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_devDependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "environment"
            YamlMappingNode environment = doc.GetMappingNode(new[] { _environmentYaml });
            if (environment == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_environmentYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            // Exists "sdk"
            YamlScalarNode sdk = doc.GetScalarNode(new[] { _environmentYaml, _sdkYaml });
            if (sdk == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_sdkYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }


        }

        static List<DartProjectDependency> GetDependencies(YamlDocument doc)
        {

            // ROOT DOCUMENT
            YamlMappingNode dependencies = doc.GetMappingNode(new[]{ _dependeciesYaml });
            if (dependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_dependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            List<DartProjectDependency> dartDependencies = new List<DartProjectDependency>();

            foreach (KeyValuePair<YamlNode, YamlNode> dependency in dependencies)
            {
                DartProjectDependency d = DartProjectDependencyExtension.FromYamlNode(dependency);

                if (d != null)
                {
                    dartDependencies.Add(d);
                }
            }

            return dartDependencies;

        }

        static void AddDependencyToDoc(YamlDocument doc, DartProjectDependency dependency, bool forceUpdate = false)
        {

            YamlMappingNode dependencies = doc.GetMappingNode(new[] {_dependeciesYaml});

            if (dependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_dependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            var dependecyNode = dependency.ToYamlNode();

            if (dependencies.Children.ContainsKey(dependecyNode.Key) == false || forceUpdate)
            {
                dependencies.Children[dependecyNode.Key] = dependecyNode.Value;
            }

        }

        static void AddDevDependencyToDoc(YamlDocument doc, DartProjectDependency dependency, bool forceUpdate = false)
        {

            YamlMappingNode devDependencies = doc.GetMappingNode(new[] { _devDependeciesYaml });

            if (devDependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_devDependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            var dependecyNode = dependency.ToYamlNode();

            if (devDependencies.Children.ContainsKey(dependecyNode.Key) == false || forceUpdate)
            {
                devDependencies.Children[dependecyNode.Key] = dependecyNode.Value;
            }

        }

        static List<DartProjectDependency> GetDevDependencies(YamlDocument doc)
        {

            YamlMappingNode dependencies = doc.GetMappingNode(new[] { _devDependeciesYaml });
            if (dependencies == null)
            {
                throw new DartProjectException($"Dart project error: missing '{_devDependeciesYaml}' section !!!", DartProjectError.PubspecInvalidFormat);
            }

            List<DartProjectDependency> dartDependencies = new List<DartProjectDependency>();

            foreach (KeyValuePair<YamlNode, YamlNode> dependency in dependencies)
            {
                DartProjectDependency d = DartProjectDependencyExtension.FromYamlNode(dependency);

                if (d != null)
                {
                    dartDependencies.Add(d);
                }
            }

            return dartDependencies;

        }


        #endregion

        #region Flutter Module utilities

        /// <summary>
        /// Returns the path of the Android AAR created when building a Flutter module.
        /// </summary>
        public static string GetAndroidArchivePath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            // Please consider a Flutter module created with the following command:
            //
            // flutter create -t module --org com.example hello_world
            //
            // Now, build the module for Android integration:
            //
            // flutter build aar
            //
            // The output AAR for Debug configuration is located under:
            //
            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\flutter_debug\1.0\flutter_debug-1.0.aar

            // <MODULE_FOLDER>\build\host\outputs\repo\
            string repoRootFolder = Path.Combine(flutterFolder, "build", "host", "outputs", "repo");

            // Find the 'androidPackage' info from the pubspec.yaml file
            string package = pubspecFile.GetScalarValue(new[] { "flutter", "module", "androidPackage" });

            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\
            string packageRootFolder = repoRootFolder;
            string[] packageFolders = string.IsNullOrEmpty(package) ? new string[0] : package.Split(".");
            foreach (string folder in packageFolders)
            {
                packageRootFolder = Path.Combine(packageRootFolder, folder);
            }

            string version = "1.0";
            string build;
            switch (buildConfig)
            {
                case FlutterModuleBuildConfig.Debug:
                    build = "debug";
                    break;
                case FlutterModuleBuildConfig.Profile:
                    build = "profile";
                    break;
                case FlutterModuleBuildConfig.Release:
                    build = "release";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildConfig), buildConfig, null);
            }

            // <MODULE_FOLDER>\build\host\outputs\repo\com\example\hello_world\flutter_debug\1.0\flutter_debug-1.0.aar
            string path = Path.Combine(packageRootFolder, $"flutter_{build}", version, $"flutter_{build}-{version}.aar");
            return path;
        }

        /// <summary>
        /// Returns the path of the iOS XCFramework created when building a Flutter 2.* module with Flutter.
        /// </summary>
        public static string GetIosXCFrameworkPath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            return GetIosFrameworkPathCore(flutterFolder, pubspecFile, buildConfig, true);
        }

        /// <summary>
        /// Returns the path of the iOS Framework created when building a Flutter 1.* module with Flutter.
        /// </summary>
        public static string GetIosFrameworkPath(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig)
        {
            return GetIosFrameworkPathCore(flutterFolder, pubspecFile, buildConfig, false);
        }

        private static string GetIosFrameworkPathCore(string flutterFolder, YamlDocument pubspecFile, FlutterModuleBuildConfig buildConfig, bool xcframework)
        {
            // Please consider a Flutter module created with the following command:
            //
            // flutter create -t module --org com.example hello_world
            //
            // Now, build the module for iOS integration:
            //
            // flutter build ios-framework
            //
            // The output framework for Debug configuration is located under:
            //
            // <MODULE_FOLDER>\build\ios\Debug\App.xcframework (Flutter 2.*)
            //
            // or
            //
            // <MODULE_FOLDER>\build\ios\Debug\App.framework   (Flutter 1.*)


            // <MODULE_FOLDER>\build\ios\framework\
            string frameworkRootFolder = Path.Combine(flutterFolder, "build", "ios", "framework");

            // Find the 'iosBundleIdentifier' info from the pubspec.yaml file
            string bundle = pubspecFile.GetScalarValue(new[] { "flutter", "module", "iosBundleIdentifier" });

            string build;
            switch (buildConfig)
            {
                case FlutterModuleBuildConfig.Debug:
                    build = "Debug";
                    break;
                case FlutterModuleBuildConfig.Profile:
                    build = "Profile";
                    break;
                case FlutterModuleBuildConfig.Release:
                    build = "Release";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buildConfig), buildConfig, null);
            }

            // <MODULE_FOLDER>\build\ios\Debug\App.framework
            string path = Path.Combine(frameworkRootFolder, build, xcframework ? "App.xcframework" : "App.framework");
            return path;
        }

        #endregion
    }

    internal static class DartProjectDependencyExtension
    {

        /*
         * Il formato delle dipendenze del file pubspec.yaml
         *
         *  node_dipendenza: versione
         *  node_dipendenza:
         *    path: 'path/dipendenza/progetto'
         *  nome_dipendenza:
         *    sdk: nome_sdk
         *
         *
           dependencies:
             flutter:
              sdk: flutter
           dev_dependencies:
             flutter_test:
               sdk: flutter
             build_runner: ^1.7.3
             json_serializable: ^3.2.5
             my_library:
               path: '..\..\..\my_library'
         *
         */

        public static KeyValuePair<YamlNode, YamlNode> ToYamlNode(this DartProjectDependency dependency)
        {
            
            YamlScalarNode key = new YamlScalarNode(dependency.Name);

            YamlNode value = null;
            switch (dependency.Type)
            {
                case DartProjectDependencyType.Version:
                    value = new YamlScalarNode(dependency.Value);
                    break;
                case DartProjectDependencyType.Path:
                    YamlMappingNode pathNode = new YamlMappingNode();
                    pathNode.Children[new YamlScalarNode("path")] = new YamlScalarNode(dependency.Value);
                    value = pathNode;
                    break;
                case DartProjectDependencyType.Sdk:
                    YamlMappingNode sdkNode = new YamlMappingNode();
                    sdkNode.Children[new YamlScalarNode("sdk")] = new YamlScalarNode(dependency.Value);
                    value = sdkNode;
                    break;
                default:
                    throw  new ArgumentException();
            }

            return new KeyValuePair<YamlNode, YamlNode>(key,value);

        }

        public static DartProjectDependency FromYamlNode(KeyValuePair<YamlNode, YamlNode> node)
        {
            if (node.Key is YamlScalarNode && node.Value is YamlScalarNode)
            {
                string name = ((YamlScalarNode)node.Key).Value;
                string version = ((YamlScalarNode)node.Value).Value;
                return new DartProjectDependency(name, DartProjectDependencyType.Version, version);
            }
            if (node.Key is YamlScalarNode && node.Value is YamlMappingNode)
            {
                string name = ((YamlScalarNode)node.Key).Value;
                YamlMappingNode nestedNode = ((YamlMappingNode)node.Value);

                YamlScalarNode pathNode = nestedNode.GetScalarNode(new[] { "path" });
                if (pathNode != null)
                {
                    string path = pathNode.Value;
                    return new DartProjectDependency(name, DartProjectDependencyType.Path, path);
                }

                YamlScalarNode sdkNode = nestedNode.GetScalarNode(new[] { "sdk" });
                if (sdkNode != null)
                {
                    string sdk = sdkNode.Value;
                    return new DartProjectDependency(name, DartProjectDependencyType.Sdk, sdk);
                }

            }

            return null;
        }

    }

}
