// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  DeferredDictionaryConverter is a JsonConverter which allows reading IDictionary&lt;string, T&gt;
    ///  items only at enumeration time, saving the memory cost of keeping every object around.
    ///  
    ///  The set of keys and the file position in the JSON stream of the values is pre-loaded and kept around
    ///  for fast retrieval.
    /// </summary>
    /// <typeparam name="T">Type of items in the Dictionary</typeparam>
    public class DeferredDictionaryConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IDictionary<string, T>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JsonPositionedTextReader r = reader as JsonPositionedTextReader;
            if (r == null) throw new InvalidOperationException($"DeferredDictionaryConverter requires a JsonPositionedTextReader be used for deserialization.");

            return new DeferredDictionary<T>(serializer, r);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Default Serialization is fine
            serializer.Serialize(writer, value);
        }
    }
}
