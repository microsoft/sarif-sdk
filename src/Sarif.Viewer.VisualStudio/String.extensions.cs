using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    static class StringExtensions
    {
        public static KeyValuePair<string, string> KeyWithValue(this string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
