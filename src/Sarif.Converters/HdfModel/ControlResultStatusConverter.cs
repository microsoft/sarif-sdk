// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    internal class ControlResultStatusConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ControlResultStatus) || t == typeof(ControlResultStatus?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            string value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "error" => ControlResultStatus.Error,
                "failed" => ControlResultStatus.Failed,
                "passed" => ControlResultStatus.Passed,
                "skipped" => ControlResultStatus.Skipped,
                _ => throw new Exception("Cannot unmarshal type ControlResultStatus"),
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (ControlResultStatus)untypedValue;
            switch (value)
            {
                case ControlResultStatus.Error:
                    serializer.Serialize(writer, "error");
                    return;
                case ControlResultStatus.Failed:
                    serializer.Serialize(writer, "failed");
                    return;
                case ControlResultStatus.Passed:
                    serializer.Serialize(writer, "passed");
                    return;
                case ControlResultStatus.Skipped:
                    serializer.Serialize(writer, "skipped");
                    return;
            }
            throw new Exception("Cannot marshal type ControlResultStatus");
        }

        public static readonly ControlResultStatusConverter Singleton = new ControlResultStatusConverter();
    }
}
