// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SerializedPropertyInfoConverter : JsonConverter
    {
        public static readonly SerializedPropertyInfoConverter Instance = new SerializedPropertyInfoConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SerializedPropertyInfo);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var serializedPropertyInfo = (SerializedPropertyInfo)reader.Value;
            return serializedPropertyInfo.SerializedValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            string serializedValue = ((SerializedPropertyInfo)value).SerializedValue;

            if (serializedValue.StartsWith("\""))
            {
                writer.WriteRawValue(serializedValue);
            }
            else
            {
                writer.WriteRawValue(@"""" + serializedValue + @"""");
            }
        }
    }
}
