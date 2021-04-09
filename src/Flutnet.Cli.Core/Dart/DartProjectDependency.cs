namespace Flutnet.Cli.Core.Dart
{
    internal enum DartProjectDependencyType
    {
        Version,
        Path,
        Sdk
    }

    internal class DartProjectDependency
    {
        public DartProjectDependencyType Type { get; }
        public string Name { get; }
        public string Value { get; }

        public DartProjectDependency(string name, DartProjectDependencyType type, string value)
        {
            Type = type;
            Value = value;
            Name = name;
        }
    }
}