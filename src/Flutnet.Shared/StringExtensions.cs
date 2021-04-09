namespace Flutnet.Utilities
{
    internal static class StringExtensions
    {
        public static string FirstCharUpper(this string value)
        {
            return string.IsNullOrWhiteSpace(value) 
                ? value 
                : value.Length > 1 
                    ? char.ToUpper(value[0]) + value.Substring(1) 
                    : value.ToUpper();
        }

        public static string FirstCharLower(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : value.Length > 1
                    ? char.ToLower(value[0]) + value.Substring(1)
                    : value.ToLower();
        }

        public static string Quoted(this string value)
        {
            return $"\"{value}\"";
        }
    }
}