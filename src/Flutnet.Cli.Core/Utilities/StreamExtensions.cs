using System.IO;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class StreamExtensions
    {
        public static void WriteTo(this Stream input, string filepath)
        {
            //your fav write method:
            /*
            using (var stream = File.Create(file))
            {
                input.CopyTo(stream);
            }
            */
            //or

            using (var stream = new MemoryStream())
            {
                input.CopyTo(stream);
                File.WriteAllBytes(filepath, stream.ToArray());
            }

            //whatever that fits.
        }
    }
}