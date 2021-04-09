using System;
using System.IO;
using System.Linq;
using System.Text;
using Flutnet.Utilities;
using Medallion.Shell;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class FlutnetShell
    {
        public static readonly string CurrentShell;
        public static readonly string CurrentShellConfigurationFile;

        static FlutnetShell()
        {
            if (Utilities.OperatingSystem.IsMacOS())
            {
                CurrentShell = GetCurrentLoginShell();
                if (!string.IsNullOrEmpty(CurrentShell))
                    CurrentShellConfigurationFile = GetShellConfigurationFile(CurrentShell);
            }
            else if (Utilities.OperatingSystem.IsWindows())
            {
                CurrentShell = "cmd";
            }
        }
        
        /// <summary>
        /// Runs a command in Windows Command Prompt.
        /// </summary>
        public static CommandResult RunInWindowsShell(string commandWithArguments, string workingDir, bool verbose = false)
        {
            //NOTE: Please consider defining different levels of verbosity
            //      in order to do specific redirection

            // MedallionShell automatically escapes all the passed arguments.
            // However we need to call "cmd /c" with un-escaped arguments. 
            // The only way to achieve this is explicitly setting the arguments of ProcessStartInfo.
            // https://github.com/madelson/MedallionShell/issues/7

            //Command command = Command.Run("cmd", new[] { "/c", commandWithArguments },
            //    options => { options.WorkingDirectory(workingDir).ThrowOnError(); });

            Command command = Command.Run("cmd",
                options: options =>
                {
                    options.WorkingDirectory(workingDir); 
                    options.ThrowOnError();
                    options.Encoding(Encoding.UTF8);
                    options.StartInfo(psi => psi.Arguments = $"/c {commandWithArguments.Quoted()}");
                });

            if (verbose)
            {
                command.RedirectTo(Console.Out);
                command.RedirectStandardErrorTo(Console.Error);
            }
            command.Wait();
            return command.Result;
        }

        /// <summary>
        /// Runs a command in the available macOS shell.
        /// </summary> 
        public static CommandResult RunInMacOsShell(string commandWithArguments, string workingDir, bool verbose = false)
        {
            //NOTE: Please consider defining different levels of verbosity
            //      in order to do specific redirection

            // Rimossa gestione escape da quando si usa Medallion Shell Library
            //string escapedArgs = commandWithArguments.Replace("\"", "\\\"");
            string escapedArgs = commandWithArguments;

            // First of all we need to detect a suitable shell:
            // both /bin/bash and /bin/zsh have the option -c.
            // Then, we can run the command: we must provide the
            // configuration file for the chosen shell that contains
            // user environment variables including Flutter SDK bin path.

            /*
            FileInfo zshFile = new FileInfo("/bin/zsh");
            FileInfo bashFile = new FileInfo("/bin/bash");

            string executable;
            string arguments;

            if (zshFile.Exists)
            {
                executable = zshFile.FullName;
                arguments = $"source ~/.zshrc && {escapedArgs}";
            }
            else if (bashFile.Exists)
            {
                executable = bashFile.FullName;
                arguments = $"source ~/.bashrc && {escapedArgs}";
            }
            else
            {
                throw new InvalidOperationException("Unable to detect a suitable shell on this machine (neither Bash or Z shell): Cannot execute the command.");
            }
            */

            string executable;
            string arguments;

            if (CurrentShell != null && CurrentShellConfigurationFile != null)
            {
                executable = CurrentShell;
                arguments = $"source {CurrentShellConfigurationFile} && {escapedArgs}";
            }
            else
            {
                throw new InvalidOperationException("Unable to detect a suitable shell on this machine (neither Bash or Z shell): Cannot execute the command.");
            }

            Command command = Command.Run(executable, new[] { "-c", arguments },
                options => { options.WorkingDirectory(workingDir).ThrowOnError(); });
            if (verbose)
            {
                command.RedirectTo(Console.Out);
                command.RedirectStandardErrorTo(Console.Error);
            }

            /*
            try
            {
                command.Wait();
                return command.Result;
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                Log.Error("Unable to execute the command: Please make sure you updated the PATH variable inside your shell configuration file to include Flutter SDK bin path.");
                throw;
            }
            */
            command.Wait();
            return command.Result;
        }

        public static CommandResult RunCommand(string commandWithArguments, string workingDir, bool verbose = false)
        {
            if (Utilities.OperatingSystem.IsMacOS())
            {
                return RunInMacOsShell(commandWithArguments, workingDir, verbose);
            } 

            if (Utilities.OperatingSystem.IsWindows())
            {
                return RunInWindowsShell(commandWithArguments, workingDir, verbose);
            }

            throw new InvalidOperationException("Platform not supported.");
        }

        /// <summary>
        /// Searches for an executable or script in the directories listed in the environment variable PATH.
        /// </summary>
        public static bool TryLocateFile(string executable, out string path)
        {
            try
            {
                if (Utilities.OperatingSystem.IsMacOS())
                {
                    CommandResult result = RunInMacOsShell($"which {executable}", Environment.CurrentDirectory);
                    path = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    return true;
                }

                if (Utilities.OperatingSystem.IsWindows())
                {
                    CommandResult result = RunInWindowsShell($"where {executable}", Environment.CurrentDirectory);
                    path = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    return true;
                }

                throw new InvalidOperationException("Platform not supported.");
            }
            catch (Exception)
            {
                path = default;
                return false;
            }
        }

        /// <summary>
        /// Returns the current login shell (macOS only). Based on: https://stackoverflow.com/a/41553295
        /// </summary>
        private static string GetCurrentLoginShell()
        {
            Command command = Command.Run("/bin/sh",
                arguments: new object[] { "-c", "echo $SHELL" },
                options: options => { options.ThrowOnError(); });

            try
            {
                CommandResult result = command.Result;
                string path = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the default login shell (macOS only). Based on: https://stackoverflow.com/a/41553295
        /// </summary>
        private static string GetDefaultLoginShell()
        {
            Command command = Command.Run("/bin/sh",
                arguments: new object[] { "-c", "dscl . -read ~/ UserShell | sed 's/UserShell: //'" },
                options: options => { options.ThrowOnError(); });

            try
            {
                CommandResult result = command.Result;
                string path = result.StandardOutput.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                return path;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the RC file for the target shell (macOs only).
        /// </summary>
        /// <returns></returns>
        private static string GetShellConfigurationFile(string shell)
        {
            FileInfo fi = new FileInfo(shell);
            if (string.Equals(fi.Name, "zsh", StringComparison.OrdinalIgnoreCase))
            {
                string config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".zshrc");
                //string config = "~/.zshrc";

                return config;
            }
            else if (string.Equals(fi.Name, "bash", StringComparison.OrdinalIgnoreCase))
            {
                string config1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".bashrc");
                //string config1 = "~/.bashrc";

                string config2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".bash_profile");
                //string config2 = "~/.bash_profile";

                return File.Exists(config2) ? config2 : config1;
            }
            return null;
        }
    }
}
