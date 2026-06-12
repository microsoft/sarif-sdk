// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// Defensive reads of <c>ai/evidence</c> entry properties. Producers in the
    /// wild emit some properties (e.g. <c>backing</c>) as either a single
    /// string or as an array of strings; a validator rule must accept both
    /// shapes without throwing on well-formed input.
    /// </summary>
    internal static class EvidenceJsonReader
    {
        /// <summary>
        /// Parses the serialized <c>ai/evidence</c> property value as a JSON array.
        /// Returns <c>false</c> when <paramref name="evidenceJson"/> is not a well-formed
        /// JSON array — malformed JSON, or a well-formed but wrong-shaped token (object,
        /// string, number). That producer defect is reported once, by <c>AI2016</c>;
        /// every other evidence rule calls this and skips cleanly on <c>false</c> so the
        /// malformed log yields a single diagnostic rather than silence.
        /// </summary>
        public static bool TryParseEvidenceArray(string evidenceJson, out JArray evidenceArray)
        {
            try
            {
                evidenceArray = JArray.Parse(evidenceJson);
                return true;
            }
            catch (JsonReaderException)
            {
                evidenceArray = null;
                return false;
            }
        }

        /// <summary>
        /// Reads <paramref name="propertyName"/> from <paramref name="entry"/>
        /// as a string. Returns null if the property is absent or not a JSON
        /// string token (i.e., array, object, number, boolean, null).
        /// </summary>
        public static string ReadString(JObject entry, string propertyName)
        {
            JToken token = entry[propertyName];
            return token != null && token.Type == JTokenType.String
                ? token.Value<string>()
                : null;
        }

        /// <summary>
        /// Reads <paramref name="propertyName"/> from <paramref name="entry"/>
        /// as a list of strings. Accepts both shapes:
        /// <list type="bullet">
        ///   <item>a single JSON string (yields a one-element list);</item>
        ///   <item>a JSON array of strings (non-string array elements are silently dropped).</item>
        /// </list>
        /// Returns an empty list when the property is absent, null-valued, or any
        /// other JSON shape (object, number, boolean).
        /// </summary>
        public static IReadOnlyList<string> ReadStrings(JObject entry, string propertyName)
        {
            JToken token = entry[propertyName];
            if (token == null)
            {
                return Array.Empty<string>();
            }

            if (token.Type == JTokenType.String)
            {
                return new[] { token.Value<string>() };
            }

            if (token is JArray array)
            {
                var values = new List<string>(array.Count);
                foreach (JToken item in array)
                {
                    if (item.Type == JTokenType.String)
                    {
                        values.Add(item.Value<string>());
                    }
                }

                return values;
            }

            return Array.Empty<string>();
        }
    }
}
