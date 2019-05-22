// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Map
{
    /// <summary>
    ///  LongArrayDeltaConverter writes a List&lt;long&gt; as a delta-encoded number array.
    ///  Each value written is the delta to add to the previous decoded value.
    ///  This makes ascending arrays with small differences between values must more compact.
    /// </summary>
    public class LongArrayDeltaConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<long>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            // StartArray
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException($"RelativeLongList must start with StartArray, but found {reader.TokenType}.");
            }

            reader.Read();

            List<long> list = new List<long>();
            long current = 0;

            // Convert the array values from relative to absolute
            while (reader.TokenType == JsonToken.Integer)
            {
                long value = (long)reader.Value;
                current += value;
                list.Add(current);

                reader.Read();
            }

            // EndArray
            if (reader.TokenType != JsonToken.EndArray)
            {
                throw new JsonSerializationException($"RelativeLongList expects an array of integers, but found {reader.TokenType} before EndArray.");
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            List<long> list = (List<long>)value;

            writer.WriteStartArray();

            // Convert the array values from absolute to relative
            long last = 0;
            for (int i = 0; i < list.Count; ++i)
            {
                long current = list[i];
                writer.WriteValue(current - last);
                last = current;
            }

            writer.WriteEndArray();
        }
    }
}
