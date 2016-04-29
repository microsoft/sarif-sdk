// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

using Microsoft.CodeAnalysis.Sarif.Sdk;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class IsSuppressedInSourceConverter : JsonConverter
    {
        public static readonly IsSuppressedInSourceConverter Instance = new IsSuppressedInSourceConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IsSuppressedInSource);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch ((string)reader.Value)
            {
                case "suppressed": return IsSuppressedInSource.Suppressed;
                case "notSuppressed": return IsSuppressedInSource.NotSuppressed;
            }

            return IsSuppressedInSource.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch ((IsSuppressedInSource)value)
            {
                case IsSuppressedInSource.Suppressed: { writer.WriteRawValue("\"suppressed\""); return; }
                case IsSuppressedInSource.NotSuppressed: { writer.WriteRawValue("\"notSuppressed\""); return; }
            }
            writer.WriteRawValue(@"""unknown""");
        }
    }
}
