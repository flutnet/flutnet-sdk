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