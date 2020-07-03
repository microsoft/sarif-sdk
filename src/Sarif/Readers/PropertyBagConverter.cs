// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    /// Converts a property bag (a JSON object whose keys have arbitrary names and whose values
    /// may be any JSON values) into a dictionary whose keys match the JSON object's
    /// property names, and whose values are of type <see cref="SerializedPropertyInfo"/>
    /// </summary>
    internal class PropertyBagConverter : JsonConverter
    {
        internal static readonly JsonConverter Instance = new PropertyBagConverter();

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, SerializedPropertyInfo>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IDictionary<string, SerializedPropertyInfo> dictionary = (existingValue as IDictionary<string, SerializedPropertyInfo> ?? new Dictionary<string, SerializedPropertyInfo>());

            reader.Read();

            while(reader.TokenType == JsonToken.PropertyName)
            {
                string name = (string)reader.Value;
                reader.Read();

                SerializedPropertyInfo value = SerializedPropertyInfoConverter.Read(reader);
                reader.Read();

                dictionary[name] = value;
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            var propertyDictionary = (IDictionary<string, SerializedPropertyInfo>)value;
            foreach (KeyValuePair<string, SerializedPropertyInfo> pair in propertyDictionary)
            {
                writer.WritePropertyName(pair.Key);
                SerializedPropertyInfoConverter.Write(writer, pair.Value);
            }

            writer.WriteEndObject();
        }
    }
}
