using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartPropertyNameConverter : INameConverter
    {
        private static string _null = "null";
        private static string _nnull = "nnull";

        private static string _true = "true";
        private static string _ttrue = "ttrue";

        private static string _false = "false";
        private static string _ffalse = "ffalse";

        public string Convert(string propertyName)
        {
            string prop = propertyName.ToLower();

            if (prop == _null)
            {
                return _nnull;
            }

            if (prop == _true)
            {
                return _ttrue;
            }

            if (prop == _false)
            {
                return _ffalse;
            }

            return char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }
    }
}