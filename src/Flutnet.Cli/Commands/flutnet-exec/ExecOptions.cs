using System;
using System.IO;
using CommandLine;
using Flutnet.Cli.Core.Infrastructure;
using Flutnet.Cli.DTO;
using Newtonsoft.Json;

namespace Flutnet.Cli.Commands
{
    [Verb("exec", HelpText = "Execute a generic command.", Hidden = true)]
    public class ExecOptions : OptionsBase
    {
        [Value(0, Required = true, HelpText = "Path of the JSON file containing all the command arguments.")]
        public string InputFile { get; set; }

        internal InArg Arguments { get; private set; }

        [Value(1, Required = true, HelpText = "Path of the JSON file containing command result details.")]
        public string OutputFile { get; set; }

        public override void Validate()
        {
            if (!File.Exists(InputFile))
                throw new OptionsValidationException(GetType(), $"File {InputFile} not found.");

            try
            {
                string json = File.ReadAllText(InputFile);
                Arguments = JsonConvert.DeserializeObject<InArg>(json);
            }
            catch (Exception ex)
            {
                Log.Ex(ex);
                throw new OptionsValidationException(GetType(), $"File {InputFile} is badly formatted.", ex);
            }

            //TODO: Check output path accessibility
        }
    }
}
