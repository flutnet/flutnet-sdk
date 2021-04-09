namespace Flutnet.Cli.Core.Dart
{
    internal static class DartClasses
    {
        internal const string SerializationExceptionClassName = "SerializationException";
        internal const string DeserializationExceptionClassName = "DeserializationException";
        internal const string BridgeClassName = "FlutnetBridge";
        internal const string MessageSerializerClassName = "MessageSerializer";
        internal const string LibMessageClassName = "LibMessage";
        internal const string Uint8ListClassName = "Uint8List";
        internal const string Uint8ListConverterClassName = "Uint8Converter"; // see file converters.dart
        internal const string DateTimeClassName = "DateTime";
        internal const string DateTimeConverterClassName = "DateTimeConverter";
    }

    internal static class DartFiles
    {
        internal const string ExceptionsFileName = "exceptions.dart";
        internal const string BridgeFileName = "flutnet_bridge.dart";
        internal const string ConvertersFileName = "converters.dart";
        internal const string MessageSerializerFileName = "message_serializer.dart";
        internal const string IndexFileName = "index.dart";
        internal const string LibMessageFileName = "lib_message.dart";
    }

    internal static class DartImports
    {
        internal const string JsonAnnotationImport = "import 'package:json_annotation/json_annotation.dart';";
        internal const string DartAsyncImport = "import 'dart:async';";
        internal const string DartConvertImport = "import 'dart:convert';";
        internal const string DartTypedDataImport = "import 'dart:typed_data';";
        internal const string MetaImport = "import 'package:meta/meta.dart';";
        internal const string FlutterServiceImport = "import 'package:flutter/services.dart';";

        public static string BridgeImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.BridgeFileName);
        public static string ExceptionsImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.ExceptionsFileName);
        public static string ConvertersImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.ConvertersFileName);
        public static string MessageSerializerImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.MessageSerializerFileName);
        public static string IndexImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.IndexFileName);
        public static string LibMessageImport(string package) => DartSupport.DartPackageImport(package, DartProject.DartSeparator.ToString(), DartFiles.LibMessageFileName);
    }
}