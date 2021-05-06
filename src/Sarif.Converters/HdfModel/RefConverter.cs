// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    internal class RefConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Ref) || t == typeof(Ref?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    string stringValue = serializer.Deserialize<string>(reader);
                    return new Ref { String = stringValue };
                case JsonToken.StartArray:
                    List<Dictionary<string, object>> arrayValue = serializer.Deserialize<List<Dictionary<string, object>>>(reader);
                    return new Ref { AnythingMapArray = arrayValue };
            }
            throw new Exception("Cannot unmarshal type Ref");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Ref)untypedValue;
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            if (value.AnythingMapArray != null)
            {
                serializer.Serialize(writer, value.AnythingMapArray);
                return;
            }
            throw new Exception("Cannot marshal type Ref");
        }

        public static readonly RefConverter Singleton = new RefConverter();
    }
}
