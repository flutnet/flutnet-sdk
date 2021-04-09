using System;
using System.Reflection;

namespace Flutnet.Data
{
    internal class FlutnetAppInfo
    {
        [Obfuscation(Exclude = true)]
        public string ApplicationId { get; set; }
        [Obfuscation(Exclude = true)]
        public string ProductKey { get; set; }
        [Obfuscation(Exclude = true)]
        public DateTime LicenseExpirationDate { get; set; }
    }
}
