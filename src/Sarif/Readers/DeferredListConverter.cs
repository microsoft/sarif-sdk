// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  DeferredListConverter is a JsonConverter which allows reading a List&lt;T&gt;
    ///  only at enumeration time, saving the memory cost of keeping every object around.
    ///  
    ///  The position of each item is saved on the first random access, so that later use
    ///  of List[index] is fast.
    /// </summary>
    /// <typeparam name="T">Type of items in the List</typeparam>
    public class DeferredListConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IList<T>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader is JsonInnerTextReader)
            {
                // Nested in another deferred container, read the list non-deferred
                return serializer.Deserialize<List<T>>(reader);
            }

            // If we don't have a positioned reader, we must return an error
            JsonPositionedTextReader r = reader as JsonPositionedTextReader;
            if (r == null)
            {
                throw new InvalidOperationException($"{nameof(DeferredListConverter<T>)} requires a {nameof(JsonPositionedTextReader)} be used for deserialization.");
            }

            return new DeferredList<T>(serializer, r);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Default Serialization is fine
            serializer.Serialize(writer, value);
        }
    }
}
