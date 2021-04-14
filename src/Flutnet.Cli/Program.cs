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
