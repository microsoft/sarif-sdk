// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class NotificationLevelConverter : JsonConverter
    {
        public static readonly NotificationLevelConverter Instance = new NotificationLevelConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NotificationLevel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = (string)reader.Value;
            return Enum.Parse(typeof(NotificationLevel), value.Substring(0, 1).ToUpper() + value.Substring(1));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            string notificationLevelString = value.ToString();

            notificationLevelString = notificationLevelString.Substring(0, 1).ToLower() + notificationLevelString.Substring(1);

            writer.WriteRawValue("\"" + notificationLevelString + "\"");
        }
    }
}
