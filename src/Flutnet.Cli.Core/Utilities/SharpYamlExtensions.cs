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
using SharpYaml.Serialization;
using YamlDocument = SharpYaml.Serialization.YamlDocument;
using YamlNode = SharpYaml.Serialization.YamlNode;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class SharpYamlExtensions
    {
        public static string GetScalarValue(this YamlDocument doc, string[] keys)
        {
            // ROOT DOCUMENT
            YamlMappingNode root = (YamlMappingNode)doc.RootNode;
            // Find the nested value
            YamlScalarNode scalarNode = GetScalarNode(root, keys);
            return scalarNode?.Value ?? string.Empty;
        }

        public static void SetScalarValue(this YamlDocument doc, string[] keys, string value)
        {
            // ROOT DOCUMENT
            YamlMappingNode root = (YamlMappingNode)doc.RootNode;
            // Find the nested value
            YamlScalarNode scalarNode = GetScalarNode(root, keys);
            if (scalarNode == null)
            {
                throw new InvalidOperationException($"Scalar node for keys {string.Join(":", keys)} not found!");
            }
            scalarNode.Value = value;
        }

        public static YamlScalarNode GetScalarNode(this YamlDocument doc, string[] keys)
        {
            // ROOT DOCUMENT
            YamlMappingNode root = (YamlMappingNode)doc.RootNode;
            return root.GetScalarNode(keys);
        }

        public static YamlScalarNode GetScalarNode(this YamlMappingNode node, string[] keys, int index = 0)
        {
            if (index >= keys.Length)
                return null;

            YamlScalarNode currentKey = new YamlScalarNode(keys[index]);

            // Node NOT found
            if (node.Children.ContainsKey(currentKey) == false || node.Children[currentKey] == null)
                return null;

            YamlNode nestedNode = node.Children[currentKey];

            // Node FOUND
            if (nestedNode is YamlScalarNode scalarNode && index == keys.Length - 1)
            {
                return scalarNode;
            }

            // Retry by going deeper
            if (nestedNode is YamlMappingNode mappingNode)
            {
                return GetScalarNode(mappingNode, keys, ++index);
            }

            // Not found
            return null;
        }

        public static YamlMappingNode GetMappingNode(this YamlDocument doc, string[] keys, int index = 0)
        {
            // ROOT DOCUMENT
            YamlMappingNode root = (YamlMappingNode)doc.RootNode;
            return root.GetMappingNode(keys);
        }

        static YamlMappingNode GetMappingNode(this YamlMappingNode node, string[] keys, int index = 0)
        {
            if (index >= keys.Length)
                return null;

            YamlScalarNode currentKey = new YamlScalarNode(keys[index]);

            // Node NOT found
            if (node.Children.ContainsKey(currentKey) == false || node.Children[currentKey] == null)
                return null;

            YamlNode nestedNode = node.Children[currentKey];

            if (nestedNode is YamlMappingNode == false)
            {
                return null;
            }

            YamlMappingNode nestedMappingNode = (YamlMappingNode) nestedNode;

            // Last key: found
            if (index == keys.Length - 1)
            {
                return nestedMappingNode;
            }

            return GetMappingNode(nestedMappingNode, keys, ++index);
        }
    }
}
