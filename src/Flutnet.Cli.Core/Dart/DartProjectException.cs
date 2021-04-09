using System;

namespace Flutnet.Cli.Core.Dart
{
    internal enum DartProjectError
    {
        PrjFolderNotExists,
        LibFolderNotExists,
        PubspecNotExists,
        PubspecInvalidFormat,
        MetadataFileNotExists,
        MetadataInvalidFormat,
    }

    internal class DartProjectException : Exception
    {
        public DartProjectError Error { get; }

        public DartProjectException(string message, DartProjectError error) : base(message)
        {
            Error = error;
        }
    }
}