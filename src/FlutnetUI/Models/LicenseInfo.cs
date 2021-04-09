using Flutnet.Cli.DTO;

namespace FlutnetUI.Models
{
    public class LicenseInfo
    {
        public string ProductKey { get; set; }
        public LicenseStatus LicenseStatus { get; set; }
        public LicenseInvalidReasons InvalidReasons { get; set; }
        public int LicenseType { get; set; }
        public string LicenseTypeText
        {
            get
            {
                switch (LicenseType)
                {
                    case 1: 
                        return "Single Developer";
                    case 2: 
                        return "Give-away";
                    case 3: 
                        return "Community";
                    default: 
                        return "Company";
                }
            }
        }
        public string LicenseOwner { get; set; }
        public bool LoadError { get; set; }
        public string LoadErrorMessage { get; set; }
    }
}
