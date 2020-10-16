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
                // Absolute file-scheme URIs need special treatment. OriginalString might be a file
                // system path such as "C:\test\file.c", which is not a valid URI. In that case we
                // must instead serialize "file:///C:/test/file.c", which we can get from AbsoluteUri.
                //
                // However, if OriginalString starts with "file:", we do want to serialize it. It
                // might (for example) be "file:///C:/dir1/dir2/..", in which case AbsolutePath would
                // be "file:///C:/dir1". We don't want to lose the dot-dot segment when round-tripping
                // the URI, if for no other reason than that we might want to run the SARIF validator
                // on the result of the round trip, and we don't want to lose the warning that you
                // shouldn't use dot-dot segments!
                bool useAbsoluteUri =
                    uri.IsAbsoluteUri &&
                    uri.Scheme.Equals(UriUtilities.FileScheme, StringComparison.Ordinal) &&
                    !uri.OriginalString.StartsWith(UriUtilities.FileScheme.WithColon(), StringComparison.Ordinal);
                
                string serializedValue = useAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString;

                writer.WriteValue(serializedValue);
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
