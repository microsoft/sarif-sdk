// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class AnnotatedCodeLocationKindConverterTests : JsonTests
    {
        [Fact]
        public void AnnotatedCodeLocationKind_AllMembers()
        {
            var testTuples = new List<Tuple<AnnotatedCodeLocationKind, string>>();

            foreach (string name in Enum.GetNames(typeof(AnnotatedCodeLocationKind)))
            {
                var kind = (AnnotatedCodeLocationKind)Enum.Parse(typeof(AnnotatedCodeLocationKind), name);

                if (kind == 0) { continue; }

                string serializedValue = EnumConverter.ConvertToCamelCase(name);
                testTuples.Add(new Tuple<AnnotatedCodeLocationKind, string>(kind, serializedValue));
            }

            foreach (var testTuple in testTuples)
            {
                string expected =
@"{
  ""$schema"": """ + SarifSchemaUri + @""",
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {
          ""codeFlows"": [
            {
              ""locations"": [
                {
                  ""physicalLocation"": {
                    ""uri"": ""file:///c:/test.c""
                  },
                  ""kind"": """ +  testTuple.Item2 + @"""
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}";

                string actual = GetJson(uut =>
                {
                    var run = new Run();

                    uut.Initialize(id: null, automationId: null);

                    uut.WriteTool(DefaultTool);

                    var result = new Result
                    {
                        CodeFlows = new CodeFlow[]
                        {
                            new CodeFlow
                            {
                                Locations = new AnnotatedCodeLocation[]
                                {
                                    new AnnotatedCodeLocation
                                    {
                                        PhysicalLocation = new PhysicalLocation
                                        {
                                            Uri = new Uri(@"c:\test.c", UriKind.Absolute),
                                        },
                                        Kind = testTuple.Item1
                                    }
                                }
                            }
                        }
                    };

                    uut.WriteResults(new[] { result });
                });
                Assert.Equal(expected, actual);

                var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
                Assert.Equal(testTuple.Item1, sarifLog.Runs[0].Results[0].CodeFlows[0].Locations[0].Kind);
            }
        }
    }
}
