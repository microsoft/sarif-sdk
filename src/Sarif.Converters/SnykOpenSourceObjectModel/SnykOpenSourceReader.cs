// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters.SnykOpenSourceObjectModel
{
    public class SnykOpenSourceReader : LogReader<List<Test>>
    {
        public override List<Test> ReadLog(Stream input)
        {
            string reportData;

            using (TextReader streamReader = new StreamReader(input))
            {
                reportData = streamReader.ReadToEnd();
            }

            //Parse JSON
            var token = JToken.Parse(reportData);

            //Return object
            var tests = new List<Test>();

            //Check start token for object type
            //Handle appropriately
            if (token is JObject)
            {
                var test = token.ToObject<Test>();
                tests.Add(test);
            }
            else if (token is JArray)
            {
                tests.AddRange(token.ToObject<List<Test>>());
            }

            return tests;
        }
    }
}
