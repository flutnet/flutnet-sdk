using System;

namespace Flutnet.Cli.Core.Infrastructure
{
    internal class Log
    {
        public static void Debug(string text)
        {
            // TODO
            //Console.WriteLine(text);
        }

        public static void Debug(string format, params object[] args)
        {
            // TODO
            //Console.WriteLine(text);
        }

        public static void Ex(Exception e)
        {
            // TODO
            //Console.Error.WriteLine(e);
        }

        public static void Error(string text)
        {
            // TODO
            //Console.Error.WriteLine(text);
        }

        public static void Error(string format, params object[] args)
        {
            // TODO
            //Console.Error.WriteLine(format, args);
        }
    }
}