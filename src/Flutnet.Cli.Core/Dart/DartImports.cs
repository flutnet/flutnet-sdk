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