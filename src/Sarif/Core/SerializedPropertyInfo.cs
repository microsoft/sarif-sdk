// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a single entry in a JSON property bag (a JSON object whose keys have
    /// arbitrary names and whose values may be any JSON values).
    /// </summary>
    [JsonConverter(typeof(SerializedPropertyInfoConverter))]
    public class SerializedPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedPropertyInfo"/> class.
        /// </summary>
        /// <param name="serializedValue">
        /// The string representation of the JSON value of the property.
        /// </param>
        /// <param name="isString">
        /// <code>true</code> if the property is a string; otherwise <code>false</code>.
        /// </param>
        /// <remarks>
        /// This representation allows properties to be read from JSON into memory and
        /// then round-tripped back to JSON, without storing "live" Json.NET objects.
        /// Live JSON objects hold references to their parent container, which would consume
        /// a lot of memory for objects that are part of a large JSON file.
        /// </remarks>
        /// <example>
        /// The integer-value JSON property <code>"n": 42</code>
        /// is represented by
        /// <code>
        /// new SerializedPropertyInfo("42", JTokenType.Integer)
        /// </code>
        /// The string-valued JSON property <code>"s": "abc"</code>
        /// is represented by
        /// <code>
        /// new SerializedPropertyInfo("\"abc\"", JTokenType.String)
        /// </code>
        /// The array-valued JSON property <code>"a": [ 1, "b" ]</code>
        /// is represented by
        /// <code>
        /// new SerializedPropertyInfo("[ 1, \"b\" ]", JTokenType.Array)
        /// </code>
        /// The object-values JSON property <code>"o": { "a": 1, "b": false }</code>
        /// is represented by
        /// <code>
        /// new SerializedPropertyInfo("{ \"a\": 1, \"b\": false }", JTokenType.Object)
        /// </code>
        /// </example>
        public SerializedPropertyInfo(string serializedValue, bool isString)
        {
            SerializedValue = serializedValue;
            IsString = isString;
        }

        /// <summary>
        /// Gets the string representation of the JSON value of the property.
        /// </summary>
        public string SerializedValue { get; }

        /// <summary>
        /// Gets a value indicating whether the property is a string.
        /// </summary>
        /// <remarks>
        /// We need to know that because the <see cref="PropertyBagConverter"/> needs to
        /// put an extra pair of quotes around strings before it writes them out.
        /// </remarks>
        public bool IsString { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SerializedPropertyInfo);
        }

        private const int HashCodeSeedValue = 17;
        private const int HashCodeCombiningValue = 31;

        public override int GetHashCode()
        {
            int result = HashCodeSeedValue;
            result = (result * HashCodeCombiningValue) + IsString.GetHashCode();
            if (SerializedValue != null)
            {
                result = (result * HashCodeCombiningValue) + SerializedValue.GetHashCode();
            }

            return result;
        }

        public bool Equals(SerializedPropertyInfo other)
        {
            if (other == null)
            {
                return false;
            }

            if (IsString != other.IsString)
            {
                return false;
            }

            if (!ReferenceEquals(SerializedValue, other.SerializedValue))
            {
                if (SerializedValue == null || other.SerializedValue == null)
                {
                    return false;
                }

                if (!SerializedValue.Equals(other.SerializedValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
