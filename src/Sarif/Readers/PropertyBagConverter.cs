// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            var objectDictionary = new Dictionary<string, object>();
            serializer.Populate(reader, objectDictionary);

            var propertyDictionary = new Dictionary<string, SerializedPropertyInfo>();
            foreach (string key in objectDictionary.Keys)
            {
                object value = objectDictionary[key];
                Type propertyType = value.GetType();

                string serializedValue = value.ToString();
                bool isString = false;

                if (propertyType == typeof(bool))
                {
                    serializedValue = serializedValue.ToLowerInvariant();
                }
                else if (propertyType == typeof(string))
                {
                    serializedValue = JsonConvert.ToString(serializedValue);
                    isString = true;
                }

                propertyDictionary.Add(
                    key,
                    new SerializedPropertyInfo(serializedValue, isString));
            }

            return propertyDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteStartObject();
            var propertyDictionary = (Dictionary<string, SerializedPropertyInfo>)value;
            foreach (string key in propertyDictionary.Keys)
            {
                writer.WritePropertyName(key);
                writer.WriteRawValue(propertyDictionary[key].SerializedValue);
            }

            writer.WriteEndObject();
        }
    }
}
