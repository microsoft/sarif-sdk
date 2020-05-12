// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class UriConverter : JsonConverter
    {
        public static readonly UriConverter Instance = new UriConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri) ||
                   objectType == typeof(IDictionary<string, Uri>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.Value is Uri) { return reader.Value; }

            if (objectType == typeof(IList<Uri>) ||
                objectType == typeof(IDictionary<string, Uri>))
            {
                return serializer.Deserialize(reader, objectType);
            }

            return new Uri((string)reader.Value, UriKind.RelativeOrAbsolute);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var uri = value as Uri;
            if (uri != null && !string.IsNullOrWhiteSpace(uri.OriginalString))
            {
                writer.WriteValue(uri.OriginalString);
                return;
            }

            var uriList = value as IList<Uri>;
            if (uriList != null)
            {
                serializer.Serialize(writer, value, typeof(IList<Uri>));
                return;
            }

            serializer.Serialize(writer, value, typeof(IDictionary<string, Uri>));
        }
    }
}
