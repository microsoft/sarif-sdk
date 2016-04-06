// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class DateTimeConverter : JsonConverter
    {
        public static readonly DateTimeConverter Instance = new DateTimeConverter();

        // Note: this static property left mutable by design, in case SDK users
        // would like to alter format in some way that still conforms to the 
        // SARIF spec (e.g., a user might want more or less precision).
        private static string s_dateTimeFormat = SarifUtilities.SarifDateTimeFormatCentisecondsPrecision;
        public static string DateTimeFormat
        {
            get { return s_dateTimeFormat; }
            set { s_dateTimeFormat = value; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is DateTime) { return reader.Value; }

            return DateTime.Parse((string)reader.Value, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string formattedDate = ((DateTime)value).ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            writer.WriteRawValue(@"""" + formattedDate + @"""");
        }
    }
}
