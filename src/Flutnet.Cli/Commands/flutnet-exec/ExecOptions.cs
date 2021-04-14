// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

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
