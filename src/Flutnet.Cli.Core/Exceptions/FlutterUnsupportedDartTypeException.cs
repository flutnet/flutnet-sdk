using System;
using System.Collections.Generic;
using System.Text;
using Flutnet.Cli.Core.Dart;

namespace Flutnet.Cli.Core.Exceptions
{
    internal class FlutterUnsupportedDartTypeException : Exception
    {
        public readonly IEnumerable<Type> InvalidTypes;

        public FlutterUnsupportedDartTypeException(params Type[] invalidType) 
            : base(_getErrorMessages(invalidType))
        {
            InvalidTypes = invalidType;
        }

        static string _getErrorMessages(IEnumerable<Type> types)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Type type in types)
            {
                builder.AppendLine(DartSupport.GetUnsupportedDartTypeErrorMessage(type));
            }

            return builder.ToString();
        }
    }
}