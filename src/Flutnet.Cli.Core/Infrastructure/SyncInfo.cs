using System;
using System.Collections.Generic;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class SyncInfo
    {
        public string RootFolder { get; }
        public List<string> Folders { get; }
        public Dictionary<Type, string> TypeToFolder { get; }
        public Dictionary<string, List<Type>> FolderToTypes { get; }

        public SyncInfo(string rootFolder, List<string> folders, Dictionary<string, List<Type>> folderToTypes, Dictionary<Type, string> typeToFolder)
        {
            FolderToTypes = folderToTypes;
            TypeToFolder = typeToFolder;
            RootFolder = rootFolder;
            Folders = folders;
        }
    }
}