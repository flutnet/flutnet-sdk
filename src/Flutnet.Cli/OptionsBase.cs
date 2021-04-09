using CommandLine;

namespace Flutnet.Cli
{
    public class OptionsBase
    {
        [Option('v', "verbose", Required = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        public virtual void Validate()
        {
            // Add support for validation and custom types
            // https://github.com/commandlineparser/commandline/issues/146
        }
    }
}
