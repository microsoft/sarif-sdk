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
        public static string DateTimeFormat { get; set; } = SarifUtilities.SarifDateTimeFormatMillisecondsPrecision;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.Value is DateTime) { return reader.Value; }

            // The AdjustToUniversal style is important. Without it, we don't properly round-trip DateTime values.
            // Here's what happens. We start with a property like "startTimeUtc": "2019-07-08T19:56:46.144Z".
            // Without the AdjustToUniversal style, DateTime.Parse converts to local time (in Redmond, subtracting
            // 7 hours), resulting in a DateTime object whose Hours property is 12 and whose Kind property is
            // Local. Then, when we later reserialize the property, our default date/time format (which is defined in
            // SarifUtilities.SarifDateTimeFormatMillisecondsPrecision, and which ends in a "Z") produces
            // "2019-07-08T12:56:46.144Z". Observe we now have "12" instead of "19" in the hours position.
            //
            // With the AdjustToUniversal style, again starting with "startTimeUtc": "2019-07-08T19:56:46.144Z",
            // DateTime.Parse produces a DateTime object whose Hours property is 19 and whose Kind property is Utc.
            // Now when we reserialize, we correctly recover "2019-07-08T19:56:46.144Z".
            //
            // None of this mattered before we fixed https://github.com/microsoft/sarif-sdk/issues/1577,
            // "PrereleaseCompatibilityTransformer mishandles date/times in property bag". Before that, during
            // deserialization, we were (implicitly) using Newtonsoft.Json's default date/time parsing behavior,
            // DateParseHandling.DateTime. With this behavior, when NS.Json encountered a property value that
            // _looked like_ a date/time, it _automatically_ parsed it into a DateTime object. So on the line
            // above, reader.Value was already a DateTime, we returned that value, and this call to DateTime.Parse
            // was never executed. However, that date/time parsing behavior led to a bug in handling date/time
            // values in property bags, as explained in #1577. To fix it, we began to explicitly specify
            // DateParseHandling.None during deserialization. Now NS.Json left date/time-like strings alone,
            // meaning that this converter now received that string, exposing the bug in our call to DateTime.Parse.
            return DateTime.Parse((string)reader.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            string formattedDate = ((DateTime)value).ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            writer.WriteRawValue(@"""" + formattedDate + @"""");
        }
    }
}
