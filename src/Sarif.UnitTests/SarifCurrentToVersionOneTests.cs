// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifCurrentToVersionOneTests
    {
        private static SarifLog GetSarifLog(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLog>(logText, SarifTransformerUtilities.JsonSettingsV2);
        }

        private static SarifLogVersionOne TransformCurrentToVersionOne(string v2LogText)
        {
            SarifLog v2Log = GetSarifLog(v2LogText);
            var transformer = new SarifCurrentToVersionOneVisitor();
            transformer.VisitSarifLog(v2Log);

            return transformer.SarifLogVersionOne;
        }

        private static void VerifyCurrentToVersionOneTransformation(string v2LogText, string v1LogExpectedText)
        {
            SarifLogVersionOne v1Log = TransformCurrentToVersionOne(v2LogText);
            string v1LogText = JsonConvert.SerializeObject(v1Log, SarifTransformerUtilities.JsonSettingsV1);
            v1LogText.Should().Be(v1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_RestoreFromPropertyBag()
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
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""results\"":[]}""
      }
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
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""results\"":[]}""
      }
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
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""results\"":[]}""
      }
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""AssetScanner\"",\""semanticVersion\"":\""1.7.2\""},\""results\"":[]}""
      }
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
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""logicalLocations\"":{\""collections::list::add\"":{\""name\"":\""add\"",\""parentKey\"":\""collections::list\"",\""kind\"":\""function\""},\""collections::list\"":{\""name\"":\""list\"",\""parentKey\"":\""collections\"",\""kind\"":\""type\""},\""collections\"":{\""name\"":\""collections\"",\""kind\"":\""namespace\""}},\""results\"":[]}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithFiles()
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
      ""files"": {
        ""file:///home/list.txt"": {
          ""length"": 43,
          ""mimeType"": ""text/plain"",
          ""contents"": {
            ""text"": ""The quick brown fox jumps over the lazy dog"",
            ""binary"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==""
          },
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha-256""
            }
          ]
        },
        ""file:///home/buildAgent/bin/app.zip"": {
          ""mimeType"": ""application/zip"",
          ""properties"": {
            ""my_key"": ""some value""
          }
        },
        ""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"": {
          ""fileLocation"": {
            ""uri"": ""file:///docs/intro.docx""
          },
          ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
          ""offset"": 17522,
          ""length"": 4050,
          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"",
          ""contents"": {}
        }
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
      ""files"": {
        ""file:///home/list.txt"": {
          ""length"": 43,
          ""mimeType"": ""text/plain"",
          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha256""
            }
          ]
        },
        ""file:///home/buildAgent/bin/app.zip"": {
          ""mimeType"": ""application/zip"",
          ""properties"": {
            ""my_key"": ""some value""
          }
        },
        ""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"": {
          ""uri"": ""file:///docs/intro.docx"",
          ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
          ""offset"": 17522,
          ""length"": 4050,
          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""files\"":{\""file:///home/list.txt\"":{\""length\"":43,\""mimeType\"":\""text/plain\"",\""contents\"":{\""text\"":\""The quick brown fox jumps over the lazy dog\"",\""binary\"":\""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==\""},\""hashes\"":[{\""value\"":\""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592\"",\""algorithm\"":\""sha-256\""}]},\""file:///home/buildAgent/bin/app.zip\"":{\""mimeType\"":\""application/zip\"",\""properties\"":{\""my_key\"":\""some value\""}},\""file:///home/buildAgent/bin/app.zip#/docs/intro.docx\"":{\""fileLocation\"":{\""uri\"":\""file:///docs/intro.docx\""},\""parentKey\"":\""file:///home/buildAgent/bin/app.zip\"",\""offset\"":17522,\""length\"":4050,\""mimeType\"":\""application/vnd.openxmlformats-officedocument.wordprocessingml.document\"",\""contents\"":{}}},\""results\"":[]}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithRules()
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
      ""results"": [],
      ""resources"": {
        ""rules"": {
          ""C2001"": {
            ""id"": ""C2001"",
            ""shortDescription"": {
              ""text"": ""A variable was used without being initialized.""
            },
            ""messageStrings"": {
              ""default"": ""Variable \""{0}\"" was used without being initialized.""
            },
            ""properties"": {
              ""some_key"": ""FoxForceFive""
            }
          },
          ""C2002"": {
            ""id"": ""C2002"",
            ""fullDescription"": {
              ""text"": ""Catfish season continuous hen lamb include dose copy grant.""
            },
            ""configuration"": {
              ""enabled"": true,
              ""defaultLevel"": ""error""
            },
            ""helpLocation"": {
              ""uri"": ""http://www.domain.com/rules/c2002.html""
            }
          },
          ""C2003"": {
            ""id"": ""C2003"",
            ""name"": {
              ""text"": ""Rule C2003""
            },
            ""shortDescription"": {
              ""text"": ""Rules were meant to be broken.""
            },
            ""fullDescription"": {
              ""text"": ""Rent internal rebellion competence biography photograph.""
            }
          }
        }
      }
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
      ""results"": [],
      ""rules"": {
        ""C2001"": {
          ""id"": ""C2001"",
          ""shortDescription"": ""A variable was used without being initialized."",
          ""messageFormats"": {
            ""default"": ""Variable \""{0}\"" was used without being initialized.""
          },
          ""properties"": {
            ""some_key"": ""FoxForceFive""
          }
        },
        ""C2002"": {
          ""id"": ""C2002"",
          ""fullDescription"": ""Catfish season continuous hen lamb include dose copy grant."",
          ""configuration"": ""enabled"",
          ""defaultLevel"": ""error"",
          ""helpUri"": ""http://www.domain.com/rules/c2002.html""
        },
        ""C2003"": {
          ""id"": ""C2003"",
          ""name"": ""Rule C2003"",
          ""shortDescription"": ""Rules were meant to be broken."",
          ""fullDescription"": ""Rent internal rebellion competence biography photograph.""
        }
      },
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""results\"":[],\""resources\"":{\""rules\"":{\""C2001\"":{\""id\"":\""C2001\"",\""shortDescription\"":{\""text\"":\""A variable was used without being initialized.\""},\""messageStrings\"":{\""default\"":\""Variable \\\""{0}\\\"" was used without being initialized.\""},\""properties\"":{\""some_key\"":\""FoxForceFive\""}},\""C2002\"":{\""id\"":\""C2002\"",\""fullDescription\"":{\""text\"":\""Catfish season continuous hen lamb include dose copy grant.\""},\""configuration\"":{\""enabled\"":true,\""defaultLevel\"":\""error\""},\""helpLocation\"":{\""uri\"":\""http://www.domain.com/rules/c2002.html\""}},\""C2003\"":{\""id\"":\""C2003\"",\""name\"":{\""text\"":\""Rule C2003\""},\""shortDescription\"":{\""text\"":\""Rules were meant to be broken.\""},\""fullDescription\"":{\""text\"":\""Rent internal rebellion competence biography photograph.\""}}}}}""
      }
    }
  ]
}";
            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithBasicInvocation()
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
      ""invocations"": [
        {
          ""commandLine"": ""CodeScanner @collections.rsp"",
          ""responseFiles"": [
            {
              ""uri"": ""collections.rsp""
            }
          ],
          ""startTime"": ""2016-07-16T14:18:25.000Z"",
          ""endTime"": ""2016-07-16T14:19:01.000Z"",
          ""machine"": ""BLD01"",
          ""account"": ""buildAgent"",
          ""processId"": 1218,
          ""executableLocation"": {
            ""uri"": ""/bin/tools/CodeScanner""
          },
          ""workingDirectory"": ""/home/buildAgent/src"",
          ""environmentVariables"": {
            ""PATH"": ""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin"",
            ""HOME"": ""/home/buildAgent"",
            ""TZ"": ""EST""
          }
        }
      ],
      ""files"": {
        ""collections.rsp"": {
          ""fileLocation"": {
            ""uri"": ""collections.rsp""
          },
          ""contents"": {
            ""text"": ""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999""
          }
        }
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
      ""invocation"": {
        ""commandLine"": ""CodeScanner @collections.rsp"",
        ""responseFiles"": {
          ""collections.rsp"": ""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999""
        },
        ""startTime"": ""2016-07-16T14:18:25.000Z"",
        ""endTime"": ""2016-07-16T14:19:01.000Z"",
        ""machine"": ""BLD01"",
        ""account"": ""buildAgent"",
        ""processId"": 1218,
        ""fileName"": ""/bin/tools/CodeScanner"",
        ""workingDirectory"": ""/home/buildAgent/src"",
        ""environmentVariables"": {
          ""PATH"": ""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin"",
          ""HOME"": ""/home/buildAgent"",
          ""TZ"": ""EST""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""invocations\"":[{\""commandLine\"":\""CodeScanner @collections.rsp\"",\""responseFiles\"":[{\""uri\"":\""collections.rsp\""}],\""startTime\"":\""2016-07-16T14:18:25.000Z\"",\""endTime\"":\""2016-07-16T14:19:01.000Z\"",\""machine\"":\""BLD01\"",\""account\"":\""buildAgent\"",\""processId\"":1218,\""executableLocation\"":{\""uri\"":\""/bin/tools/CodeScanner\""},\""workingDirectory\"":\""/home/buildAgent/src\"",\""environmentVariables\"":{\""PATH\"":\""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin\"",\""HOME\"":\""/home/buildAgent\"",\""TZ\"":\""EST\""}}],\""files\"":{\""collections.rsp\"":{\""fileLocation\"":{\""uri\"":\""collections.rsp\""},\""contents\"":{\""text\"":\""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999\""}}},\""results\"":[]}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithInvocationAndNotifications()
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
      ""invocations"": [
        {
          ""commandLine"": ""CodeScanner @collections.rsp"",
          ""toolNotifications"": [
            {
              ""id"": ""CTN0001"",
              ""message"": {
                ""text"": ""Run started.""
              },
              ""level"": ""note""
            },
            {
              ""id"": ""CTN9999"",
              ""ruleId"": ""C2152"",
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///home/buildAgent/src/crypto/hash.cpp""
                }
              },
              ""message"": {
                ""text"": ""Exception evaluating rule \""C2152\"". Rule disabled; run continues.""
              },
              ""level"": ""error"",
              ""threadId"": 52,
              ""time"": ""2016-07-16T14:18:43.119Z"",
              ""exception"": {
                ""kind"": ""ExecutionEngine.RuleFailureException"",
                ""message"": ""Unhandled exception during rule evaluation."",
                ""innerExceptions"": [
                  {
                    ""kind"": ""System.ArgumentException"",
                    ""message"": ""length is < 0""
                  }
                ]
              }
            },
            {
              ""id"": ""CTN0002"",
              ""message"": {
                ""text"": ""Run ended.""
              },
              ""level"": ""note""
            }
          ],
          ""configurationNotifications"": [
            {
              ""id"": ""UnknownRule"",
              ""ruleId"": ""ABC0001"",
              ""message"": {
                ""text"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
              }
            }
          ]
        }
      ],
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
      ""invocation"": {
        ""commandLine"": ""CodeScanner @collections.rsp""
      },
      ""results"": [],
      ""toolNotifications"": [
        {
          ""id"": ""CTN0001"",
          ""message"": ""Run started."",
          ""level"": ""note""
        },
        {
          ""id"": ""CTN9999"",
          ""ruleId"": ""C2152"",
          ""physicalLocation"": {
            ""uri"": ""file:///home/buildAgent/src/crypto/hash.cpp""
          },
          ""message"": ""Exception evaluating rule \""C2152\"". Rule disabled; run continues."",
          ""level"": ""error"",
          ""threadId"": 52,
          ""time"": ""2016-07-16T14:18:43.119Z"",
          ""exception"": {
            ""kind"": ""ExecutionEngine.RuleFailureException"",
            ""message"": ""Unhandled exception during rule evaluation."",
            ""innerExceptions"": [
              {
                ""kind"": ""System.ArgumentException"",
                ""message"": ""length is < 0""
              }
            ]
          }
        },
        {
          ""id"": ""CTN0002"",
          ""message"": ""Run ended."",
          ""level"": ""note""
        }
      ],
      ""configurationNotifications"": [
        {
          ""id"": ""UnknownRule"",
          ""ruleId"": ""ABC0001"",
          ""message"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
        }
      ],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""invocations\"":[{\""commandLine\"":\""CodeScanner @collections.rsp\"",\""toolNotifications\"":[{\""id\"":\""CTN0001\"",\""message\"":{\""text\"":\""Run started.\""},\""level\"":\""note\""},{\""id\"":\""CTN9999\"",\""ruleId\"":\""C2152\"",\""physicalLocation\"":{\""fileLocation\"":{\""uri\"":\""file:///home/buildAgent/src/crypto/hash.cpp\""}},\""message\"":{\""text\"":\""Exception evaluating rule \\\""C2152\\\"". Rule disabled; run continues.\""},\""level\"":\""error\"",\""threadId\"":52,\""time\"":\""2016-07-16T14:18:43.119Z\"",\""exception\"":{\""kind\"":\""ExecutionEngine.RuleFailureException\"",\""message\"":\""Unhandled exception during rule evaluation.\"",\""innerExceptions\"":[{\""kind\"":\""System.ArgumentException\"",\""message\"":\""length is < 0\""}]}},{\""id\"":\""CTN0002\"",\""message\"":{\""text\"":\""Run ended.\""},\""level\"":\""note\""}],\""configurationNotifications\"":[{\""id\"":\""UnknownRule\"",\""ruleId\"":\""ABC0001\"",\""message\"":{\""text\"":\""Could not disable rule \\\""ABC0001\\\"" because there is no rule with that id.\""}}]}],\""results\"":[]}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithNotificationsButNoInvocations()
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
      ""invocations"": [
        {
          ""toolNotifications"": [
            {
              ""id"": ""CTN0001"",
              ""message"": {
                ""text"": ""Run started.""
              },
              ""level"": ""note""
            }
          ],
          ""configurationNotifications"": [
            {
              ""id"": ""UnknownRule"",
              ""ruleId"": ""ABC0001"",
              ""message"": {
                ""text"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
              }
            }
          ]
        }
      ],
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
      ""invocation"": {},
      ""results"": [],
      ""toolNotifications"": [
        {
          ""id"": ""CTN0001"",
          ""message"": ""Run started."",
          ""level"": ""note""
        }
      ],
      ""configurationNotifications"": [
        {
          ""id"": ""UnknownRule"",
          ""ruleId"": ""ABC0001"",
          ""message"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
        }
      ],
      ""properties"": {
        ""sarifv2/run"": ""{\""tool\"":{\""name\"":\""CodeScanner\"",\""semanticVersion\"":\""2.1.0\""},\""invocations\"":[{\""toolNotifications\"":[{\""id\"":\""CTN0001\"",\""message\"":{\""text\"":\""Run started.\""},\""level\"":\""note\""}],\""configurationNotifications\"":[{\""id\"":\""UnknownRule\"",\""ruleId\"":\""ABC0001\"",\""message\"":{\""text\"":\""Could not disable rule \\\""ABC0001\\\"" because there is no rule with that id.\""}}]}],\""results\"":[]}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }
    }
}