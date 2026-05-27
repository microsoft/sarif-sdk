// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    internal class MinMaxValueCheckConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(double) || t == typeof(double?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            double value = serializer.Deserialize<double>(reader);
            return value >= 0 && value <= 1 ? value : throw new Exception("Cannot unmarshal type double");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            double value = (double)untypedValue;
            if (value >= 0 && value <= 1)
            {
                serializer.Serialize(writer, value);
                return;
            }
            throw new Exception("Cannot marshal type double");
        }

        public static readonly MinMaxValueCheckConverter Singleton = new MinMaxValueCheckConverter();
    }
}
