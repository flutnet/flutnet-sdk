using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Core.Dart
{
    internal class DartClassNameConverter : INameConverter
    {
        public string Convert(string className)
        {
            return className;
        }
    }
}