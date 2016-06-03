// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class UriConverter : JsonConverter
    {
        public static readonly UriConverter Instance = new UriConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.Value is Uri) { return reader.Value; }

            return new Uri((string)reader.Value, UriKind.RelativeOrAbsolute);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Uri uri = ((Uri)value);

            if (uri.IsAbsoluteUri && uri.IsFile && (uri.LocalPath == uri.OriginalString))
            {
                uri = new Uri(uri.ToString());
            }

            writer.WriteValue(uri.ToString());
        }
    }
}
