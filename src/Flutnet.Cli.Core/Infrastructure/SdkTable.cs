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
