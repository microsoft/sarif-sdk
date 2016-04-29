// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class EnumConverter : JsonConverter
    {
        public static readonly EnumConverter Instance = new EnumConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ResultLevel)
                || objectType == typeof(NotificationLevel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            return Enum.Parse(objectType, value.Substring(0, 1).ToUpper() + value.Substring(1));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            string resultLevelString = value.ToString();

            resultLevelString = resultLevelString.Substring(0, 1).ToLower() + resultLevelString.Substring(1);

            writer.WriteRawValue("\"" + resultLevelString + "\"");
        }
    }
}
