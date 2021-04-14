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

using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Flutnet.Cli.Core.Infrastructure
{
    [Obfuscation(Exclude = true)]
    public class SdkTable
    {
        [XmlAttribute]
        public string Revision { get; set; }

        [XmlElement("Sdk")]
        public List<SdkVersion> Versions { get; set; }
    }

    [Obfuscation(Exclude = true)]
    public class SdkVersion
    {
        [XmlAttribute]
        public string Version { get; set; }

        public List<SdkFlutterVersion> Compatibility { get; set; }
    }

    [Obfuscation(Exclude = true)]
    [XmlType("Flutter")]
    public class SdkFlutterVersion
    {
        [XmlAttribute]
        public string Version { get; set; }

        [XmlAttribute]
        [DefaultValue(null)]
        public string PrjTemplateVersion { get; set; }

        [XmlAttribute]
        [DefaultValue(null)]
        public string AndroidInteropVersion { get; set; }

        [XmlAttribute]
        [DefaultValue(null)]
        public string IosInteropVersion { get; set; }
    }
}
