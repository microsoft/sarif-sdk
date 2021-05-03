// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    internal class SkippedDueToWaiverConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(SkippedDueToWaiver) || t == typeof(SkippedDueToWaiver?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    bool boolValue = serializer.Deserialize<bool>(reader);
                    return new SkippedDueToWaiver { Bool = boolValue };
                case JsonToken.String:
                case JsonToken.Date:
                    string stringValue = serializer.Deserialize<string>(reader);
                    return new SkippedDueToWaiver { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type SkippedDueToWaiver");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (SkippedDueToWaiver)untypedValue;
            if (value.Bool != null)
            {
                serializer.Serialize(writer, value.Bool.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type SkippedDueToWaiver");
        }

        public static readonly SkippedDueToWaiverConverter Singleton = new SkippedDueToWaiverConverter();
    }
}
