// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class JsonToSerializedPropertyInfo : JsonConverter
    {
        public static SerializedPropertyInfo Read<TRoot>(JsonReader reader, TRoot root)
        {
            return Read(reader);
        }

        public static SerializedPropertyInfo Read(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                using (StringWriter w = new StringWriter(builder))
                using (JsonTextWriter writer = new JsonTextWriter(w))
                {
                    writer.WriteToken(reader);
                }

                return new SerializedPropertyInfo(builder.ToString(), false);
            }
        }

        public static void Write(JsonWriter writer, SerializedPropertyInfo value)
        {
            SerializedPropertyInfo spi = (SerializedPropertyInfo)value;

            if (spi == null || spi.SerializedValue == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteRawValue(spi.SerializedValue);
            }
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
