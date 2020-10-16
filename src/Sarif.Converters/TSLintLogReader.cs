// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintLogReader : LogReader<TSLintLog>
    {
        private readonly XmlObjectSerializer Serializer;

        public TSLintLogReader()
        {
            Serializer = new DataContractJsonSerializer(typeof(TSLintLog));
        }

        public override TSLintLog ReadLog(Stream input)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));

            using (TextReader streamReader = new StreamReader(input))
            using (JsonReader reader = new JsonTextReader(streamReader))
            {
                JToken rootToken = JToken.ReadFrom(reader);
                rootToken = NormalizeLog(rootToken);
                string normalizedLogContents = rootToken.ToString();
                using (Stream normalizedLogStream = new MemoryStream(Encoding.UTF8.GetBytes(normalizedLogContents)))
                {
                    return (TSLintLog)Serializer.ReadObject(normalizedLogStream);
                }
            }
        }

        // This method transforms all "fix" properties in the input to a standard form
        // by wrapping the property value in an array if it is not already an array.
        //
        // The input is a JSON token representing the entire TSLint log file. The method
        // modifies the input token in place.
        //
        // This method returns the same input value that it modified in place.
        //
        // This is necessary because the TSLint JSON contains multiple patterns for fix, i.e.:
        //
        // "fix":{"innerStart":4429,"innerLength":0,"innerText":"\r\n"}
        // "fix":[{"innerStart":4429,"innerLength":0,"innerText":"\r\n"}]
        // "fix":[{"innerStart":4429,"innerLength":0,"innerText":"\r\n"},{"innerStart":4429,"innerLength":0,"innerText":"\r\n"}]
        //
        // The following pattern also occurs, although the most recent version of the TSLint
        // source code does not appear to support it:
        //
        // "fix": {
        //   "innerRuleName": "no-trailing-whitespace",
        //   "innerReplacements": [
        //     {
        //       "innerStart": 1872,
        //       "innerLength": 4,
        //       "innerText": ""
        //     }
        //   ]
        // }
        //
        // Lacking any documentation on how to interpret this, we treat any objects
        // found within the "innerReplacements" array as if they occurred directly
        // under the "fix" object.
        //
        // This method is marked internal rather than private for the sake of unit tests.
        internal JToken NormalizeLog(JToken rootToken)
        {
            if (rootToken is JArray entries)
            {
                NormalizeEntries(entries);
            }
            else
            {
                throw new Exception(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The root JSON value should be a JArray, but is a {0}.",
                        rootToken.GetType().Name));
            }

            return rootToken;
        }

        private void NormalizeEntries(JArray entries)
        {
            foreach (JToken entryToken in entries)
            {
                if (entryToken is JObject entry)
                {
                    NormalizeEntry(entry);
                }
                else
                {
                    var lineInfo = entryToken as IJsonLineInfo;
                    throw new Exception(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "({0}, {1}): The JSON value should be a JObject, but is a {2}.",
                            lineInfo.LineNumber,
                            lineInfo.LinePosition,
                            entryToken.GetType().Name));
                }
            }
        }

        private static void NormalizeEntry(JObject entry)
        {
            JProperty fixProperty = entry.Properties().SingleOrDefault(p => p.Name.Equals("fix"));
            if (fixProperty != null)
            {
                NormalizeFixProperty(fixProperty);
            }
        }

        private static void NormalizeFixProperty(JProperty fixProperty)
        {
            JToken fixValueToken = fixProperty.Value;
            if (fixValueToken is JObject fixValueObject)
            {
                JProperty innerReplacementsProperty = fixValueObject.Property("innerReplacements");
                if (innerReplacementsProperty?.Value is JArray innerReplacementsArray)
                {
                    fixProperty.Value = innerReplacementsArray;
                }
                else
                {
                    fixProperty.Value = new JArray(fixValueToken);
                }
            }
            else if (!(fixValueToken is JArray))
            {
                var lineInfo = fixValueToken as IJsonLineInfo;
                throw new Exception(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "({0}, {1}): The value of the 'fix' property should be either a JObject or a JArray, but is a {2}.",
                        lineInfo.LineNumber,
                        lineInfo.LinePosition,
                        fixValueToken.GetType().Name));
            }
        }
    }
}
