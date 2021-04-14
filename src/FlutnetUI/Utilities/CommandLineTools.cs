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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flutnet.Cli.DTO;
using Medallion.Shell;
using Newtonsoft.Json;

namespace FlutnetUI.Utilities
{
    internal class CommandLineTools
    {
#if DEBUG
        static readonly string ExecutableFolder = Path.GetFullPath(Path.Combine(AppSettings.Default.AppPath, "../../../../Flutnet.Cli/bin/Debug/netcoreapp3.1"));
        static readonly string Executable = OperatingSystem.IsMacOS()
            ? Path.Combine(ExecutableFolder, "flutnet")
            : Path.Combine(ExecutableFolder, "flutnet.exe");
#else
        static readonly string Executable = OperatingSystem.IsMacOS() ? "flutnet" : "flutnet.exe";
#endif

        public static Task<CommandLineCallResult> Call<TIn, TOut>(CancellationToken cancellationToken, Action<string> onOutputReceived = null) 
            where TIn : InArg, new()
            where TOut : OutArg, new()
        {
            return Call<TOut>(new TIn(), cancellationToken, onOutputReceived);
        }

        public static async Task<CommandLineCallResult> Call<TOut>(InArg arguments, CancellationToken cancellationToken, Action<string> onOutputReceived = null) where TOut: OutArg, new()
        {
            string inPath = Path.GetTempFileName();
            string outPath = Path.GetTempFileName();
            CommandLineCallResult callResult = new CommandLineCallResult();

            try
            {
                // Serialize and write command arguments to file
                string inContent = JsonConvert.SerializeObject(arguments);
                File.WriteAllText(inPath, inContent);

                List<object> commandArguments = new List<object> {"exec", inPath, outPath};

                if (OperatingSystem.IsMacOS())
                {
                    string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                    if (string.IsNullOrEmpty(path) || !path.Contains("dotnet"))
                        Environment.SetEnvironmentVariable("PATH", $"{path}:/usr/local/share/dotnet:~/.dotnet/tools", EnvironmentVariableTarget.Process);
                }

                using (Command command = Command.Run(Executable, commandArguments, options => options
                    .CancellationToken(cancellationToken)
                    .ThrowOnError()
                    .DisposeOnExit(false)))
                {
                    if (onOutputReceived != null)
                    {
#pragma warning disable 4014
                        Task.Run(async () =>
                        {
                            string line;
                            while ((line = await command.StandardOutput.ReadLineAsync()) != null)
                            {
                                onOutputReceived.Invoke(line);
                            }

                        }, cancellationToken);
#pragma warning restore 4014
                    }

                    await command.Task;

                }

                // Read and deserialize command result from file
                string outContentDec = File.ReadAllText(outPath);
                TOut result = JsonConvert.DeserializeObject<TOut>(outContentDec);

                callResult.Completed = true;
                callResult.CommandResult = result ?? new TOut {Success = false};
                Log.Debug("CLI result: Success={0}; ErrorCode={1}", callResult.CommandResult.Success, callResult.CommandResult.ErrorCode);
            }
            catch (TaskCanceledException ex)
            {
                callResult.Canceled = true;
                callResult.Error = ex;
            }
            catch (AggregateException ex)
            {
                callResult.Failed = true;
                callResult.Error = ex.GetBaseException();
                Log.Ex(callResult.Error);
            }
            catch (Exception ex)
            {
                callResult.Failed = true;
                callResult.Error = ex;
                Log.Ex(callResult.Error);
            }
            finally
            {
                if (File.Exists(inPath)) 
                    File.Delete(inPath);
                if (File.Exists(outPath)) 
                    File.Delete(outPath);
            }

            return callResult;
        }
    }

    internal class CommandLineCallResult
    {
        /// <summary>
        /// Indicates whether the Flutnet CLI command has completed execution due to being canceled.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// Indicates whether an error occured while invoking the Flutnet CLI command or while parsing its result.
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// Indicates whether the Flutnet CLI command has completed execution and its result has been successfully parsed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The exception that has been thrown while invoking the Flutnet CLI command or while parsing its result.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// The result details of the Flutnet CLI command.
        /// </summary>
        public OutArg CommandResult { get; set; }
    }
}