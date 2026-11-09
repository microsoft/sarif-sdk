// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SerializedPropertyInfoConverter : JsonConverter
    {
        public static SerializedPropertyInfo Read(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                bool wasString = reader.TokenType == JsonToken.String;

                var builder = new StringBuilder();
                using (var w = new StringWriter(builder))
                using (var writer = new JsonTextWriter(w))
                {
                    writer.WriteToken(reader);
                }

                return new SerializedPropertyInfo(builder.ToString(), wasString);
            }
        }

        public static void Write(JsonWriter writer, SerializedPropertyInfo value)
        {
            SerializedPropertyInfo spi = value;

            if (spi == null || spi.SerializedValue == null)
            {
                writer.WriteNull();
            }
            else
            {
                if (writer.StringEscapeHandling == StringEscapeHandling.Default)
                {
                    writer.WriteRawValue(spi.SerializedValue);
                }
                else
                {
                    writer.WriteRawValue(
                        EscapeStrings(spi.SerializedValue, writer.StringEscapeHandling));
                }
            }
        }

        private static string EscapeStrings(
            string serializedValue,
            StringEscapeHandling stringEscapeHandling)
        {
            var builder = new StringBuilder(serializedValue.Length);

            for (int index = 0; index < serializedValue.Length; index++)
            {
                if (serializedValue[index] != '"')
                {
                    builder.Append(serializedValue[index]);
                    continue;
                }

                int startIndex = index;
                do
                {
                    index = serializedValue.IndexOf('"', index + 1);
                    if (index < 0)
                    {
                        throw new JsonReaderException("Unterminated string in serialized property value.");
                    }
                }
                while (IsEscaped(serializedValue, index));

                string jsonString = serializedValue.Substring(startIndex, index - startIndex + 1);
                string value = JsonConvert.DeserializeObject<string>(jsonString);
                builder.Append(JsonConvert.ToString(value, '"', stringEscapeHandling));
            }

            return builder.ToString();
        }

        private static bool IsEscaped(string value, int quoteIndex)
        {
            int precedingBackslashCount = 0;
            for (int index = quoteIndex - 1; index >= 0 && value[index] == '\\'; index--)
            {
                precedingBackslashCount++;
            }

            return precedingBackslashCount % 2 != 0;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SerializedPropertyInfo);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Read(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Write(writer, (SerializedPropertyInfo)value);
        }
    }
}
