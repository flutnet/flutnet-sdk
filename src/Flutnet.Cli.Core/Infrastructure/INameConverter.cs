namespace Flutnet.Cli.Core.Infrastructure
{
    internal interface INameConverter
    {
        string Convert(string name);
    }

    internal class IdentityNameConverter : INameConverter
    {
        public string Convert(string name) => name;
    }
}