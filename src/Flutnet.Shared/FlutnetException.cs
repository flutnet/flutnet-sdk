using System;
using System.Reflection;
using Flutnet.ServiceModel;

namespace Flutnet.Data
{
    [Obfuscation(Exclude = true)]
    internal class FlutnetException : PlatformOperationException
    {
        public FlutnetException(FlutnetErrorCode errorCode)
        {
            Code = errorCode;
        }
        public FlutnetException(FlutnetErrorCode errorCode, string message) : base(message)
        {
            Code = errorCode;
        }
        public FlutnetException(FlutnetErrorCode errorCode, string message, Exception innerException) : base(message, innerException)
        {
            Code = errorCode;
        }

        public FlutnetErrorCode Code { get; }
    }
}