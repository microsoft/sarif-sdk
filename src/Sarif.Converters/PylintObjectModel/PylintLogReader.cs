using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PylintObjectModel
{
    public static class PylintLogReader
    {
        public static PylintLog ReadLog(string input)
        {
            return ReadLog(input, Encoding.UTF8);
        }

        public static PylintLog ReadLog(string input, Encoding encoding)
        {
            return ReadLog(new MemoryStream(encoding.GetBytes(input)));
        }

        public static PylintLog ReadLog(Stream input)
        {
            string PylintText;

            using (TextReader streamReader = new StreamReader(input))
            {
                PylintText = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<PylintLog>(PylintText);
        }
    }
}