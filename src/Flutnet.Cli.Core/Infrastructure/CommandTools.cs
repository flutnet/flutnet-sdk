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
using Medallion.Shell;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal static class CommandTools
    {
        /// <summary>
        /// A convenience method that wraps <code>command.Wait()</code> in order to re-throw a fancy <see cref="CommandLineException"/>
        /// when the underlying task fails or is canceled.
        /// </summary>
        public static void WaitWithFancyError(this Command command, CommandLineErrorCode errorCode)
        {
            try
            {
                command.Wait();
            }
            catch (Exception e)
            {
                Log.Ex(e);
                throw new CommandLineException(errorCode, e);
            }
        }
    }
}