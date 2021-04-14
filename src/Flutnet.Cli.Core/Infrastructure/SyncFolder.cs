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
using System.Reflection;
using System.Text;
using Flutnet.Cli.Core.Utilities;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal static class SyncFolder
    {
        public static SyncInfo SyncFolderStructureUsingNamespace(Assembly assembly, string destinationFolderPath, INameConverter nameConverter = null)
        {
            INameConverter converter = nameConverter ?? (INameConverter)new IdentityNameConverter();

            string destPath = destinationFolderPath;

            if (Directory.Exists(destPath) == false)
            {
                Directory.CreateDirectory(destPath);
            }

            // Create a directory for each namespace
            List<string> namespaces = assembly.GetNamespaces().ToList();

            // Mapping fra un tipo e la sua locazione nel disco
            Dictionary<Type, string> typeToFolder = new Dictionary<Type, string>();
            // Mapping fra un path ed una lista di tipi
            Dictionary<string, List<Type>> folderToTypes = new Dictionary<string, List<Type>>();

            List<string> allNewFolders = new List<string>();

            char[] separator = {'.'};
            StringBuilder pathBuilder = new StringBuilder();
            foreach (string ns in namespaces)
            {
                string[] nsFolders = ns.Split(separator);
                pathBuilder.Clear();
                pathBuilder.Append(destPath);
                foreach (string folder in nsFolders)
                {
                    pathBuilder.Append(Path.DirectorySeparatorChar).Append(converter.Convert(folder));
                }

                string path = pathBuilder.ToString();

                // Calling Create() will not error if the path already exists.
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                // Add the path to the all folders list
                allNewFolders.Add(path);


                // Mappo i tipi con la cartella e viceversa
                List<Type> typesInFolder = assembly.GetTypesInNamespace(ns).ToList();

                foreach (Type t in typesInFolder)
                {
                    typeToFolder.Add(t, path);
                }

                folderToTypes.Add(path, typesInFolder);

            }

            // Dopo aver generato tutto elimino le cartelle che non centrano nulla
            List<string> newDirectories = GetAllSubfoldersPaths(destinationFolderPath); 

            foreach (var dir in newDirectories)
            {
                bool exists = allNewFolders.Contains(dir) || allNewFolders.Any(f=>f.StartsWith(dir));
                if (exists == false)
                {
                    Directory.Delete(dir);
                }
            }

            // Ritorno il risultato del mapping
            return new SyncInfo(destPath,allNewFolders,folderToTypes,typeToFolder);
        }

        /// <summary>
        /// Sincronizza la strutture delle directory di 2 folder.
        /// Specchio da sorgente --> destinazione.
        /// </summary>
        public static void SyncFolderStructure(string sourceFolderPath, string destinationFolderPath, bool canCreateDestination = false, INameConverter nameConverter = null)
        {
            INameConverter converter = nameConverter ?? new IdentityNameConverter();

            if (Directory.Exists(sourceFolderPath) == false)
            {
                throw new Exception("Source directory not exist!");
            }

            string destPath = ConvertDirectoryName(destinationFolderPath, converter);

            if (Directory.Exists(destPath) == false)
            {
                if (canCreateDestination == false)
                {
                    throw new Exception("Destination directory not exist!");
                }
                else
                {
                    Directory.CreateDirectory(destPath);
                }
            }

            // Recurse into subdirectories of this directory.
            List<string> sourceDirectories = Directory.EnumerateDirectories(sourceFolderPath).ToList();

            foreach (var dir in sourceDirectories)
            {
                string directoryName = Path.GetFileName(dir);
                if(directoryName == null)
                    continue;
                SyncFolderStructure(dir, Path.Combine(destinationFolderPath, directoryName), true, converter);
            }

            // Dopo aver generato tutto elimino le cartelle che non centrano nulla
            List<string> newDirectories = Directory.EnumerateDirectories(destPath).ToList();
            List<string> localFolderDirectoriesName = sourceDirectories.Select(Path.GetFileName).Select(name => converter.Convert(name)).ToList();

            foreach (var dir in newDirectories)
            {
                bool exists = localFolderDirectoriesName.Contains(Path.GetFileName(dir));
                if (exists == false)
                {
                    Directory.Delete(dir);
                }
            }
        }

        private static string ConvertDirectoryName(string path, INameConverter converter)
        {
            string parentDirPath = Path.GetDirectoryName(path);
            
            if (parentDirPath == null)
                throw new Exception("Invalid path to convert!");

            string dirName = Path.GetFileName(path);

            return Path.Combine(parentDirPath, converter.Convert(dirName));
        }

        private static List<string> GetAllSubfoldersPaths(string directoryPath)
        {
            if (Directory.Exists(directoryPath) == false)
            {
                throw new Exception("The directory not exists!");
            }

            List<string> allSubdirectories = new List<string>();

            // Recurse into subdirectories of this directory.
            List<string> subdirectories = Directory.EnumerateDirectories(directoryPath).ToList();

            foreach (string dir in subdirectories)
            {
                allSubdirectories.Add(dir);
                List<string> subdirs = GetAllSubfoldersPaths(dir);
                allSubdirectories.AddRange(subdirs);
            }

            return allSubdirectories;
        }
    }
}