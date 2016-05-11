// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var objectDictionary = new Dictionary<string, object>();
            serializer.Populate(reader, objectDictionary);

            var propertyDictionary = new Dictionary<string, SerializedPropertyInfo>();
            foreach (string key in objectDictionary.Keys)
            {
                Type propertyType = objectDictionary[key].GetType();

                JTokenType jTokenType = DotNetTypeToJTokenType(propertyType);
                if (jTokenType == JTokenType.None)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SdkResources.ApplicationException_InvalidJsonPropertyType,
                            key,
                            objectDictionary[key],
                            propertyType));
                }

                string serializedValue = objectDictionary[key].ToString();

                switch (jTokenType)
                {
                    case JTokenType.Boolean:
                        serializedValue = serializedValue.ToLowerInvariant();
                        break;

                    case JTokenType.String:
                        serializedValue = '"' + serializedValue + '"';
                        break;

                    default:
                        break;
                }

                propertyDictionary.Add(
                    key,
                    new SerializedPropertyInfo(serializedValue, jTokenType));
            }

            return propertyDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var propertyDictionary = (Dictionary<string, SerializedPropertyInfo>)value;
            foreach (string key in propertyDictionary.Keys)
            {
                writer.WritePropertyName(key);
                writer.WriteRawValue(propertyDictionary[key].SerializedValue);
            }

            writer.WriteEndObject();
        }

        public static JTokenType DotNetTypeToJTokenType(Type dotNetType)
        {
            JTokenType jTokenType = JTokenType.None;

            if (dotNetType == typeof(JObject))
            {
                jTokenType = JTokenType.Object;
            }
            else if (dotNetType == typeof(JArray))
            {
                jTokenType = JTokenType.Array;
            }
            else if (dotNetType == typeof(bool))
            {
                jTokenType = JTokenType.Boolean;
            }
            else if (dotNetType == typeof(long))
            {
                jTokenType = JTokenType.Integer;
            }
            else if (dotNetType == typeof(double))
            {
                jTokenType = JTokenType.Float;
            }
            else if (dotNetType == typeof(string))
            {
                jTokenType = JTokenType.String;
            }

            return jTokenType;
        }
    }
}
