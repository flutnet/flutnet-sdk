using System;
using Flutnet.Cli.Core;
using Flutnet.Cli.Core.Infrastructure;

namespace Flutnet.Cli.Commands
{
    internal class PackCommand : CommandBase
    {
        readonly PackOptions _options;

        public PackCommand(PackOptions options)
        {
            _options = options;
        }

        public static int Run(PackOptions options)
        {
            options.Validate();
            return new PackCommand(options).Execute();
        }

        public override int Execute()
        {
            try
            {
                DartBridgeBuilder.Build(
                    _options.Assembly, _options.OutputDirectory, _options.PackageName,
                    _options.PackageDescription, _options.Verbose);
                return ReturnCodes.Success;
            }
            catch (Exception ex)
            {
                if (!(ex is CommandLineException))
                    Log.Ex(ex);
                Console.WriteLine(ex.Message); // Print message in VisualStudio output console
                //Console.Error.WriteLine(ex.Message); // Print message in VisualStudio error console
                return ReturnCodes.CommandExecutionError;
            }
        }
    }
}