// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifCurrentToVersionOneTests
    {
        private static readonly JsonSerializerSettings s_v1JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance,
            Formatting = Formatting.Indented
        };

        private static readonly JsonSerializerSettings s_v2JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolver.Instance,
            Formatting = Formatting.Indented
        };

        private static SarifLog GetSarifLog(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLog>(logText, s_v2JsonSettings);
        }

        private static SarifLogVersionOne TransformCurrentToVersionOne(string V2LogText)
        {
            SarifLog v2Log = GetSarifLog(V2LogText);
            var transformer = new SarifCurrentToVersionOneVisitor();
            transformer.VisitSarifLog(v2Log);

            return transformer.SarifLogVersionOne;
        }

        private static void VerifyCurrentToVersionOneTransformation(string V2LogText, string v1LogExpectedText)
        {
            SarifLogVersionOne v1Log = TransformCurrentToVersionOne(V2LogText);
            string v1LogText = JsonConvert.SerializeObject(v1Log, s_v1JsonSettings);
            v1LogText.Should().Be(v1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Minimum()
        {
            const string V2LogText =
              @"{
                  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
                  ""version"": ""2.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": []
                    }
                  ]
                }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": []
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_MinimumWithTwoRuns()
        {
            const string V2LogText =
              @"{
                  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
                  ""version"": ""2.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": []
                    },
                    {
                      ""tool"": {
                        ""name"": ""AssetScanner"",
                        ""semanticVersion"": ""1.7.2""
                      },
                      ""results"": []
                    }
                  ]
                }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": []
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": []
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithLogicalLocations()
        {
            const string V2LogText =
              @"{
                  ""version"": ""2.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""logicalLocations"": {
                        ""collections::list::add"": {
                          ""name"": ""add"",
                          ""kind"": ""function"",
                          ""parentKey"": ""collections::list""
                        },
                        ""collections::list"": {
                          ""name"": ""list"",
                          ""kind"": ""type"",
                          ""parentKey"": ""collections""
                        },
                        ""collections"": {
                          ""name"": ""collections"",
                          ""kind"": ""namespace""
                        }
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""logicalLocations"": {
        ""collections::list::add"": {
          ""name"": ""add"",
          ""parentKey"": ""collections::list"",
          ""kind"": ""function""
        },
        ""collections::list"": {
          ""name"": ""list"",
          ""parentKey"": ""collections"",
          ""kind"": ""type""
        },
        ""collections"": {
          ""name"": ""collections"",
          ""kind"": ""namespace""
        }
      },
      ""results"": []
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }
    }
}
