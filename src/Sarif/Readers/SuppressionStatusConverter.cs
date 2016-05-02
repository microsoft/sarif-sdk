// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

using Microsoft.CodeAnalysis.Sarif.Sdk;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SuppressionStatesConverter : JsonConverter
    {
        public static readonly SuppressionStatesConverter Instance = new SuppressionStatesConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SuppressionStates);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            return Enum.Parse(typeof(SuppressionStates), value.Substring(0, 1).ToUpper() + value.Substring(1));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string suppressionStates = value.ToString();

            suppressionStates = suppressionStates.Substring(0, 1).ToLower() + suppressionStates.Substring(1);

            writer.WriteRawValue("\"" + suppressionStates + "\"");
        }
    }
}
