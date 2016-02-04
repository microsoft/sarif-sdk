using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal static class ConsoleUtil
    {
        internal static void WriteError(string message, params object[] args)
        {
            WriteColor(ConsoleColor.Red, message, args);
        }

        internal static void WriteSuccess(string message, params object[] args)
        {
            WriteColor(ConsoleColor.Green, message, args);
        }

        private static void WriteColor(ConsoleColor color, string message, params object[] args)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            try
            {
                Console.Error.WriteLine(
                    string.Format(CultureInfo.CurrentCulture, message, args));
            }
            finally
            {
                Console.ForegroundColor = prevColor;
            }
        }
    }
}
