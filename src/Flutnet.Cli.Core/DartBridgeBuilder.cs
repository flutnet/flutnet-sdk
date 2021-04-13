using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Flutnet.Cli.Core.Dart;
using Flutnet.Cli.Core.Exceptions;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.Core.Utilities;
using Flutnet.Data;
using Flutnet.ServiceModel;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core
{
    internal class DartBridgeBuilder
    {
        public const string DefaultPackageDescription = "Auto-generated Flutter package that acts as a proxy between Dart code and native (Xamarin) code.";

        public static void Build(Assembly assembly, string outputDir, string packageName, string packageDescription = null, bool verbose = false)
        {
            // NOTE: Parameters have been already validated!

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Load or create Flutter package if it doesn't already exist

            DartProject dartProject = InitializePackage(outputDir, packageName, packageDescription, verbose);

            // Configure and resolve package dependencies

            ConfigurePackageDependencies(dartProject, verbose);


            // Delete all the "testing" project files
            dartProject.TestFolder.Clear();


            string dartPackageName = dartProject.Name;


            Console.WriteLine("Parsing .NET assembly...");

            // Load all the assembly types: this call the assembly resolver.
            ICollection<Type> protocolTypes;

            try
            {
                protocolTypes = assembly.GetExportedTypes();
            }
            catch(Exception ex)
            {
                string error = $"Error loading the assembly. Verify that your .NET project not reference some native library.\nDetails: {ex.Message}";
                throw new Exception(error);
            }


            //-------------------------------------------------------------------------------------
            // Load all classes that need to be exported in Dart and check if everything is valid
            //-------------------------------------------------------------------------------------

            // Force to add this exception type export
            //ICollection<Type> flutterEx = new List<Type>() { typeof(flutnet.sdk.FlutterException), typeof(flutnet.sdk.FlutterRunException) };

            // Base SDK types
            ICollection<Type> flutnetSdkTypes = new List<Type>()
            {
                typeof(PlatformOperationException), 
                typeof(FlutnetException),
            };

            ICollection<Type> sysDataTypes = new List<Type>()
            {
                typeof(FlutnetErrorCode),
                typeof(EventArgs),
            };

            protocolTypes = protocolTypes.Concat(flutnetSdkTypes).Concat(sysDataTypes).Distinct().ToList();

            // Exception types
            ICollection<Type> exceptionTypes = protocolTypes.Where(t => t.IsValidPlatformOperationException()).ToList();


            // Find out if some exception have some decoration: invalid!!!
            ICollection<Type> decoratedExceptions = exceptionTypes.Where(t => t.GetCustomAttributes(typeof(PlatformDataAttribute), true).Length > 0 || 
                                                                              t.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length > 0).ToList();

            if (decoratedExceptions.Any())
            {
                string errorMessage = string.Join("\n", decoratedExceptions.Select(t => $"The type {t.FullName} is a {typeof(PlatformOperationException).Name}: cannot be decorated with annotations.").ToArray());
                throw new Exception(errorMessage);
            }


            List<Type> referencedTypesFromException = new List<Type>();
            foreach (Type type in exceptionTypes)
            {
                // Extract all the property types for each class
                List<Type> properties = DartSupport.GetAllReferencedTypes(type, protocolTypes).ToList();
                referencedTypesFromException.AddRange(properties.Where(p => p.IsValidPlatformOperationException() == false));
            }



            // Verify that not exists classes with both decorations [FlutterData] and [FlutterService]
            ICollection<Type> multipleAttributeType = protocolTypes.Where(t => (t.IsClass || t.IsEnum || t.IsInterface)
                                                                               && t.GetCustomAttributes(typeof(PlatformDataAttribute), true).Length > 0
                                                                               && t.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length > 0
                                                                               ).ToList();
            
            if (multipleAttributeType.Any())
            {
                string errorMessage = string.Join("\n", multipleAttributeType.Select(t => $"The type {t.Name} cannot have both attribute {nameof(PlatformDataAttribute)} and {nameof(PlatformServiceAttribute)}").ToArray());
                throw new Exception(errorMessage);
            }

            
            // Find all the classes marked as [FlutterData]: will be converted into Dart classes with FromJson and ToJson methods.
            ICollection<Type> dataTypes = protocolTypes.Where(t => 
                sysDataTypes.Contains(t) || // To ensure the inclusion about FlutnetErrorCode & EventArgs
                ((t.IsClass || t.IsEnum) && t.GetCustomAttributes(typeof(PlatformDataAttribute), true).Length > 0) // user declared [PlatformData]
            ).ToList();



            // Take in consideration even all the property/fields types included in this classes
            List<Type> referencedTypes = new List<Type>();
            foreach (Type type in dataTypes)
            {
                // Extrac all the property types for each class
                List<Type> properties = DartSupport.GetAllReferencedTypes(type, protocolTypes).ToList();
                referencedTypes.AddRange(properties);
            }

            // Add all the types referenced from FlutterData classes
            foreach (Type t in referencedTypes)
            {
                if (dataTypes.Contains(t) == false)
                    dataTypes.Add(t);
            }

            // Add all the types referenced from some FlutterException class
            foreach (Type et in referencedTypesFromException)
            {
                if (dataTypes.Contains(et) == false)
                    dataTypes.Add(et);
            }

            // Find all the classes or interface (even abstract) marked as [FlutterService]: all their method will be converted into Dart project.
            ICollection<Type> serviceTypes = protocolTypes
                                             .Where(t => t.IsValidPlatformService() && t.GetCustomAttributes(typeof(PlatformServiceAttribute), true).Length > 0)
                                             .ToArray();

            // Map each service with all his methods and events
            Dictionary<Type,List<MethodInfo>> serviceToMethods = new Dictionary<Type, List<MethodInfo>>();
            Dictionary<Type, List<EventInfo>> serviceToEvents = new Dictionary<Type, List<EventInfo>>();

            // Take in consideration even all the types referenced by this methods(params and return types) -> will be considered as [FlutterData]
            List<Type> methodsTypes = new List<Type>();
            List<Type>  eventArgsTypes = new List<Type>();
            foreach (Type service in serviceTypes)
            {
                #region Operations

                // Extract all the PUBLIC methods marked as "FlutterOperation"
                List<MethodInfo> flutterOperations = service.GetPlatformOperations().ToList();

                serviceToMethods.Add(service, flutterOperations);

                // Check for each platform operation the return and params class types
                foreach (MethodInfo methodInfo in flutterOperations)
                {
                    // Extract all the class properties
                    Type returnType = DartReturnType.GetNestedType(methodInfo.ReturnType);
                    if (returnType != typeof(void))
                    {
                        List<Type> properties = DartSupport.GetAllReferencedTypes(returnType, protocolTypes).ToList();
                        methodsTypes.AddRange(properties);
                        bool isCustom = protocolTypes.Contains(returnType);
                        if (isCustom)
                            methodsTypes.Add(returnType);
                    }

                    foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
                    {
                        List<Type> properties = DartSupport.GetAllReferencedTypes(paramInfo.ParameterType, protocolTypes).ToList();
                        methodsTypes.AddRange(properties);
                        bool isCustom = protocolTypes.Contains(paramInfo.ParameterType);
                        if (isCustom)
                            methodsTypes.Add(paramInfo.ParameterType);
                    }
                }

                #endregion

                #region Events

                // Exrract all the events marked as PlaftormEvent
                List<EventInfo> flutterEvents = service.GetPlatformEvents().ToList();

                serviceToEvents.Add(service, flutterEvents);

                foreach (EventInfo e in flutterEvents)
                {
                    // Get the associated EventArgs
                    Type args = e.GetPlatformEventArgs();

                    eventArgsTypes.Add(args);

                    List<Type> properties = DartSupport.GetAllReferencedTypes(args, protocolTypes).ToList();
                    eventArgsTypes.AddRange(properties);

                    /*
                    if (args.BaseType == typeof(EventArgs)) // Extend EventArgs
                    {
                                         
                    }              
                    else if(args == typeof(EventArgs)) // Empty args
                    {
                        
                    }*/

                }

                #endregion
            }

            // Add all the method types in the data types collection
            foreach (Type mt in methodsTypes)
            {
                if (dataTypes.Contains(mt) == false)
                    dataTypes.Add(mt);
            }

            // Add all the events args type
            foreach (Type argsType in eventArgsTypes)
            {
                if (dataTypes.Contains(argsType) == false)
                    dataTypes.Add(argsType);
            }

            // FlutterData
            dataTypes = dataTypes.Where(t => t.IsValidPlatformOperationException() == false).Distinct().ToList();

            // Find all data typed NOT marked with decoration
            ICollection<Type> notMarkedDataTypes = dataTypes.Where(t => 
                sysDataTypes.Contains(t) == false && // Skip check on system data type
                protocolTypes.Contains(t) && t.GetCustomAttributes(typeof(PlatformDataAttribute), true).Length <= 0).ToList();

            if (notMarkedDataTypes.Any())
            {
                string[] notMarkedTypeNames = notMarkedDataTypes.Select(t => $" - {t.FullName} not marked with [PlatformData] decoration.").ToArray();
                string errorList = string.Join("\n", notMarkedTypeNames);
                throw new Exception($"Some types may need [PlatformData] decoration: \n{errorList}\n");
            }


            // Verify that all the data types not extends somethings:
            // inheritance is not supported by the generator for flutter data.
            Type[] extendedDataTypes = dataTypes.Where(t => DartSupport.HaveInheritance(dataTypes, t)).ToArray();


            // We enable PlatformData Inheritance: the base type must be PlatformData to (or NULL)
            Type[] platformDataSubtypes = extendedDataTypes.ToArray();


            // PlatformData Super Types with no attribute
            Type[] invalidPlatformDataSuperTypes = platformDataSubtypes.Where(subT =>
                subT.BaseType != null && 
                !sysDataTypes.Contains(subT.BaseType) && // Skip system data type, like EventArgs
                subT.BaseType.GetCustomAttributes(typeof(PlatformDataAttribute), true).Length <= 0).ToArray();

            if (invalidPlatformDataSuperTypes.Any())
            {
                throw new FlutterInheritedDataException(invalidPlatformDataSuperTypes);
            }

            // Verify that all the classe marker with flutnet.sdk.FlutterData + FlutterException are compatible with Dart language
            // !!! NOTE: we check both FlutterData and FlutterException classes.
            Type[] invalidDataTypes = DartSupport.GetUnsupportedDartTypes(dataTypes.Concat(exceptionTypes).ToList()).ToArray();

            if (invalidDataTypes.Any())
            {
                throw new FlutterUnsupportedDartTypeException(invalidDataTypes);
            }

            // JOIN DATA + EXCEPTIONS + SERVICE 
            ICollection<Type> allExportedTypes = dataTypes.Concat( exceptionTypes ).Concat( serviceToMethods.Keys ).ToList();


            // Verificy that all the methods types (return and params) are supported in Dart
            List<Exception> errors = new List<Exception>();
            foreach (var srv in serviceToMethods)
            {
                Type service = srv.Key;
                List<MethodInfo> methods = srv.Value;
                List<EventInfo> events = serviceToEvents.ContainsKey(srv.Key) ? serviceToEvents[srv.Key] : new List<EventInfo>(0);

                List<string> uniqueOperationNames = new List<string>();

                // Find all unsupported flutter operation
                MethodInfo[] invalidMethods = service.GetUnsupportedPlatformOperations();

                foreach (MethodInfo method in invalidMethods)
                {
                    errors.Add(new Exception($"Invalid Method {method.Name}, for {service.Name}: cannot mark NON PUBLIC methods with {nameof(PlatformOperationAttribute)}."));
                }

                // Check if the method is supported
                foreach (MethodInfo method in methods)
                {

                    // Extract the method name: must be unique (Dart language NOT support Method Overloading)
                    string operationName = DartSupport.GetDartMethodName(method.Name);

                    if (uniqueOperationNames.Contains(operationName))
                    {
                        errors.Add(new FlutterOperationException(service, method, operationName));
                    }

                    uniqueOperationNames.Add(operationName);


                    // Each method cannot have attributes with the same name, because Dart is NOT KEY SENSITIVE
                    // Example: bool login( string Password, string password) ---> error!
                    bool duplicatedParamName = method.GetParameters().Select(p => p.Name.ToUpper()).Distinct().Count() <
                                               method.GetParameters().Length;

                    if (duplicatedParamName)
                    {

                        var duplicatedGroups = method.GetParameters().GroupBy(
                            k => k.Name.ToUpper(),
                            e => e.Name
                        );

                        IEnumerable<string> duplicatedParams = duplicatedGroups.Select(g => $"Duplicated: {g.ToArray()}\n");

                        errors.Add(new Exception($"Invalid Method {method.Name}, for {service.Name}: params cannot have the same name (NOT KEY_SENSITIVE)!\n {duplicatedParams}"));
                    }

                    Type invalidReturnType = null;

                    // Extract all the properties for the class
                    Type returnType = DartReturnType.GetNestedType(method.ReturnType);
                    if (returnType != typeof(void))
                    {
                        if (DartSupport.IsSupportedByDart(returnType, allExportedTypes) == false)
                        {
                            invalidReturnType = method.ReturnType;
                        }
                    }

                    List<ParameterInfo> invalidParameters = new List<ParameterInfo>();

                    foreach (ParameterInfo paramInfo in method.GetParameters())
                    {
                        if (DartSupport.IsSupportedByDart(paramInfo.ParameterType, allExportedTypes) == false)
                        {
                            invalidParameters.Add(paramInfo);
                        }
                    }

                    if (invalidReturnType != null || invalidParameters.Count > 0)
                    {
                        errors.Add(new FlutterOperationException(service, method, invalidParameters,invalidReturnType));
                    }

                }

                // Find all unsupported flutter operation
                EventInfo[] invalidEvents = service.GetUnsupportedPlatformEvents();

                // Check if all events are supported
                foreach (EventInfo @event in invalidEvents)
                {
                    errors.Add(new Exception($"Invalid Event {@event.Name}, for {service.Name}. [PlatformEvent] must be implemented using EventHandler or EventHandler<T> pattern!"));
                }



            }

            // Some methods to export in Dart (FlutterOperation) are not supported
            if (errors.Any())
            {
                throw new MultipleException(errors);
            }


            // All the checks ended well



            
            // ----------------------------------------
            // Convert all the C# Types in Dart types
            // ----------------------------------------


            // Generate all the classes marked with [FlutterData]
            Dictionary<Type, DartType> dartDataTypes = new Dictionary<Type, DartType>();

            foreach (Type type in dataTypes)
            {
                DartType dartType = new DartType(allExportedTypes, type, dartPackageName);
                dartDataTypes.Add(type, dartType);
            }

            // Generate all the classes marked with [FlutterService]
            Dictionary<Type, DartService> dartServiceTypes = new Dictionary<Type, DartService>();

            foreach (KeyValuePair<Type, List<MethodInfo>> srv in serviceToMethods)
            {
                Type service = srv.Key;
                List<MethodInfo> methods = srv.Value;
                List<EventInfo> events = serviceToEvents.ContainsKey(srv.Key) ? serviceToEvents[srv.Key] : new List<EventInfo>(0);
                DartService dartService = new DartService(allExportedTypes, service, methods, events, dartPackageName, SignatureTools.GetCSharpSignature);

                dartServiceTypes.Add(service, dartService);
            }


            // Genenerate all the FlutterException hinerited classes
            Dictionary<Type, DartType> flutterExceptionTypes = new Dictionary<Type, DartType>();

            foreach (Type type in exceptionTypes)
            {
                DartType dartType = new DartType(allExportedTypes, type, dartPackageName);
                flutterExceptionTypes.Add(type, dartType);
            }


            // Join all dart types for FlutterException and [FlutterData]
            ICollection<DartType> exportedDataAndExceptionType = dartDataTypes.Select(t => t.Value).Concat(flutterExceptionTypes.Select(e => e.Value)).ToList();


            


            // ---------------------------------------------------------
            // Check the number of objects that will be generated
            // ---------------------------------------------------------

            // PlatformData Count
            int platformDataCount = dartDataTypes.Count - 1; // exclude internal PlatformErrorCode;
            // PlatformOperationException Count
            int platformOperationExceptionCount = flutterExceptionTypes.Count -2; // exclude PlatformOperationException base class + internal FlutnetException
            // PlatformService Count
            int platformServiceCount = dartServiceTypes.Count;
            // PlatformOperation Count
            int platformOperationCount = dartServiceTypes.Values.Sum(s => s.Methods.Count);
            // PlatformEvent Count
            int platformEventCount = dartServiceTypes.Values.Sum(s => s.Events.Count);

            if (verbose)
            {
                Console.WriteLine($"Found {platformServiceCount} PlatformServices");
                Console.WriteLine($"Found {platformOperationCount} PlatformOperations");
                Console.WriteLine($"Found {platformEventCount} PlatformEvents");
                Console.WriteLine($"Found {platformDataCount} PlatformData");
                Console.WriteLine($"Found {platformOperationExceptionCount} PlatformOperationExceptions");
            }


            // ----------------------------------------
            // Delete all the files in the project "lib/" folder
            // ----------------------------------------

            DirectoryInfo libDir = dartProject.LibFolder;
            libDir.Clear();


            string libPath = Path.GetFullPath(libDir.FullName);


            // ----------------------------------------
            // Start generating all the project files
            // ----------------------------------------
            Console.WriteLine($"Start generating all the dart project files ...", verbose);

            // Generate all the Dart PlatformData classes
            foreach (DartType dartType in dartDataTypes.Values)
            {
                string filePath = DartSupport.GetDartFilePath(libPath, dartType);
                string folderPath = DartSupport.GetDartDirectoryPath(libPath, dartType);

                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                DartGenerator.GenerateDartTypeFile(dartType, exportedDataAndExceptionType, filePath);
            }


            // Generate all the exception
            foreach (DartType dartType in flutterExceptionTypes.Values)
            {
                string filePath = DartSupport.GetDartFilePath(libPath, dartType);
                string folderPath = DartSupport.GetDartDirectoryPath(libPath, dartType);

                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                DartGenerator.GenerateDartTypeFile(dartType, exportedDataAndExceptionType, filePath, skipCopyWith:true);
            }


            // For each FlutterService we generate:
            // - the service class
            // - all the FAKE classes that wrap RETURN and PARAMS for methods
            foreach (DartService dartSrv in dartServiceTypes.Values)
            {

                foreach (DartMethod method in dartSrv.Methods)
                {
                    // Return Object generator
                    string returnFilePath = DartSupport.GetDartFilePath(libPath, method.ReturnObj);
                    string folderReturnFilePath = DartSupport.GetDartDirectoryPath(libPath, method.ReturnObj);

                    if (Directory.Exists(folderReturnFilePath) == false)
                    {
                        Directory.CreateDirectory(folderReturnFilePath);
                    }

                    DartGenerator.GenerateDartTypeFile(method.ReturnObj, exportedDataAndExceptionType, returnFilePath, skipDynamicJson: true, skipCopyWith: true);

                    // Param Object generator
                    string paramFilePath = DartSupport.GetDartFilePath(libPath, method.ParamObj);
                    string folderParamFilePath = DartSupport.GetDartDirectoryPath(libPath, method.ParamObj);

                    if (Directory.Exists(folderParamFilePath) == false)
                    {
                        Directory.CreateDirectory(folderParamFilePath);
                    }

                    DartGenerator.GenerateDartTypeFile(method.ParamObj, exportedDataAndExceptionType, paramFilePath, skipDynamicJson: true, skipCopyWith: true);
                }


                //
                // Generate the service class
                //
                DartType dartType = dartSrv.Type;

                string filePath = DartSupport.GetDartFilePath(libPath, dartType);
                string folderPath = DartSupport.GetDartDirectoryPath(libPath, dartType);

                if (Directory.Exists(folderPath) == false)
                {
                    Directory.CreateDirectory(folderPath);
                }

                DartGenerator.GenerateDartServiceFile(dartSrv, exportedDataAndExceptionType, filePath);

            }


            // Prendo tutti i tipi relativi ai metodi dei servizi (sono stati generati solo per i metodi)
            List<DartType> allFakeDartTypes = dartServiceTypes.SelectMany(s => s.Value.Methods)
                .SelectMany(m => new[] {m.ReturnObj, m.ParamObj}).ToList();

            List<DartType> serviceDartType = dartServiceTypes.Values.Select(s=>s.Type).ToList();


            IEnumerable<DartType> indexedTypes = dartDataTypes.Values
                                                 //.Concat(allFakeDartTypes)
                                                 .Concat(serviceDartType);


            // 20201202 - skip index file to prevent class name collision
            // Generating index.dart
            //string indexPath = Path.Combine(libPath, DartFiles.IndexFileName);
            //DartGenerator.GenerateIndexFile(dartPackageName, indexedTypes, indexPath);

            // Generating bridge.dart
            string bridgePath = Path.Combine(libPath, DartFiles.BridgeFileName);
            DartGenerator.GenerateBridgeFile(bridgePath);

            // Generating converters.dart
            string converterPath = Path.Combine(libPath, DartFiles.ConvertersFileName);
            DartGenerator.GenerateConverterFile(converterPath);

            // Generating exceptions.dart
            string exceptionsPath = Path.Combine(libPath, DartFiles.ExceptionsFileName);
            DartGenerator.GenerateExportFile(flutterExceptionTypes.Values, exceptionsPath);


            // Default generation path
            string dartProjectPath = dartProject.WorkingDir.FullName;

            // Temp folder link used by flutnet to build the folder
            DirectoryInfo tempLinkedFolderInfo = null;

            bool isWindows = Utilities.OperatingSystem.IsWindows();

            // Windows ISSUES for build_runner --> PATH to LONG (create a temp mklink for the project folder)
            if (isWindows)
            {
                // Get Windows system location
                DirectoryInfo winSystemFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));

                // Like "C:\"
                string winSystemDrive = Path.GetPathRoot(winSystemFolder.FullName);

                // Folder Link JUNCTION "C:\_flutnet.tmp_"
                string tempProjectPath = Path.Combine(winSystemDrive, "_flutnet.tmp_");

                tempLinkedFolderInfo = new DirectoryInfo(tempProjectPath);

                try
                {

                    int counter = 0;

                    // Find the first free folder name like "C:\_flutnet.tmp_" or "C:\_flutnet.tmp0_"  or "C:\_flutnet.tmp1_"
                    while (tempLinkedFolderInfo.Exists && tempLinkedFolderInfo.IsSymbolic() == false)
                    {
                        tempProjectPath = Path.Combine(winSystemDrive, $"_flutnet.tmp{counter}_");
                        tempLinkedFolderInfo = new DirectoryInfo(tempProjectPath);
                        counter++;
                    }

                    if (tempLinkedFolderInfo.Exists)
                    {
                        // Delete the oldest link
                        FlutnetShell.RunCommand($"rmdir \"{tempLinkedFolderInfo.FullName}\"", Path.GetPathRoot(tempLinkedFolderInfo.FullName), verbose);
                    }

                    // Make the folder link in C:\_flutnet_\
                    Console.WriteLine($"Creating link {tempLinkedFolderInfo.FullName} for building the dart project...", verbose);

                    // Make the linked folder (JUNCTION)
                    FlutnetShell.RunCommand($"mklink /d /J \"{tempLinkedFolderInfo.FullName}\" \"{dartProject.WorkingDir.FullName}\"", winSystemDrive, verbose);

                    tempLinkedFolderInfo = new DirectoryInfo(tempLinkedFolderInfo.FullName);

                    if (tempLinkedFolderInfo.Exists == false || tempLinkedFolderInfo.IsSymbolic() == false)
                    {
                        throw new Exception($"{tempProjectPath} creation failed");
                    }

                    // ******************************************
                    // The project is changed to the symbolic link
                    // ******************************************
                    dartProjectPath = tempLinkedFolderInfo.FullName;

                }
                catch (Exception)
                {
                    
                    Console.WriteLine($"{tempProjectPath} creation failed: use default location {dartProject.WorkingDir.FullName}", verbose);
                    
                    dartProjectPath = dartProject.WorkingDir.FullName;

                }

            }


            Console.WriteLine($"End of generation procedure in progress ...", verbose);

            int buildRunnerErrorCount = 0;
            RetryBuildRunner:

            //
            // Run flutter build_runner command to generate "fromjson" e "tojson" methods.
            //
            try
            {
                FlutterTools.BuildBuildRunner(dartProjectPath, buildRunnerErrorCount > 0, verbose);
            }
            catch (Exception)
            {
                buildRunnerErrorCount++;

                // The runner build command can fail if the dart pacgake are not
                // update correctry, so retry after that
                if (buildRunnerErrorCount <= 1)
                {
                    //Console.WriteLine($"Clean flutter project ...", verbose);
                    FlutterTools.Clean(dartProjectPath);
                    //Console.WriteLine($"Execute flutter pub upgrade ...", verbose);
                    FlutterTools.PubUpgrade(dartProjectPath, verbose);
                    Console.WriteLine($"flutter pub run build_runner build failed. Retrying updating the packages...", verbose);
                    //Console.WriteLine($"Get flutter packages ...", verbose);
                    FlutterTools.GetDependencies(dartProjectPath);
                    goto RetryBuildRunner;
                }

                throw;

            }

            // Windows ISSUES for build_runner --> PATH to LONG (create a temp mklink for the project folder)
            if (isWindows && tempLinkedFolderInfo != null && tempLinkedFolderInfo.Exists && tempLinkedFolderInfo.IsSymbolic())
            {
                // Delete the link
                Console.WriteLine($"Deleting link {tempLinkedFolderInfo.FullName} ...", verbose);
                FlutnetShell.RunCommand($"rmdir \"{tempLinkedFolderInfo.FullName}\"", Path.GetPathRoot(tempLinkedFolderInfo.FullName), verbose);
            }



            //
            // MODIFING AUTO-GENERATED PART G. FILE (JSON) replacing fromJson -> fromJsonDynamic and toJson -> toJsonDinamic
            //

            // MODIFING AUTO-GENERATED PART G. FILE PLATFORM-DATA
            foreach (DartType dartType in dartDataTypes.Values)
            {
                string dartPartialFilePath = DartSupport.GetDartPartialFilePath(libPath, dartType);
                DartGenerator.FixDartGPartedJsonFile(dartType, dartPartialFilePath);
            }

            // MODIFING AUTO-GENERATED PART G. FILE EXCEPTIONS
            foreach (DartType dartType in flutterExceptionTypes.Values)
            {
                string dartPartialFilePath = DartSupport.GetDartPartialFilePath(libPath, dartType);
                DartGenerator.FixDartGPartedJsonFile(dartType, dartPartialFilePath);
            }

            // MODIFING AUTO-GENERATED PART G. FILE CMD/RES fake classes
            foreach (DartService dartSrv in dartServiceTypes.Values)
            {
                foreach (DartMethod method in dartSrv.Methods)
                {
                    // Return Object generator
                    string returnPartialFilePath = DartSupport.GetDartPartialFilePath(libPath, method.ReturnObj);
                    DartGenerator.FixDartGPartedJsonFile(method.ReturnObj, returnPartialFilePath);

                    // Param Object generator
                    string paramPartialFilePath = DartSupport.GetDartPartialFilePath(libPath, method.ParamObj);
                    DartGenerator.FixDartGPartedJsonFile(method.ParamObj, paramPartialFilePath);
                }
            }



            Console.WriteLine($"Flutter package \"{dartProject.WorkingDir.FullName}\" generated/updated successfully.");

        }

        private static DartProject InitializePackage(string outputDir, string packageName, string packageDescription = null, bool verbose = false)
        {
            // Load or create Flutter package if it doesn't already exist

            string packageFolder = Path.Combine(outputDir, packageName);
            DartProject dartProject = new DartProject(new DirectoryInfo(packageFolder));
            try
            {
                dartProject.Load();
                if (verbose)
                {
                    Console.WriteLine($"Flutter package \"{packageFolder}\" loaded successfully.");
                    Console.WriteLine();
                }
            }
            catch (DartProjectException ex)
            {
                switch (ex.Error)
                {
                    case DartProjectError.PrjFolderNotExists:
                        Console.Write($"Generating Flutter package \"{packageName}\" within directory {outputDir}... ");
                        dartProject = FlutterTools.CreatePackage(outputDir, packageName, !string.IsNullOrEmpty(packageDescription) ? packageDescription : DefaultPackageDescription, verbose);
                        Console.Write("Done.");
                        Console.WriteLine();
                        break;

                    case DartProjectError.LibFolderNotExists:
                    case DartProjectError.PubspecNotExists:
                    case DartProjectError.PubspecInvalidFormat:
                        throw;
                }
            }

            return dartProject;
        }

        private static void ConfigurePackageDependencies(DartProject prj, bool verbose = false, bool resetDefaultVersions = false)
        {
            if (verbose)
                Console.WriteLine("Configuring package dependencies...");
            else
                Console.Write("Configuring package dependencies... ");

            /*
             * dependencies:
                flutter:
                  sdk: flutter
                # Your other regular dependencies here
                json_annotation: ^3.0.1
                
                # Dependency for the project
                intl: ^0.15.7
                web_socket_channel: ^1.1.0
                synchronized: ^2.2.0+2

              */

            List<DartProjectDependency> dependencies = new List<DartProjectDependency>();

            dependencies.Add(new DartProjectDependency("json_annotation", DartProjectDependencyType.Version, "^3.0.1"));
            dependencies.Add(new DartProjectDependency("intl", DartProjectDependencyType.Version, "^0.15.7"));
            dependencies.Add(new DartProjectDependency("web_socket_channel", DartProjectDependencyType.Version, "^1.1.0"));
            dependencies.Add(new DartProjectDependency("synchronized", DartProjectDependencyType.Version, "^2.2.0+2"));

            // Add the dependencies to the project
            dependencies.ForEach(d =>
            {
                if (resetDefaultVersions)
                {
                    prj.AddDependency(d, true);
                }
                else
                {
                    // Override the dependency version only if it's greater than the current one defined inside pubspec
                    bool overrideVersion = true;
                    try
                    {
                        DartProjectDependency currentDependency = prj.GetCurrentDependency(d);
                        if (currentDependency != null && currentDependency.Value.ToNetVersion().CompareTo(d.Value.ToNetVersion()) > 0)
                        {
                            overrideVersion = false;
                        }
                    }
                    finally
                    {
                        prj.AddDependency(d, overrideVersion);
                    }
                }
            });

            /* 
               dev_dependencies:
                build_runner: ^1.10.0
                json_serializable: ^3.3.0
             */

            List<DartProjectDependency> devDependencies = new List<DartProjectDependency>();

            devDependencies.Add(new DartProjectDependency("build_runner", DartProjectDependencyType.Version, "^1.10.0"));
            devDependencies.Add(new DartProjectDependency("json_serializable", DartProjectDependencyType.Version, "^3.3.0"));

            // Add the DEV dependencies to the project
            devDependencies.ForEach(d =>
            {
                if (resetDefaultVersions)
                {
                    prj.AddDevDependency(d, true);
                }
                else
                {
                    // Override the DEV dependency version only if it's greater than the current one defined inside pubspec
                    bool overrideVersion = true;
                    try
                    {
                        DartProjectDependency currentDependency = prj.GetCurrentDevDependency(d);
                        if (currentDependency != null && currentDependency.Value.ToNetVersion().CompareTo(d.Value.ToNetVersion()) > 0)
                        {
                            overrideVersion = false;
                        }
                    }
                    finally
                    {
                        prj.AddDevDependency(d, overrideVersion);
                    }
                }
            });

            // Set Dart SDK version
            if (verbose)
                Console.WriteLine("Configuring Dart SDK version...");
            prj.SetEnvironmentSdk(">=2.6.0 <3.0.0");

            // Save changes to file 'pubspec.yaml' and run 'flutter pub get' if required
            bool changed = prj.ApplyChanges();
            if (changed)
            {
                if (verbose)
                    Console.WriteLine("Resolving all dependencies...");
                FlutterTools.GetDependencies(prj.WorkingDir.FullName, verbose);
            }

            if (verbose)
                Console.WriteLine("Done.");
            else
                Console.Write("Done.");
            Console.WriteLine();
        }
    }
}