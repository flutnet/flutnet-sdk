using System;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class CommandLineException : Exception
    {
        public CommandLineException(CommandLineErrorCode errorCode) : base(errorCode.GetDefaultMessage())
        {
            Code = errorCode;
        }

        public CommandLineException(CommandLineErrorCode errorCode, string message) : base(message)
        {
            Code = errorCode;
        }

        public CommandLineException(CommandLineErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            Code = errorCode;
        }

        public CommandLineException(CommandLineErrorCode errorCode, Exception innerException) : base(errorCode.GetDefaultMessage(), innerException)
        {
            Code = errorCode;
        }

        public CommandLineErrorCode Code { get; }
    }
}