using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flutnet.Cli.Core.Utilities
{
    internal static class StreamUtils
    {
        /// <summary>
        /// Read full stream and put into a byte array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Read lines in a stream.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">If null will be used UTF8</param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(Stream input, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            using (var reader = new StreamReader(input, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        /// <summary>
        /// Copy stream to a file
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="destPath"></param>
        public static void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}