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