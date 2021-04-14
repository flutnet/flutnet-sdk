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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Flutnet.Cli.Core.Utilities;
using Flutnet.ServiceModel;
using Flutnet.Utilities;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartGenerator
    {

        /*
         * import 'package:json_annotation/json_annotation.dart';

           /// This allows the `User` class to access private members in
           /// the generated file. The value for this is *.g.dart, where
           /// the star denotes the source file name.
           part 'user.g.dart';

           /// An annotation for the code generator to know that this class needs the
           /// JSON serialization logic to be generated.
           @JsonSerializable(nullable: false)

           class User {
           User(this.name, this.email);

           String name;
           String email;

           /// A necessary factory constructor for creating a new User instance
           /// from a map. Pass the map to the generated `_$UserFromJson()` constructor.
           /// The constructor is named after the source class, in this case, User.
           factory User.fromJson(Map<String, dynamic> json) => _$UserFromJson(json);

           /// `toJson` is the convention for a class to declare support for serialization
           /// to JSON. The implementation simply calls the private, generated
           /// helper method `_$UserToJson`.
           Map<String, dynamic> toJson() => _$UserToJson(this);
           }

         */
        /// <param name="t"></param>
        /// <param name="exportedTypes"></param>
        /// <param name="destinationPath"></param>
        /// <param name="skipDynamicJson"></param>
        /// <param name="skipCopyWith"></param>
        /// <exception cref="Exception"> Error during write process</exception>
        internal static void GenerateDartTypeFile(DartType t, ICollection<DartType> exportedTypes, string destinationPath, bool skipDynamicJson = false, bool skipCopyWith=false)
        {
            if (t.DotNetType.IsClass)
            {
                // TO String method is generated only for exceptions

                bool isException = typeof(PlatformOperationException).IsAssignableFrom(t.DotNetType);

                bool isBaseType = DartSupport.HaveInheritance(exportedTypes.Select(dt => dt.DotNetType).ToArray(), t.DotNetType) == false;
                //bool hasSubtypes = DartSupport.GetDartSubtypes(t, exportedTypes).Any();

                //bool generateDynamicJson = true;// isBaseType && hasSubtypes;

                //bool handleInheritance = t.BaseDartType != null || isBaseException;

                // Note: toString is generated only for the exception base class
                _GenerateDartClassFileWithInheritance(t, exportedTypes, destinationPath, true && !skipDynamicJson, isBaseType && !skipDynamicJson, isException, !skipCopyWith);

                /*
                if (handleInheritance)
                {
                    // Note: toString is generated only for the exception base class
                    _GenerateDartClassFileWithInheritance(t, exportedTypes, destinationPath, generateDynamicJson, isBaseException);
                }
                else
                {
                    _GenerateDartClassFile(t, destinationPath, false);
                }
                */


            }
            else if (t.DotNetType.IsEnum)
            {
                _GenerateDartEnumFile(t, destinationPath);
            }
            else
            {
                throw new Exception($"GenerateClassDataFile: invalid NET type to generate -> {t.DotNetType.FullName}");
            }
        }


        
        static void _GenerateDartClassFile(DartType t, string destinationPath, bool generateToString)
        {

            bool isValid = t.DotNetType.IsClass;

            if (isValid == false)
                throw new Exception($"GenerateDartClassFile: invalid NET type to generate -> {t.DotNetType.FullName}");

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");

                    sw.WriteLine(DartImports.JsonAnnotationImport);
                    sw.WriteLine(DartImports.MetaImport);

                    // Import for each property member
                    List<string> imports = new List<string>();
                    foreach (DartProperty prop in t.Members)
                    {
                        imports.AddRange(prop.Type.Imports);
                    }
                    foreach (string importLine in imports.Distinct())
                    {
                        sw.WriteLine(importLine);
                    }

                    // Import converter.dart if needed
                    if (t.Members.Any(m => m.Type.ConverterType != ConverterType.None))
                    {
                        sw.WriteLine(DartImports.ConvertersImport(t.Package));
                    }

                    // NON USO PIU IL LibMessage
                    //sw.WriteLine(DartImports.LibMessageImport(t.Package));

                    sw.WriteLine($"");
                    // Add partial class to generate

                    string partialFilename = DartSupport.GetDartPartialFilename(t.DotNetType);

                    sw.WriteLine($"part '{partialFilename}';");
                    sw.WriteLine($"");
                    sw.WriteLine($"");
                    sw.WriteLine($"/// An annotation for the code generator to know that this class needs the");
                    sw.WriteLine($"/// the star denotes the source file name.");

                    sw.WriteLine($"@immutable");

                    // Nullable parameter for json conversion
                    string isNullable = t.IsNullable ? "true" : "false";
                    sw.WriteLine($"@JsonSerializable(nullable: {isNullable}, explicitToJson: true, anyMap: true)");

                    // NON USO PIU IL LibMessage
                    //sw.WriteLine($"class {t.Name} extends {LibMessageClassName} {{");
                    sw.WriteLine($"class {t.Name} {{");

                    sw.WriteLine($"");

                    List<string> thisparams = t.Members.Select(m => $"this.{m.DartName}").ToList();

                    // User({this.name, this.email,});
                    if (thisparams.Count > 0)
                    {
                        sw.WriteLine($"\t{t.Name}({{\n\t\t{String.Join(",\n\t\t", thisparams)},\n\t}});");
                    }
                    else
                    {
                        sw.WriteLine($"\t{t.Name}();");
                    }

                    sw.WriteLine($"");

                    foreach (DartProperty prop in t.Members)
                    {

                        if (prop.Type.ConverterType != ConverterType.None)
                        {
                            sw.WriteLine($"\t@JsonKey(name: \"{prop.Name}\")");
                            string customConverterClass = DartSupport.GetConverterName(prop.Type.ConverterType);
                            sw.WriteLine($"\t{customConverterClass}");
                        }
                        else
                        {
                            string isPropNullable = prop.Type.IsNullable ? "true" : "false";
                            sw.WriteLine($"\t@JsonKey(name: \"{prop.Name}\", nullable: {isPropNullable})");
                        }

                        sw.WriteLine($"\tfinal {prop.Type.Name} {prop.DartName};");
                        sw.WriteLine($"");
                    }

                    sw.WriteLine($"");

                    // A necessary factory constructor for creating a new User instance
                    // from a map. Pass the map to the generated `_$UserFromJson()` constructor.
                    // The constructor is named after the source class, in this case, User.
                    // factory User.fromJson(Map < String, dynamic > json) => _$UserFromJson(json);
                    sw.WriteLine($"\tfactory {t.Name}.fromJson(Map<dynamic, dynamic> json) => _${t.Name}FromJson(json);");
                    sw.WriteLine($"");

                    // `toJson` is the convention for a class to declare support for serialization
                    // to JSON. The implementation simply calls the private, generated
                    // helper method `_$UserToJson`.
                    // Map<String, dynamic> toJson() => _$UserToJson(this);
                    sw.WriteLine($"\tMap<String, dynamic> toJson() => _${t.Name}ToJson(this);");
                    sw.WriteLine($"");

                    // Add toString method
                    if (generateToString)
                    {
                        sw.WriteLine(_GenerateToStringMethod(t));
                        sw.WriteLine($"");
                    }

                    sw.WriteLine($"}}");

                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }

        }


        // Example with inheritance

        /*
           import 'package:json_annotation/json_annotation.dart';
           import 'package:meta/meta.dart';
           import 'package:my_net_library/my/net/library/exceptions/media_error.dart';
           import 'package:my_net_library/my/net/library/exceptions/media_exception.dart';
           
           part 'media_exception_2.g.dart';

           @immutable
           @JsonSerializable(nullable: true, explicitToJson: true, anyMap: true)
           class MediaException2 extends MediaException {
           MediaException2({
           String message,
           this.error,
           }) : super(
           message: message,
           error: error,
           );
           
           @JsonKey(name: "Error", nullable: true)
           final MediaError error;
           
           factory MediaException2.fromJson(Map<dynamic, dynamic> json) =>
           _$MediaException2FromJson(json);
           
           Map<String, dynamic> toJson() => _$MediaException2ToJson(this);
           }

         */
        static void _GenerateDartClassFileWithInheritance(DartType t, ICollection<DartType> exportedTypes, string destinationPath, bool generateFromJsonDynamic, bool generateToJsonDynamic, bool generateToString, bool generateCopyWith)
        {

            bool isValid = t.DotNetType.IsClass;

            if (isValid == false)
                throw new Exception($"GenerateDartClassFileWithInheritance: invalid NET type to generate -> {t.DotNetType.FullName}");

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");

                    // Current file import: is not necessary for the current dart file
                    string currentTypeImport = DartSupport.DartPackageImport(t.Package, t.Namespace, t.FileName);

                    sw.WriteLine($"// IMPORT FILE: {currentTypeImport}");
                    sw.WriteLine("// *************************************");

                    List<string> imports = new List<string>();

                    imports.Add(DartImports.JsonAnnotationImport);
                    imports.Add(DartImports.MetaImport);
             

                    // Import for each property member
                    
                    foreach (DartProperty prop in t.Members)
                    {
                        if (prop.Type.DotNetType != t.DotNetType)
                        {
                            imports.AddRange(prop.Type.Imports);
                        }
                    }

                    // Add to imports the base dart type.
                    if (t.BaseDartType != null)
                    {
                        string baseTypeImport = DartSupport.DartPackageImport(t.BaseDartType.Package, t.BaseDartType.Namespace, t.BaseDartType.FileName);
                        imports.Add(baseTypeImport);
                    }
                    
                    // If generate the dynamic json method, we need to import all the reference types.
                    if (generateFromJsonDynamic || generateToJsonDynamic)
                    {
                        ICollection<DartType> dartSubtypes = DartSupport.GetDartSubtypes(t, exportedTypes).Where(dt => dt.DotNetType != t.DotNetType).ToList();
                        foreach (DartType subtype in dartSubtypes)
                        {
                            string subtypeImport = DartSupport.DartPackageImport(subtype.Package, subtype.Namespace, subtype.FileName);
                            imports.Add(subtypeImport);
                        }
                    }

                    // Import converter.dart if needed
                    if (t.Members.Any(m => m.Type.ConverterType != ConverterType.None))
                    {
                        imports.Add(DartImports.ConvertersImport(t.Package));
                    }

                    // Write all imports
                    foreach (string importLine in imports.Where(il => il != currentTypeImport).Distinct())
                    {
                        sw.WriteLine(importLine);
                    }

                    // NON USO PIU IL LibMessage
                    //sw.WriteLine(DartImports.LibMessageImport(t.Package));

                    sw.WriteLine($"");
                    // Add partial class to generate

                    string partialFilename = DartSupport.GetDartPartialFilename(t.DotNetType);

                    sw.WriteLine($"part '{partialFilename}';");
                    sw.WriteLine($"");
                    sw.WriteLine($"");
                    sw.WriteLine($"/// An annotation for the code generator to know that this class needs the");
                    sw.WriteLine($"/// the star denotes the source file name.");

                    sw.WriteLine($"@immutable");

                    // Nullable parameter for json conversion
                    string isNullable = t.IsNullable ? "true" : "false";
                    sw.WriteLine($"@JsonSerializable(nullable: {isNullable}, explicitToJson: true, anyMap: true)");

                    // NON USO PIU IL LibMessage
                    //sw.WriteLine($"class {t.Name} extends {LibMessageClassName} {{");

                    string implementsException = t.DotNetType == typeof(PlatformOperationException) ? "implements Exception " : "";

                    sw.WriteLine($"class {t.Name} extends {t?.BaseDartType?.Name ?? "Object"} {implementsException}{{");

                    sw.WriteLine($"");

                    List<string> thisParams = t.Members.Select(m => m.IsDeclared ? $"this.{m.DartName}" : $"{m.Type.Name} {m.DartName}").ToList();

                    // Super constructor (only if have some params)
                    List<string> superParams = (t.BaseDartType == null) ? new List<string>() : t.BaseDartType.Members.Select(m => $"{m.DartName}: {m.DartName}").ToList();

                    // User({this.name, this.email,}) : super(value: value, value1: value1,);
                    if (thisParams.Count > 0)
                    {
                        sw.Write($"\t{t.Name}({{\n\t\t{string.Join(",\n\t\t", thisParams)},\n\t}})");
                        sw.Write(t.BaseDartType == null || superParams.Count <= 0
                            ? $";"
                            : $" : super(\n{_multiTab5}{string.Join($",\n{_multiTab5}", superParams)},\n{_multiTab4});");
                        sw.WriteLine();
                    }
                    else
                    {
                        sw.WriteLine($"\t{t.Name}();");
                    }

                    sw.WriteLine($"");

                    // Only declared members: inherited NOT.
                    foreach (DartProperty prop in t.Members.Where(m => m.IsDeclared))
                    {

                        if (prop.Type.ConverterType != ConverterType.None)
                        {
                            sw.WriteLine($"\t@JsonKey(name: \"{prop.Name}\")");
                            string customConverterClass = DartSupport.GetConverterName(prop.Type.ConverterType);
                            sw.WriteLine($"\t{customConverterClass}");
                        }
                        else
                        {
                            string isPropNullable = prop.Type.IsNullable ? "true" : "false";
                            sw.WriteLine($"\t@JsonKey(name: \"{prop.Name}\", nullable: {isPropNullable})");
                        }

                        sw.WriteLine($"\tfinal {prop.Type.Name} {prop.DartName};");
                        sw.WriteLine($"");
                    }

                    sw.WriteLine($"");

                    // A necessary factory constructor for creating a new User instance
                    // from a map. Pass the map to the generated `_$UserFromJson()` constructor.
                    // The constructor is named after the source class, in this case, User.
                    // factory User.fromJson(Map < String, dynamic > json) => _$UserFromJson(json);
                    sw.WriteLine($"\tfactory {t.Name}.fromJson(Map<dynamic, dynamic> json) => _${t.Name}FromJson(json);");
                    sw.WriteLine($"");

                    // `toJson` is the convention for a class to declare support for serialization
                    // to JSON. The implementation simply calls the private, generated
                    // helper method `_$UserToJson`.
                    // Map<String, dynamic> toJson() => _$UserToJson(this);
                    sw.WriteLine($"\tMap<String, dynamic> toJson() => _${t.Name}ToJson(this);");
                    sw.WriteLine($"");

                    if (generateFromJsonDynamic)
                    {
                        // Dynamic deserialization method
                        string dynamicFromJsonMethod = _GenerateDynamicDeserializationMethod(t, exportedTypes);
                        sw.WriteLine(dynamicFromJsonMethod);
                    }

                    if (generateToJsonDynamic)
                    {
                        // Dynamic Serialization method
                        string dynamicToJsonMethod = _GenerateDynamicSerializationMethod(t, exportedTypes);
                        sw.WriteLine(dynamicToJsonMethod);
                    }
                    
                    sw.WriteLine($"");

                    // Add copyWith method
                    if (generateCopyWith && t.Members.Count > 0)
                    {
                        string copyWithMethod = _GenerateCopyWithMethod(t, exportedTypes);
                        sw.WriteLine(copyWithMethod);
                    }

                    sw.WriteLine($"");

                    // Add toString method
                    if (generateToString)
                    {
                        sw.WriteLine(_GenerateToStringMethod(t));
                        sw.WriteLine($"");
                    }

                    sw.WriteLine($"}}");

                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }

        }

        static void _GenerateDartEnumFile(DartType t, string destinationPath)
        {

            bool isValid = t.DotNetType.IsEnum;

            if (isValid == false)
                throw new Exception($"GenerateDartEnumFile: invalid NET type to generate -> {t.DotNetType.FullName}");

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");

                    // Current file import: is not necessary for the current dart file
                    string currentTypeImport = DartSupport.DartPackageImport(t.Package, t.Namespace, t.FileName);

                    sw.WriteLine($"// IMPORT FILE: {currentTypeImport}");
                    sw.WriteLine("// *************************************");

                    sw.WriteLine("");
                    sw.WriteLine($"enum {t.Name} {{");

                    // IMPORTANTE: in dart i nomi degli enum andrebbero minuscoli, ma poi ci sarebbe il problema
                    // nella serializzazione json: usiamo i nomi uguali a quelli definiti in c#
                    //INameConverter nameConverter = new DartClassNameConverter();

                    string[] names = Enum.GetNames(t.DotNetType);//.Select(n => nameConverter.Convert(n) ).ToArray();

                    sw.WriteLine($"\t{string.Join(",\n\t", names)},");

                    sw.WriteLine($"}}");
                }

                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }


        }

        private static readonly string ServiceType = "_type";
        private static readonly string ServiceInstanceId = "instanceId";

        internal static void GenerateDartServiceFile(DartService srv, ICollection<DartType> exportedTypes, string destinationPath)
        {

            DartType flutterException = exportedTypes.First(dt => dt.DotNetType == typeof(PlatformOperationException));

            DartType t = srv.Type;

            bool isValid = srv.Type.DotNetType.IsClass || srv.Type.DotNetType.IsInterface;

            if (isValid == false)
                throw new Exception($"GenerateClassServiceFile: invalid NET type to generate -> {t.DotNetType.FullName}");

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");

                    // Current file import: is not necessary for the current dart file
                    string currentTypeImport = DartSupport.DartPackageImport(t.Package, t.Namespace, t.FileName);

                    sw.WriteLine($"// IMPORT FILE: {currentTypeImport}");
                    sw.WriteLine("// *************************************");

                    // IMPORT DART ASYNC
                    //sw.WriteLine(DartImports.MetaImport);
                    sw.WriteLine(DartImports.DartAsyncImport);
                    sw.WriteLine(DartImports.FlutterServiceImport);
                    sw.WriteLine(DartSupport.DartPackageImport(flutterException.Package, flutterException.Namespace, flutterException.FileName));

                    // Import del bridge
                    sw.WriteLine(DartImports.BridgeImport(t.Package));

                    // Import fot each method import all the referenced types
                    List<string> imports = new List<string>();
                    foreach (DartMethod method in srv.Methods)
                    {
                        imports.AddRange(method.Imports);
                    }

                    // Import for all events args
                    foreach (DartEvent @event in srv.Events)
                    {
                        imports.AddRange(@event.Imports);
                    }
                    foreach (string importLine in imports.Distinct())
                    {
                        sw.WriteLine(importLine);
                    }

                    sw.WriteLine($"");
                    // Add partial class to generate

                    string filename = DartSupport.GetDartFilename(t.DotNetType);

                    sw.WriteLine($"");
                    sw.WriteLine($"");
                    sw.WriteLine($"class {t.Name} {{");
                    sw.WriteLine($"");

                    // Full namespace del tipo associato al servizio flutter
                    sw.WriteLine($"\tstatic const String {ServiceType} = '{t.DotNetType.FullName}';");
                    sw.WriteLine($"");

                    

                    // INIT for events
                    string[] initEvents = srv.Events.Select(e => _GenerateDartEventInit(e)).ToArray();

                    string initForEvent = !initEvents.Any() ? $"" : $" : {string.Join(",\n\t\t\t", initEvents)}";

                    // Constructor
                    sw.WriteLine($"\t{t.Name}(\n\t\tthis.{ServiceInstanceId},\n\t){initForEvent};");
                    sw.WriteLine($"");

                    sw.WriteLine($"\tfinal String {ServiceInstanceId};");

                    sw.WriteLine($"");
                    sw.WriteLine($"");

                    sw.WriteLine($"\t// Events ***************************** ");
                    // Eventi esposti dalla classe servizio
                    foreach (DartEvent @event in srv.Events)
                    {
                        sw.WriteLine(_GenerateDartEventGet(@event));
                    }

                    sw.WriteLine($"\t// Operations ***************************** ");
                    //
                    // Create all the related methods that use the MethodChannel _channel
                    //
                    foreach (DartMethod method in srv.Methods)
                    {
                        sw.WriteLine(_GenerateDartMethod(method, flutterException));
                    }

                    sw.WriteLine($"}}");


                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }


        }

        static string _GenerateToStringMethod(DartType t)
        {
            StringBuilder sw = new StringBuilder();
            sw.AppendLine($"\t@override");
            sw.AppendLine($"\tString toString() {{");
            sw.AppendLine($"\t\treturn toJson().toString();");
            sw.AppendLine($"\t}}");
            return sw.ToString();
        }

        static string _GenerateDartMethod(DartMethod method, DartType flutterException)
        {
            StringBuilder sw = new StringBuilder();

            string returnType = method.ReturnType.IsVoid ? "void" : method.ReturnType.Type.Name;

            List<string> @params = method.ParamObj.Members.Select(p => $"{p.Type.Name} {p.DartName}").ToList();

            string methodNameVariable = $"_k{method.Name.FirstCharUpper()}";

            sw.AppendLine($"\tstatic const {methodNameVariable} = '{method.MethodId}';");

            // Definizione del metodo: RETURN_TYPE nomeMetodo(param1,param2,...)
            sw.AppendLine(@params.Count > 0
                ? $"\tFuture<{returnType}> {method.Name}({{\n\t\t{string.Join(", \n\t\t", @params)},\n\t}}) async {{"
                : $"\tFuture<{returnType}> {method.Name}() async {{");

            sw.AppendLine($"");
            // Inizio Corpo del metodo


            sw.AppendLine($"\t\t// Errors occurring on the platform side cause invokeMethod to throw");
            sw.AppendLine($"\t\t// PlatformExceptions.");
            sw.AppendLine($"\t\ttry {{");

            // Here we call the method on the native flutter channel
            {
                
                sw.AppendLine($"");
                //sw.AppendLine($"\t\t// Here the real code....");

                // Parametri di inizializzazione della classe param
                List<string> initParams = method.ParamObj.Members.Select(p => $"{p.DartName}: {p.DartName}").ToList();

                if (initParams.Count > 0)
                {
                    sw.AppendLine($"\t\t\t{method.ParamObj.Name} _param = {method.ParamObj.Name}(\n\t\t\t\t{string.Join(", \n\t\t\t\t", initParams)},\n\t\t\t);");
                }
                else
                {
                    sw.AppendLine($"\t\t\t{method.ParamObj.Name} _param = {method.ParamObj.Name}();");
                }

                // Chiamo il bridge
                List<string> bridgeParameters = new List<string>(){ $"instanceId: {ServiceInstanceId}", $"service: {ServiceType}", $"operation: {methodNameVariable}" , $"arguments: _param.toJson()"}; 
                
                sw.AppendLine($"\t\t\tMap<String, dynamic> _data = await {DartClasses.BridgeClassName}().invokeMethod(\n\t\t\t\t{string.Join(", \n\t\t\t\t", bridgeParameters)},\n\t\t\t);");

                sw.AppendLine($"\t\t\t{method.ReturnObj.Name} _res = {method.ReturnObj.Name}.fromJson(_data);");

                if (method.ReturnType.IsVoid == false)
                {
                    string returnProperty = method.ReturnObj.Members.Count <= 0 ? string.Empty : $".{method.ReturnObj.Members.First().DartName}";
                    sw.AppendLine($"\t\t\treturn _res{returnProperty};");
                }

            }
            
            sw.AppendLine($"");
            sw.AppendLine($"\t\t}} on PlatformException catch (e) {{");
            sw.AppendLine($"\t\t\tthrow Exception(\"Unable to execute method '{method.Name}': ${{e.code}}, ${{e.message}}\");");
            sw.AppendLine($"\t\t}} on {flutterException.Name} catch (fe) {{");
            sw.AppendLine($"\t\t\tthrow fe;");
            sw.AppendLine($"\t\t}} on Exception catch (e) {{");
            sw.AppendLine($"\t\t\tthrow Exception(\"Unable to execute method '{method.Name}': $e\");");
            sw.AppendLine($"\t\t}}");

            // Fine corpo del metodo
            sw.AppendLine($"\t}}");
            sw.AppendLine($"");
            return sw.ToString();
        }

        static string _GenerateDartEventGet(DartEvent @event)
        {
            StringBuilder sw = new StringBuilder();

            sw.AppendLine($"\tfinal Stream<{@event.EventArgs.Name}> _{@event.Name};");
            sw.AppendLine($"\tStream<{@event.EventArgs.Name}> get {@event.Name} => _{@event.Name};");

            return sw.ToString();
        }

        private static string _multiTab4 = "\t\t\t\t";
        private static string _multiTab5 = "\t\t\t\t\t";
        private static string _multiTab6 = "\t\t\t\t\t\t";

        static string _GenerateDartEventInit(DartEvent @event)
        {
            StringBuilder sw = new StringBuilder();

            //sw.Append($"_{@event.Name} = {DartClasses.BridgeClassName}().events( {ServiceInstanceId}: {ServiceInstanceId}, event: '{@event.Name}').map((_) => {@event.EventArgs.Name}.fromJson(_))");
            sw.AppendLine($"_{@event.Name} = {DartClasses.BridgeClassName}()");
            sw.AppendLine($"{_multiTab6}.events( {ServiceInstanceId}: {ServiceInstanceId}, event: '{@event.Name}')");
            sw.Append($"{_multiTab6}.map((_) => {@event.EventArgs.Name}.fromJsonDynamic(_))");

            return sw.ToString();
        }


        internal static void GenerateExportFile(IEnumerable<DartType> types, string destinationPath)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    foreach (DartType type in types)
                    {
                        // export 'folder1/folder2/nome_file.dart';
                        sw.WriteLine($"export '{type.Namespace.Remove(0, 1)}{type.FileName}';");
                    }

                    //sw.WriteLine($"export '{ExceptionsFileName}';");
                    // NON USO PIU IL LibMessage
                    //sw.WriteLine($"export '{LibMessageFileName}';");
                    // NON USO PIU IL Serializzatore
                    //sw.WriteLine($"export '{MessageSerializerFileName}';");

                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }


        }


        /*
         * class LibMessage {
           
           }
         */

        /// <param name="t"></param>
        /// <param name="destinationPath"></param>
        /// <exception cref="Exception"> Error during write process</exception>
        internal static void GenerateLibMessageFile(string destinationPath)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");
                    sw.WriteLine("");
                    sw.WriteLine($"class {DartClasses.LibMessageClassName} {{}}");
                }

                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }


        }


        /*
         * library flutter_xamarin_protocol;
           
           export 'base/index.dart';
           export 'messages/index.dart';
           export 'models/index.dart';
           export 'message_serializer.dart';
           export 'message_deserializer.dart';

         */
        /// <param name="types"></param>
        /// <param name="destinationPath"></param>
        /// <param name="packageName"></param>
        /// <exception cref="Exception"> Error during write process</exception>
        internal static void GenerateIndexFile(string packageName, IEnumerable<DartType> types, string destinationPath)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");
                    sw.WriteLine("");
                    sw.WriteLine($"library {packageName};");
                    sw.WriteLine("");
                    foreach (DartType type in types)
                    {
                        // export 'folder1/folder2/nome_file.dart';
                        sw.WriteLine($"export '{type.Namespace.Remove(0,1)}{type.FileName}';");
                    }

                    //sw.WriteLine($"export '{ExceptionsFileName}';");
                    // NON USO PIU IL LibMessage
                    //sw.WriteLine($"export '{LibMessageFileName}';");
                    // NON USO PIU IL Serializzatore
                    //sw.WriteLine($"export '{MessageSerializerFileName}';");

                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }


        }

        /*
         * import "index.dart";
           import "dart:convert";
           
           class MessageSerializer {
           static final Map<String, LibMessage Function(Map<String, dynamic>)>
           _codeToMessage = {
           "CmdHello": (Map<String, dynamic> json) => CmdHello.fromJson(json),
           "CmdHello2": (Map<String, dynamic> json) => CmdHello2.fromJson(json),
           "CmdError": (Map<String, dynamic> json) => CmdError.fromJson(json),
           "CmdFetchItems": (Map<String, dynamic> json) =>
           CmdFetchItems.fromJson(json),
           "ResFetchItems": (Map<String, dynamic> json) =>
           ResFetchItems.fromJson(json),
           "CmdTestItem": (Map<String, dynamic> json) => CmdTestItem.fromJson(json),
           "ResTestItem": (Map<String, dynamic> json) => ResTestItem.fromJson(json),
           };
           
           static final Map<Type, String> _typeToCode = {
           CmdHello().runtimeType: "CmdHello",
           CmdHello2().runtimeType: "CmdHello2",
           CmdError().runtimeType: "CmdError",
           CmdFetchItems().runtimeType: "CmdFetchItems",
           ResFetchItems().runtimeType: "ResFetchItems",
           CmdTestItem().runtimeType: "CmdTestItem",
           ResTestItem().runtimeType: "ResTestItem",
           };
           
           ///
           /// Convert a serializes netJsonMessage in a [LibMessage].
           /// When the message is formatted bad, this mathod will fail
           /// with a [DeserializationException].
           ///
           static LibMessage deserialize(String jsonMsg) {
           // Nothing to do
           if (jsonMsg == null || jsonMsg.isEmpty) return null;
           
           try {
           // Json decoding
           Map<String, dynamic> json = jsonDecode(jsonMsg);
           
           String msgTypeCode = json.keys.first;
           
           var fromJson = _codeToMessage.containsKey(msgTypeCode)
           ? _codeToMessage[msgTypeCode]
           : null;
           
           String content = json[msgTypeCode];
           
           Map<String, dynamic> payload = jsonDecode(content);
           
           return fromJson(payload);
           
           // Try to obtain the clazz information
           LibMessage msgBase = LibMessage.fromJson(json);
           
           ///! *******************************
           ///! REAL DESERIALIZATION PROCESS
           ///! *******************************
           return MessageDeserializer.deserialize(msgBase.clazz, json);
           } catch (e) {
           throw new DeserializationException(
           "Error during lib deserialize process: $jsonMsg");
           }
           }
           
           ///
           /// Convert a [LibMessage] in a string json format
           /// used in .NET Xamarin library.
           /// In case of erros thow a [SerializationException].
           ///
           static String serialize(LibMessage libMessage) {
           // Nothing to do
           if (libMessage == null) return null;
           
           try {
           //
           final String key = _typeToCode.containsKey(libMessage.runtimeType)
           ? _typeToCode[libMessage.runtimeType]
           : null;
           
           String content = jsonEncode(libMessage);
           
           final Map<String, String> map = {
           key: content,
           };
           
           // Encode the message using JSON Serielization
           final String json = jsonEncode(map);
           
           return json;
           } catch (e) {
           throw new SerializationException(
           "Error during lib serialize process: $libMessage");
           }
           }
           }
           
         */

        public static void GenerateMessageSerializerFile(string package, ICollection<DartType> types, string destinationPath)
        {

            ICollection<DartType> dartTypes = types;

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {

                    sw.WriteLine("// *************************************");
                    sw.WriteLine("//         NOT EDIT THIS FILE          *");
                    sw.WriteLine("// *************************************");

                    // Imports
                    sw.WriteLine(DartImports.IndexImport(package));
                    sw.WriteLine(DartImports.DartConvertImport);

                    // OPEN CLASS
                    sw.WriteLine($"class {DartClasses.MessageSerializerClassName} {{");
                    sw.WriteLine($"");

                    // Creo il mapping fra il tipo e la funzione COSTRUTTURE DEL MESSAGGIO

                    #region generate _typeToMessage map

                    string typeToMessageVar = "_typeToMessage";

                    sw.WriteLine($"static final Map<String, {DartClasses.LibMessageClassName} Function(Map<String, dynamic>)> {typeToMessageVar} = {{");
                    foreach (DartType type in dartTypes)
                    {
                        string mapLine = $"'{type.DartId}': (Map<String, dynamic> json) => {type.Name}.fromJson(json),";
                        sw.WriteLine(mapLine);
                    }

                    sw.WriteLine($"}};");

                    #endregion

                    sw.WriteLine($"");
                    sw.WriteLine($"");

                    // Creo il mapping fra il tipo ed il suo codice

                    #region generate _typeToCode map

                    string typeToCodeVar = "_typeToCode";

                    sw.WriteLine($"static final Map<Type, String>  {typeToCodeVar} = {{");
                    foreach (DartType type in dartTypes)
                    {
                        string mapLine = $" {type.Name}().runtimeType: '{type.DartId}' ,";
                        sw.WriteLine(mapLine);
                    }

                    sw.WriteLine($"}};");

                    #endregion


                    sw.WriteLine($"");
                    sw.WriteLine($"");
                    sw.WriteLine($"/// Convert a [{DartClasses.LibMessageClassName}] in a string json format");
                    sw.WriteLine($"/// used in .NET Xamarin library.");
                    sw.WriteLine($"/// In case of errors throws a [{DartClasses.SerializationExceptionClassName}].");
                    sw.WriteLine($"///");
                    #region SErialize method

                    string libMessageVar = "libMessage";
                    
                    // method start
                    sw.WriteLine($"static String serialize({DartClasses.LibMessageClassName} {libMessageVar}) {{");

                    // try
                    sw.WriteLine($"try {{"); 

                    sw.WriteLine($"// Obtain the code for the specific type");
                    sw.WriteLine($"final String key = {typeToCodeVar}.containsKey({libMessageVar}.runtimeType) " +
                                     $"? {typeToCodeVar}[{libMessageVar}.runtimeType] " +
                                     $": null; ");
                    sw.WriteLine($"");
                    sw.WriteLine($"String content = jsonEncode({libMessageVar});");
                    sw.WriteLine($"");
                    sw.WriteLine($"final Map<String, String> map = {{ key: content, }};");
                    sw.WriteLine($"");
                    sw.WriteLine($"// Encode the message using JSON Serialization ");
                    sw.WriteLine($"final String json = jsonEncode(map);");
                    sw.WriteLine($"return json;");
                    sw.WriteLine($"");
                    
                    // catch
                    sw.WriteLine($"}} catch (e) {{"); 
                    sw.WriteLine($"  throw new {DartClasses.SerializationExceptionClassName}( 'Error during lib serialization process: ${libMessageVar}' );");
                    sw.WriteLine($"}}");

                    // method end
                    sw.WriteLine($"}}");

                    #endregion


                    sw.WriteLine($"");
                    sw.WriteLine($"");
                    sw.WriteLine($"///");
                    sw.WriteLine($"/// Convert a serializes netJsonMessage in a [{DartClasses.LibMessageClassName}].");
                    sw.WriteLine($"/// When the message is formatted bad, this method will fail");
                    sw.WriteLine($"/// with a [{DartClasses.DeserializationExceptionClassName}].");
                    sw.WriteLine($"///");
                    #region DEserialize method

                    string jsonVar = "jsonMessage";

                    sw.WriteLine($"static {DartClasses.LibMessageClassName} deserialize(String {jsonVar}) {{");

                    // try
                    sw.WriteLine($"try {{");

                    sw.WriteLine($"// Json decoding");
                    sw.WriteLine($"Map<String, dynamic> json = jsonDecode({jsonVar});");
                    sw.WriteLine($"");
                    sw.WriteLine($"// The code type for the message");
                    sw.WriteLine($"String msgTypeCode = json.keys.first;");
                    sw.WriteLine($"");
                    sw.WriteLine($"// The constructor for the message");
                    sw.WriteLine($"var fromJson = {typeToMessageVar}.containsKey(msgTypeCode) ? {typeToMessageVar}[msgTypeCode] : null;");
                    sw.WriteLine($"");
                    sw.WriteLine($"String content = json[msgTypeCode];");
                    sw.WriteLine($"");
                    sw.WriteLine($"// decode the content as a Map");
                    sw.WriteLine($"Map<String, dynamic> payload = jsonDecode(content);");
                    sw.WriteLine($"");
                    sw.WriteLine($"return fromJson(payload);");
                    sw.WriteLine($"");
                    // catch
                    sw.WriteLine($"}} catch (e) {{");
                    sw.WriteLine($"  throw new {DartClasses.DeserializationExceptionClassName}( 'Error during lib deserialization process: ${jsonVar}' );");
                    sw.WriteLine($"}}");

                    sw.WriteLine($"}}");

                    #endregion


                    // CLOSE CLASS
                    sw.WriteLine($"}}");


                }


                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());
            }
        }

        static string _GenerateDynamicDeserializationMethod(DartType type, ICollection<DartType> exportedTypes)
        {
            StringBuilder sw = new StringBuilder();

            ICollection<DartType> dartSubtypes = DartSupport.GetDartSubtypes(type,exportedTypes);

            string typeToMessageVar = $"\t_typeTo{type.Name}";

            // Creo il mapping fra il tipo e la funzione COSTRUTTURE DEL MESSAGGIO
            #region generate _typeToMessage map

            sw.AppendLine($"\t/// Mapping between NET types and Dart Type");
            sw.AppendLine($"\tstatic final Map<String, {type.Name} Function(Map<String, dynamic>)> {typeToMessageVar} = {{");
            foreach (DartType subType in dartSubtypes)
            {
                string mapLine = $"\t\t'{subType.DartId}': (Map<String, dynamic> json) => {subType.Name}.fromJson(json),";
                sw.AppendLine(mapLine);
            }

            sw.AppendLine($"\t}};");

            #endregion

            sw.AppendLine($"");
            sw.AppendLine($"");

            string jsonVar = "json";

            // Metodo di deserializzazione
            sw.AppendLine($"\t/// Dynamic deserialization");
            sw.AppendLine($"\tfactory {type.Name}.fromJsonDynamic(Map<String, dynamic> {jsonVar}) {{");
            sw.AppendLine($"");
            sw.AppendLine($"\t\t// Nothing to do");
            // FIX 20201203: data with no params, like EventArgs can pass this check
            //sw.AppendLine($"\t\tif ({jsonVar} == null || {jsonVar}.isEmpty) return null;");
            sw.AppendLine($"\t\tif ({jsonVar} == null) return null;");
            sw.AppendLine($"");

            // try
            sw.AppendLine($"\t\ttry {{");
            #region OLD WAY
            //sw.AppendLine($"\t\t\tString typeKey = {jsonVar}.keys.first;");
            //sw.AppendLine($"\t\t\tvar fromJson = {typeToMessageVar}.containsKey(typeKey)");
            //sw.AppendLine($"\t\t\t ? {typeToMessageVar}[typeKey] ");
            //sw.AppendLine($"\t\t\t : null;");

            //sw.AppendLine($"");
            //sw.AppendLine($"\t\t\tMap<String, dynamic> payload = {jsonVar}[typeKey];");
            //sw.AppendLine($"");
            //sw.AppendLine($"\t\t\t///! REAL DESERIALIZATION PROCESS");
            //sw.AppendLine($"\t\t\treturn fromJson(payload);");
            //sw.AppendLine($"");
            #endregion

            sw.AppendLine($"\t\t\tString typeKey = {jsonVar}['\\$type'];");
            sw.AppendLine($"\t\t\t// Default type key");
            sw.AppendLine($"\t\t\ttypeKey ??= '{type.DartId}';");
            sw.AppendLine($"\t\t\tvar fromJson = {typeToMessageVar}.containsKey(typeKey)");
            sw.AppendLine($"\t\t\t ? {typeToMessageVar}[typeKey] ");
            sw.AppendLine($"\t\t\t : null;");

            sw.AppendLine($"");
            sw.AppendLine($"\t\t\t///! REAL DESERIALIZATION PROCESS");
            sw.AppendLine($"\t\t\treturn fromJson({jsonVar});");
            sw.AppendLine($"");

            // catch
            sw.AppendLine($"\t\t}} catch (e) {{");
            sw.AppendLine($"\t\t  throw new Exception('Error during lib deserialization process: ${jsonVar}');");
            sw.AppendLine($"\t\t}}");

            sw.AppendLine($"\t}}");


            return sw.ToString();
        }

        static string _GenerateDynamicSerializationMethod(DartType type, ICollection<DartType> exportedTypes)
        {
            StringBuilder sw = new StringBuilder();

            ICollection<DartType> dartSubtypes = DartSupport.GetDartSubtypes(type, exportedTypes);

            string mapDartTypeToNetType = $"\t_{type.Name.FirstCharLower()}ToType";

            // Mapping fra il tipo Dart e il tipo .NET
            #region generate _typeToMessage map

            sw.AppendLine($"\t/// Mapping between Dart Type and NET types");
            sw.AppendLine($"\tstatic final Map<Type, String> {mapDartTypeToNetType} = {{");
            foreach (DartType subType in dartSubtypes)
            {
                string mapLine = $"\t\t{subType.Name}().runtimeType : \"{subType.DartId}\",";
                sw.AppendLine(mapLine);
            }

            sw.AppendLine($"\t}};");

            #endregion

            sw.AppendLine($"");
            sw.AppendLine($"");


            // Metodo di deserializzazione
            sw.AppendLine($"\t/// Dynamic serialization");
            sw.AppendLine($"\tMap<String, dynamic> toJsonDynamic() {{");
            sw.AppendLine($"");

            // try
            sw.AppendLine($"\t\ttry {{");
            sw.AppendLine($"\t\t\t// Get the NET Type from the Dart runtime type");
            sw.AppendLine($"\t\t\tfinal String typeKey = ");
            sw.AppendLine($"\t\t\t{mapDartTypeToNetType}.containsKey(this.runtimeType)");
            sw.AppendLine($"\t\t\t ? {mapDartTypeToNetType}[this.runtimeType] ");
            sw.AppendLine($"\t\t\t : null;");

            sw.AppendLine($"");

            #region OLD WAY
            //sw.AppendLine($"\t\t\t/// Wrap the object with his NET type key");
            //sw.AppendLine($"\t\t\tfinal Map<String, dynamic> map = {{");
            //sw.AppendLine($"\t\t\t  typeKey: this.toJson(),");
            //sw.AppendLine($"\t\t\t}};");
            #endregion

            sw.AppendLine($"\t\t\t/// Wrap the object with his NET type key");
            sw.AppendLine($"\t\t\tfinal Map<String, dynamic> map = {{");
            sw.AppendLine($"\t\t\t\t'\\$type' : typeKey,");
            sw.AppendLine($"\t\t\t}};");
            sw.AppendLine($"\t\t\tmap.addAll(this.toJson());");
            sw.AppendLine($"\t\t\treturn map;");

            sw.AppendLine($"");
            // catch
            sw.AppendLine($"\t\t}} catch (e) {{");
            sw.AppendLine($"\t\t  throw new Exception('Error during lib serialization process: ${{this.runtimeType}}');");
            sw.AppendLine($"\t\t}}");

            sw.AppendLine($"\t}}");


            return sw.ToString();
        }

        static string _GenerateCopyWithMethod(DartType type, ICollection<DartType> exportedTypes)
        {
            StringBuilder sw = new StringBuilder();

            List<string> methodParams = type.Members.Select(m => $"{m.Type.Name} {m.DartName}").ToList();
            
            sw.AppendLine($"\t// Copy with method for the class");
            sw.AppendLine($"\t{type.Name} copyWith({{\n\t\t{string.Join(",\n\t\t", methodParams)},\n\t}}) {{");

            sw.AppendLine($"\treturn {type.Name}(");

            foreach (DartProperty prop in type.Members)
            {
                sw.AppendLine($"\t\t{prop.DartName}: {prop.DartName} ?? this.{prop.DartName},");
            }

            sw.AppendLine($"\t);");

            sw.AppendLine($"\t}}");


            return sw.ToString();
        }


        /// <summary>
        /// Fix all the auto-generated json files, replacig the default toJson, fromJson into toJsonDynamic & fromJsonDynamic
        /// </summary>
        /// <param name="destinationPath"></param>
        public static void FixDartGPartedJsonFile(DartType t, string destinationPath)
        {

            if (t.DotNetType.IsClass == false)
            {
                return;
            }

            List<string> fileLines = new List<string>();

            // 1 Read the all file
            using (StreamReader sr = new StreamReader(destinationPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string tmp = line.Replace("fromJson(", "fromJsonDynamic(");
                    tmp = tmp.Replace("toJson(", "toJsonDynamic(");

                    fileLines.Add(tmp);
                }
            }

            // 2 Edit the single lines
            // - fromJson( --> fromJsonDynamic(
            // - toJson( --> toJsonDynamic(
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    foreach (string newline in fileLines)
                    {
                        sw.WriteLine(newline);
                    }
                }

                // Same the stream to file path
                File.WriteAllBytes(destinationPath, ms.ToArray());

            }
        }

        public static void GenerateBridgeFile(string destinationPath)
        {
            File.WriteAllText(destinationPath, ResourceReader.ReadStringResources(typeof(DartGenerator), "bridgeV12.dart"), Encoding.UTF8);
        }

        public static void GenerateConverterFile(string destinationPath)
        {
            File.WriteAllText(destinationPath, ResourceReader.ReadStringResources(typeof(DartGenerator), "convertersV2.dart"), Encoding.UTF8);
        }
    }
}