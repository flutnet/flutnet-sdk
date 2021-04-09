using System;
using System.Collections.Generic;
using System.Text;

namespace Flutnet.Cli.Core.Exceptions
{
    internal class MultipleException : Exception
    {
        private readonly List<Exception> _operationExceptions;

        public MultipleException(List<Exception> operationExceptions) : base(_getErrorMessages(operationExceptions))
        {
            _operationExceptions = operationExceptions;
        }

        static string _getErrorMessages(List<Exception> exceptions)
        {
            StringBuilder builder = new StringBuilder();

            for (var i = 0; i < exceptions.Count; i++)
            {
                Exception ex = exceptions[i];
                builder.AppendLine($"{i}. {ex.Message}");
            }

            return builder.ToString();
        }
    }
}
