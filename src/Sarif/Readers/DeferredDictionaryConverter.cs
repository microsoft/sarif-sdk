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
            return typeof(IDictionary<string, T>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader is JsonInnerTextReader)
            {
                // Nested in another deferred container, read the dictionary non-deferred
                return serializer.Deserialize<Dictionary<string, T>>(reader);
            }

            // If we don't have a positioned reader, we must return an error
            JsonPositionedTextReader r = reader as JsonPositionedTextReader;
            if (r == null)
            {
                throw new InvalidOperationException($"{nameof(DeferredDictionaryConverter<T>)} requires a {nameof(JsonPositionedTextReader)} be used for deserialization.");
            }

            return new DeferredDictionary<T>(serializer, r);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Default Serialization is fine
            serializer.Serialize(writer, value);
        }
    }
}
