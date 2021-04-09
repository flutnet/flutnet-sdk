using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommandLine;
using Flutnet.Cli.Commands;
using Microsoft.Extensions.Configuration;

namespace Flutnet.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("flutnet.appsettings.json")
                .Build();

#if DEBUG
            //return Core.Infrastructure.DevWorkUnit.Run();
#endif

            try
            {
#if DEBUG
                if (configuration.GetValue("Debuggable", false))
                {
                    for (;;)
                    {
                        Console.WriteLine("Waiting for debugger...");
                        if (Debugger.IsAttached)
                            break;
                        System.Threading.Thread.Sleep(1000);
                    }
                }
#endif
                return Parser.Default.ParseArguments<ExecOptions, PackOptions>(args)
                    .MapResult(
                        (ExecOptions options) => ExecCommand.Run(options),
                        (PackOptions options) => PackCommand.Run(options),
                        errors => ReturnCodes.OptionsParsingError);
            }
            catch (OptionsValidationException ex)
            {
                Console.WriteLine(ex.Message);
                return ReturnCodes.OptionsValidationError;
            }
        }
    }
}
