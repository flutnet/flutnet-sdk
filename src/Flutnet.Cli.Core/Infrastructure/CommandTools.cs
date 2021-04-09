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