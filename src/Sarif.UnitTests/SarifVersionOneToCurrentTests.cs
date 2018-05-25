// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifVersionOneToCurrentTests
    {
        private static SarifLogVersionOne GetSarifLogVersionOne(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLogVersionOne>(logText, SarifTransformerUtilities.JsonSettingsV1);
        }

        private static SarifLog TransformVersionOneToCurrent(string v1LogText)
        {
            SarifLogVersionOne v1Log = GetSarifLogVersionOne(v1LogText);
            var transformer = new SarifVersionOneToCurrentVisitor();
            transformer.VisitSarifLogVersionOne(v1Log);

            return transformer.SarifLog;
        }

        private static void VerifyVersionOneToCurrentTransformation(string v1LogText, string v2LogExpectedText)
        {
            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);
            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsV2);
            v2LogText.Should().Be(v2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
        {
            const string V1LogText =
@"{
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

            const string V2LogExpectedText =
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
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithTwoRuns()
        {
            string V1LogText =
@"{
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

            const string V2LogExpectedText =
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
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""AssetScanner\"",\r\n    \""semanticVersion\"": \""1.7.2\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithPropertyAndTags()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0"",
          ""properties"": {
            ""foo"": ""bar"",
            ""tags"": [ ""1"", ""2"" ]
          }
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0"",
        ""properties"": {
          ""foo"": ""bar"",
          ""tags"": [
  ""1"",
  ""2""
]
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\"",\r\n    \""properties\"": {\r\n      \""foo\"": \""bar\"",\r\n      \""tags\"": [\r\n  \""1\"",\r\n  \""2\""\r\n]\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithLogicalLocations()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
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
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
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
          ""parentKey"": ""collections::list"",
          ""kind"": ""function""
        },
        ""collections::list"": {
          ""name"": ""list"",
          ""parentKey"": ""collections"",
          ""kind"": ""type""
        },
        ""collections"": {
          ""kind"": ""namespace""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""logicalLocations\"": {\r\n    \""collections::list::add\"": {\r\n      \""name\"": \""add\"",\r\n      \""parentKey\"": \""collections::list\"",\r\n      \""kind\"": \""function\""\r\n    },\r\n    \""collections::list\"": {\r\n      \""name\"": \""list\"",\r\n      \""parentKey\"": \""collections\"",\r\n      \""kind\"": \""type\""\r\n    },\r\n    \""collections\"": {\r\n      \""name\"": \""collections\"",\r\n      \""kind\"": \""namespace\""\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithFiles()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""files"": {
          ""file:///home/list.txt"": {
            ""mimeType"": ""text/plain"",
            ""length"": 43,
            ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",
            ""hashes"": [
              {
                ""algorithm"": ""sha256"",
                ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592""
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
            ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"",
            ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
            ""offset"": 17522,
            ""length"": 4050
          }
        },
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
            ""text"": ""The quick brown fox jumps over the lazy dog""
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
          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""files\"": {\r\n    \""file:///home/list.txt\"": {\r\n      \""length\"": 43,\r\n      \""mimeType\"": \""text/plain\"",\r\n      \""contents\"": \""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==\"",\r\n      \""hashes\"": [\r\n        {\r\n          \""value\"": \""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592\"",\r\n          \""algorithm\"": \""sha256\""\r\n        }\r\n      ]\r\n    },\r\n    \""file:///home/buildAgent/bin/app.zip\"": {\r\n      \""mimeType\"": \""application/zip\"",\r\n      \""properties\"": {\r\n        \""my_key\"": \""some value\""\r\n      }\r\n    },\r\n    \""file:///home/buildAgent/bin/app.zip#/docs/intro.docx\"": {\r\n      \""uri\"": \""file:///docs/intro.docx\"",\r\n      \""parentKey\"": \""file:///home/buildAgent/bin/app.zip\"",\r\n      \""offset\"": 17522,\r\n      \""length\"": 4050,\r\n      \""mimeType\"": \""application/vnd.openxmlformats-officedocument.wordprocessingml.document\""\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWitRules()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
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
            ""fullDescription"": ""Rent internal rebellion competence biography photograph."",
            ""configuration"": ""disabled"",
            ""defaultLevel"": ""pass""
          }
        },
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
            },
            ""configuration"": {
              ""defaultLevel"": ""note""
            }
          }
        }
      },
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": [],\r\n  \""rules\"": {\r\n    \""C2001\"": {\r\n      \""id\"": \""C2001\"",\r\n      \""shortDescription\"": \""A variable was used without being initialized.\"",\r\n      \""messageFormats\"": {\r\n        \""default\"": \""Variable \\\""{0}\\\"" was used without being initialized.\""\r\n      },\r\n      \""properties\"": {\r\n        \""some_key\"": \""FoxForceFive\""\r\n      }\r\n    },\r\n    \""C2002\"": {\r\n      \""id\"": \""C2002\"",\r\n      \""fullDescription\"": \""Catfish season continuous hen lamb include dose copy grant.\"",\r\n      \""configuration\"": \""enabled\"",\r\n      \""defaultLevel\"": \""error\"",\r\n      \""helpUri\"": \""http://www.domain.com/rules/c2002.html\""\r\n    },\r\n    \""C2003\"": {\r\n      \""id\"": \""C2003\"",\r\n      \""name\"": \""Rule C2003\"",\r\n      \""shortDescription\"": \""Rules were meant to be broken.\"",\r\n      \""fullDescription\"": \""Rent internal rebellion competence biography photograph.\"",\r\n      \""configuration\"": \""disabled\"",\r\n      \""defaultLevel\"": \""pass\""\r\n    }\r\n  }\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithBasicInvocation()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""invocation"": {
          ""commandLine"": ""CodeScanner @collections.rsp"",
          ""responseFiles"": {
            ""collections.rsp"": ""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999""
          },
          ""startTime"": ""2016-07-16T14:18:25Z"",
          ""endTime"": ""2016-07-16T14:19:01Z"",
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
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""invocation\"": {\r\n    \""commandLine\"": \""CodeScanner @collections.rsp\"",\r\n    \""responseFiles\"": {\r\n      \""collections.rsp\"": \""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999\""\r\n    },\r\n    \""startTime\"": \""2016-07-16T14:18:25.000Z\"",\r\n    \""endTime\"": \""2016-07-16T14:19:01.000Z\"",\r\n    \""machine\"": \""BLD01\"",\r\n    \""account\"": \""buildAgent\"",\r\n    \""processId\"": 1218,\r\n    \""fileName\"": \""/bin/tools/CodeScanner\"",\r\n    \""workingDirectory\"": \""/home/buildAgent/src\"",\r\n    \""environmentVariables\"": {\r\n      \""PATH\"": \""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin\"",\r\n      \""HOME\"": \""/home/buildAgent\"",\r\n      \""TZ\"": \""EST\""\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithInvocationAndNotifications()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""invocation"": {
          ""commandLine"": ""CodeScanner @collections.rsp""
        },
        ""configurationNotifications"": [
          {
            ""id"": ""UnknownRule"",
            ""ruleId"": ""ABC0001"",
            ""level"": ""warning"",
            ""message"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
          }
        ],
        ""toolNotifications"": [
          {
            ""id"": ""CTN0001"",
            ""level"": ""note"",
            ""message"": ""Run started.""
          },
          {
            ""id"": ""CTN9999"",
            ""ruleId"": ""C2152"",
            ""level"": ""error"",
            ""message"": ""Exception evaluating rule \""C2152\"". Rule disabled; run continues."",
            ""physicalLocation"": {
              ""uri"": ""file:///home/buildAgent/src/crypto/hash.cpp""
            },
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
            ""level"": ""note"",
            ""message"": ""Run ended.""
          }
        ],
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""invocation\"": {\r\n    \""commandLine\"": \""CodeScanner @collections.rsp\""\r\n  },\r\n  \""results\"": [],\r\n  \""toolNotifications\"": [\r\n    {\r\n      \""id\"": \""CTN0001\"",\r\n      \""message\"": \""Run started.\"",\r\n      \""level\"": \""note\""\r\n    },\r\n    {\r\n      \""id\"": \""CTN9999\"",\r\n      \""ruleId\"": \""C2152\"",\r\n      \""physicalLocation\"": {\r\n        \""uri\"": \""file:///home/buildAgent/src/crypto/hash.cpp\""\r\n      },\r\n      \""message\"": \""Exception evaluating rule \\\""C2152\\\"". Rule disabled; run continues.\"",\r\n      \""level\"": \""error\"",\r\n      \""threadId\"": 52,\r\n      \""time\"": \""2016-07-16T14:18:43.119Z\"",\r\n      \""exception\"": {\r\n        \""kind\"": \""ExecutionEngine.RuleFailureException\"",\r\n        \""message\"": \""Unhandled exception during rule evaluation.\"",\r\n        \""innerExceptions\"": [\r\n          {\r\n            \""kind\"": \""System.ArgumentException\"",\r\n            \""message\"": \""length is < 0\""\r\n          }\r\n        ]\r\n      }\r\n    },\r\n    {\r\n      \""id\"": \""CTN0002\"",\r\n      \""message\"": \""Run ended.\"",\r\n      \""level\"": \""note\""\r\n    }\r\n  ],\r\n  \""configurationNotifications\"": [\r\n    {\r\n      \""id\"": \""UnknownRule\"",\r\n      \""ruleId\"": \""ABC0001\"",\r\n      \""message\"": \""Could not disable rule \\\""ABC0001\\\"" because there is no rule with that id.\""\r\n    }\r\n  ]\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithNotificationsButNoInvocations()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""configurationNotifications"": [
          {
            ""id"": ""UnknownRule"",
            ""ruleId"": ""ABC0001"",
            ""level"": ""warning"",
            ""message"": ""Could not disable rule \""ABC0001\"" because there is no rule with that id.""
          }
        ],
        ""toolNotifications"": [
          {
            ""id"": ""CTN0001"",
            ""level"": ""note"",
            ""message"": ""Run started.""
          }
        ],
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": [],\r\n  \""toolNotifications\"": [\r\n    {\r\n      \""id\"": \""CTN0001\"",\r\n      \""message\"": \""Run started.\"",\r\n      \""level\"": \""note\""\r\n    }\r\n  ],\r\n  \""configurationNotifications\"": [\r\n    {\r\n      \""id\"": \""UnknownRule\"",\r\n      \""ruleId\"": \""ABC0001\"",\r\n      \""message\"": \""Could not disable rule \\\""ABC0001\\\"" because there is no rule with that id.\""\r\n    }\r\n  ]\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_NotificationExceptionWithStack()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""toolNotifications"": [
          {
            ""id"": ""CTN0001"",
            ""level"": ""error"",
            ""message"": ""Unhandled exception."",
            ""exception"": {
              ""kind"": ""ExecutionEngine.RuleFailureException"",
              ""message"": ""Unhandled exception during rule evaluation."",
              ""stack"": {
                ""message"": ""This is the stack messasge."",
                ""frames"": [
                  {
                    ""message"": ""Exception thrown"",
                    ""module"": ""RuleLibrary"",
                    ""threadId"": 52,
                    ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Evaluate"",
                    ""uri"": ""file:///C:/src/main.cs"",
                    ""address"": 10092852,
                    ""line"": 15,
                    ""column"": 9
                  },
                  {
                    ""module"": ""RuleLibrary"",
                    ""threadId"": 52,
                    ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Register"",
                    ""uri"": ""file:///C:/src/main.cs"",
                    ""address"": 1002485
                  },
                  {
                    ""module"": ""ExecutionEngine"",
                    ""threadId"": 52,
                    ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule"",
                    ""uri"": ""file:///C:/src/utils.cs"",
                    ""address"": 10073356,
                    ""offset"": 10475
                  },
                  {
                    ""module"": ""ExecutionEngine"",
                    ""threadId"": 52,
                    ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule"",
                    ""logicalLocationKey"": ""ExecutionEngine.Engine.FooBar"",
                    ""uri"": ""file:///C:/src/foobar.cs"",
                    ""address"": 10073356,
                    ""offset"": 10475
                  }
                ]
              },
              ""innerExceptions"": [
                {
                  ""kind"": ""System.ArgumentException"",
                  ""message"": ""length is < 0""
                }
              ]
            }
          }
        ],
        ""logicalLocations"": {
          ""Rules.SecureHashAlgorithmRule.Evaluate"": {
            ""name"": ""Evaluate"",
            ""kind"": ""some kind""
          },
          ""Rules.SecureHashAlgorithmRule.Register"": {
            ""name"": ""InvalidName""
          },
          ""ExecutionEngine.Engine.FooBar"": {
            ""name"": ""FooBar"",
            ""kind"": ""another kind""
          }
        },
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V2LogExpectedText =
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
                ""text"": ""Unhandled exception.""
              },
              ""level"": ""error"",
              ""exception"": {
                ""kind"": ""ExecutionEngine.RuleFailureException"",
                ""message"": ""Unhandled exception during rule evaluation."",
                ""stack"": {
                  ""message"": {
                    ""text"": ""This is the stack messasge.""
                  },
                  ""frames"": [
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/main.cs""
                          },
                          ""region"": {
                            ""startLine"": 15,
                            ""startColumn"": 9
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Evaluate"",
                        ""message"": {
                          ""text"": ""Exception thrown""
                        }
                      },
                      ""module"": ""RuleLibrary"",
                      ""threadId"": 52,
                      ""address"": 10092852
                    },
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/main.cs""
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Register-0""
                      },
                      ""module"": ""RuleLibrary"",
                      ""threadId"": 52,
                      ""address"": 1002485
                    },
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/utils.cs""
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule""
                      },
                      ""module"": ""ExecutionEngine"",
                      ""threadId"": 52,
                      ""address"": 10073356,
                      ""offset"": 10475
                    },
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/foobar.cs""
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.FooBar""
                      },
                      ""module"": ""ExecutionEngine"",
                      ""threadId"": 52,
                      ""address"": 10073356,
                      ""offset"": 10475
                    }
                  ]
                },
                ""innerExceptions"": [
                  {
                    ""kind"": ""System.ArgumentException"",
                    ""message"": ""length is < 0""
                  }
                ]
              }
            }
          ]
        }
      ],
      ""logicalLocations"": {
        ""Rules.SecureHashAlgorithmRule.Evaluate"": {
          ""name"": ""Evaluate"",
          ""kind"": ""some kind""
        },
        ""Rules.SecureHashAlgorithmRule.Register"": {
          ""name"": ""InvalidName""
        },
        ""ExecutionEngine.Engine.FooBar"": {
          ""name"": ""EvaluateRule"",
          ""fullyQualifiedName"": ""ExecutionEngine.Engine.EvaluateRule"",
          ""kind"": ""another kind""
        },
        ""Rules.SecureHashAlgorithmRule.Register-0"": {
          ""name"": ""Register"",
          ""fullyQualifiedName"": ""Rules.SecureHashAlgorithmRule.Register""
        },
        ""ExecutionEngine.Engine.EvaluateRule"": {
          ""name"": ""EvaluateRule""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""logicalLocations\"": {\r\n    \""Rules.SecureHashAlgorithmRule.Evaluate\"": {\r\n      \""name\"": \""Evaluate\"",\r\n      \""kind\"": \""some kind\""\r\n    },\r\n    \""Rules.SecureHashAlgorithmRule.Register\"": {\r\n      \""name\"": \""InvalidName\""\r\n    },\r\n    \""ExecutionEngine.Engine.FooBar\"": {\r\n      \""name\"": \""FooBar\"",\r\n      \""kind\"": \""another kind\""\r\n    }\r\n  },\r\n  \""results\"": [],\r\n  \""toolNotifications\"": [\r\n    {\r\n      \""id\"": \""CTN0001\"",\r\n      \""message\"": \""Unhandled exception.\"",\r\n      \""level\"": \""error\"",\r\n      \""exception\"": {\r\n        \""kind\"": \""ExecutionEngine.RuleFailureException\"",\r\n        \""message\"": \""Unhandled exception during rule evaluation.\"",\r\n        \""stack\"": {\r\n          \""message\"": \""This is the stack messasge.\"",\r\n          \""frames\"": [\r\n            {\r\n              \""message\"": \""Exception thrown\"",\r\n              \""uri\"": \""file:///C:/src/main.cs\"",\r\n              \""line\"": 15,\r\n              \""column\"": 9,\r\n              \""module\"": \""RuleLibrary\"",\r\n              \""threadId\"": 52,\r\n              \""fullyQualifiedLogicalName\"": \""Rules.SecureHashAlgorithmRule.Evaluate\"",\r\n              \""address\"": 10092852\r\n            },\r\n            {\r\n              \""uri\"": \""file:///C:/src/main.cs\"",\r\n              \""module\"": \""RuleLibrary\"",\r\n              \""threadId\"": 52,\r\n              \""fullyQualifiedLogicalName\"": \""Rules.SecureHashAlgorithmRule.Register\"",\r\n              \""address\"": 1002485\r\n            },\r\n            {\r\n              \""uri\"": \""file:///C:/src/utils.cs\"",\r\n              \""module\"": \""ExecutionEngine\"",\r\n              \""threadId\"": 52,\r\n              \""fullyQualifiedLogicalName\"": \""ExecutionEngine.Engine.EvaluateRule\"",\r\n              \""address\"": 10073356,\r\n              \""offset\"": 10475\r\n            },\r\n            {\r\n              \""uri\"": \""file:///C:/src/foobar.cs\"",\r\n              \""module\"": \""ExecutionEngine\"",\r\n              \""threadId\"": 52,\r\n              \""fullyQualifiedLogicalName\"": \""ExecutionEngine.Engine.EvaluateRule\"",\r\n              \""logicalLocationKey\"": \""ExecutionEngine.Engine.FooBar\"",\r\n              \""address\"": 10073356,\r\n              \""offset\"": 10475\r\n            }\r\n          ]\r\n        },\r\n        \""innerExceptions\"": [\r\n          {\r\n            \""kind\"": \""System.ArgumentException\"",\r\n            \""message\"": \""length is < 0\""\r\n          }\r\n        ]\r\n      }\r\n    }\r\n  ]\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_BasicResult()
        {
            string V1LogText =
@"{
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
          {
            ""ruleId"": ""C2001"",
            ""formattedRuleMessage"": {
              ""formatId"": ""default"",
              ""arguments"": [
                ""ptr""
              ]
            },
            ""suppressionStates"": [ ""suppressedExternally"" ],
            ""baselineState"": ""existing"",
            ""level"": ""error"",
            ""snippet"": ""add_core(ptr, offset, val);"",
            ""locations"": [
              {
                ""analysisTarget"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.cpp""
                },
                ""resultFile"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                  ""region"": {
                    ""startLine"": 1,
                    ""startColumn"": 1,
                    ""endLine"": 1,
                    ""endColumn"": 28,
                    ""length"": 27
                  }
                },
                ""fullyQualifiedLogicalName"": ""collections::list::add"",
                ""decoratedName"": ""?add@list@collections@@QAEXH@Z""
              }
            ],
            ""relatedLocations"": [
              {
                ""message"": ""\""count\"" was declared here."",
                ""physicalLocation"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                  ""region"": {
                    ""startLine"": 8,
                    ""startColumn"": 5
                  }
                },
                ""fullyQualifiedLogicalName"": ""collections::list::add""
              }
            ]
    }
        ],
        ""rules"": {
          ""C2001"": {
            ""id"": ""C2001"",
            ""shortDescription"": ""A variable was used without being initialized."",
            ""fullDescription"": ""A variable was used without being initialized. This can result in runtime errors such as null reference exceptions."",
            ""messageFormats"": {
              ""default"": ""Variable \""{0}\"" was used without being initialized.""
            }
          }
        }
      }
    ]
  }";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
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
          ""decoratedName"": ""?add@list@collections@@QAEXH@Z"",
          ""parentKey"": ""collections::list"",
          ""kind"": ""function""
        },
        ""collections::list"": {
          ""name"": ""list"",
          ""parentKey"": ""collections"",
          ""kind"": ""type""
        },
        ""collections"": {
          ""kind"": ""namespace""
        }
      },
      ""results"": [
        {
          ""ruleId"": ""C2001"",
          ""level"": ""error"",
          ""message"": {
            ""arguments"": [
              ""ptr""
            ]
          },
          ""ruleMessageId"": ""default"",
          ""analysisTarget"": {
            ""uri"": ""file:///home/buildAgent/src/collections/list.cpp""
          },
          ""locations"": [
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                },
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 1,
                  ""endLine"": 1,
                  ""endColumn"": 28,
                  ""length"": 27,
                  ""snippet"": {
                    ""text"": ""add_core(ptr, offset, val);""
                  }
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add""
            }
          ],
          ""relatedLocations"": [
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                },
                ""region"": {
                  ""startLine"": 8,
                  ""startColumn"": 5
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add"",
              ""message"": {
                ""text"": ""\""count\"" was declared here.""
              }
            }
          ],
          ""suppressionStates"": [""suppressedExternally""],
          ""baselineState"": ""existing""
        }
      ],
      ""resources"": {
        ""rules"": {
          ""C2001"": {
            ""id"": ""C2001"",
            ""shortDescription"": {
              ""text"": ""A variable was used without being initialized.""
            },
            ""fullDescription"": {
              ""text"": ""A variable was used without being initialized. This can result in runtime errors such as null reference exceptions.""
            },
            ""messageStrings"": {
              ""default"": ""Variable \""{0}\"" was used without being initialized.""
            }
          }
        }
      },
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""logicalLocations\"": {\r\n    \""collections::list::add\"": {\r\n      \""name\"": \""add\"",\r\n      \""parentKey\"": \""collections::list\"",\r\n      \""kind\"": \""function\""\r\n    },\r\n    \""collections::list\"": {\r\n      \""name\"": \""list\"",\r\n      \""parentKey\"": \""collections\"",\r\n      \""kind\"": \""type\""\r\n    },\r\n    \""collections\"": {\r\n      \""name\"": \""collections\"",\r\n      \""kind\"": \""namespace\""\r\n    }\r\n  },\r\n  \""results\"": [\r\n    {\r\n      \""ruleId\"": \""C2001\"",\r\n      \""level\"": \""error\"",\r\n      \""formattedRuleMessage\"": {\r\n        \""formatId\"": \""default\"",\r\n        \""arguments\"": [\r\n          \""ptr\""\r\n        ]\r\n      },\r\n      \""locations\"": [\r\n        {\r\n          \""analysisTarget\"": {\r\n            \""uri\"": \""file:///home/buildAgent/src/collections/list.cpp\""\r\n          },\r\n          \""resultFile\"": {\r\n            \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n            \""region\"": {\r\n              \""startLine\"": 1,\r\n              \""startColumn\"": 1,\r\n              \""endLine\"": 1,\r\n              \""endColumn\"": 28,\r\n              \""length\"": 27\r\n            }\r\n          },\r\n          \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n          \""decoratedName\"": \""?add@list@collections@@QAEXH@Z\""\r\n        }\r\n      ],\r\n      \""snippet\"": \""add_core(ptr, offset, val);\"",\r\n      \""relatedLocations\"": [\r\n        {\r\n          \""physicalLocation\"": {\r\n            \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n            \""region\"": {\r\n              \""startLine\"": 8,\r\n              \""startColumn\"": 5\r\n            }\r\n          },\r\n          \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n          \""message\"": \""\\\""count\\\"" was declared here.\""\r\n        }\r\n      ],\r\n      \""suppressionStates\"": 2,\r\n      \""baselineState\"": \""existing\""\r\n    }\r\n  ],\r\n  \""rules\"": {\r\n    \""C2001\"": {\r\n      \""id\"": \""C2001\"",\r\n      \""shortDescription\"": \""A variable was used without being initialized.\"",\r\n      \""fullDescription\"": \""A variable was used without being initialized. This can result in runtime errors such as null reference exceptions.\"",\r\n      \""messageFormats\"": {\r\n        \""default\"": \""Variable \\\""{0}\\\"" was used without being initialized.\""\r\n      }\r\n    }\r\n  }\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_TwoResultsWithFixes()
        {
            string V1LogText =
@"{
    ""version"": ""1.0.0"",
    ""runs"": [
      {
        ""tool"": {
          ""name"": ""CodeScanner""
        },
        ""results"": [
          {
            ""ruleId"": ""WEB1079"",
            ""formattedRuleMessage"": {
              ""formatId"": ""default"",
              ""arguments"": [
                ""shape""
              ]
            },
            ""snippet"": ""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />"",
            ""locations"": [
              {
                ""analysisTarget"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""resultFile"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                  ""region"": {
                    ""startLine"": 20,
                    ""startColumn"": 69,
                    ""endColumn"": 74,
                    ""offset"": 720,
                    ""length"": 5
                  }
                }
              }
            ],
            ""fixes"": [
              {
                ""description"": ""Wrap attribute values in single quotes."",
                ""fileChanges"": [
                  {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                    ""replacements"": [
                      {
                        ""offset"": 720,
                        ""insertedBytes"": ""Jw==""
                      },
                      {
                        ""offset"": 725,
                        ""insertedBytes"": ""Jw==""
                      }
                    ]
                  }
                ]
              },
              {
                ""description"": ""Wrap attribute value in double quotes."",
                ""fileChanges"": [
                  {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                    ""replacements"": [
                      {
                        ""offset"": 720,
                        ""insertedBytes"": ""Ig==""
                      },
                      {
                        ""offset"": 725,
                        ""insertedBytes"": ""Ig==""
                      }
                    ]
                  }
                ]
              }
            ]
          },
          {
            ""ruleId"": ""WEB1066"",
            ""formattedRuleMessage"": {
              ""formatId"": ""default"",
              ""arguments"": [
                ""DIV""
              ]
            },
            ""snippet"": ""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>"",
            ""locations"": [
              {
                ""analysisTarget"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""resultFile"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                  ""region"": {
                    ""startLine"": 24,
                    ""startColumn"": 4,
                    ""endColumn"": 38,
                    ""offset"": 803,
                    ""length"": 34
                  }
                }
              }
            ],
            ""fixes"": [
              {
                ""description"": ""Convert tag name to lowercase."",
                ""fileChanges"": [
                  {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                    ""replacements"": [
                      {
                        ""offset"": 804,
                        ""deletedLength"": 3,
                        ""insertedBytes"": ""ZGl2""
                      }
                    ]
                  }
                ]
              }
            ]
          }
        ],
        ""rules"": {
          ""WEB1079.AttributeValueIsNotQuoted"": {
            ""id"": ""WEB1079"",
            ""shortDescription"": ""The attribute value is not quoted."",
            ""messageFormats"": {
              ""default"": ""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.""
            }
          },
          ""WEB1066.TagNameIsNotLowercase"": {
            ""id"": ""WEB1066"",
            ""shortDescription"": ""The tag name is not lowercase."",
            ""messageFormats"": {
              ""default"": ""Convert the name of the <{0}> tag to lowercase.""
            }
          }
        }
      }
    ]
  }";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner""
      },
      ""results"": [
        {
          ""ruleId"": ""WEB1079"",
          ""message"": {
            ""arguments"": [
              ""shape""
            ]
          },
          ""ruleMessageId"": ""default"",
          ""locations"": [
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""region"": {
                  ""startLine"": 20,
                  ""startColumn"": 69,
                  ""endColumn"": 74,
                  ""offset"": 720,
                  ""length"": 5,
                  ""snippet"": {
                    ""text"": ""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />""
                  }
                }
              }
            }
          ],
          ""fixes"": [
            {
              ""description"": {
                ""text"": ""Wrap attribute values in single quotes.""
              },
              ""fileChanges"": [
                {
                  ""fileLocation"": {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""offset"": 720
                      },
                      ""insertedContent"": {
                        ""binary"": ""Jw==""
                      }
                    },
                    {
                      ""deletedRegion"": {
                        ""offset"": 725
                      },
                      ""insertedContent"": {
                        ""binary"": ""Jw==""
                      }
                    }
                  ]
                }
              ]
            },
            {
              ""description"": {
                ""text"": ""Wrap attribute value in double quotes.""
              },
              ""fileChanges"": [
                {
                  ""fileLocation"": {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""offset"": 720
                      },
                      ""insertedContent"": {
                        ""binary"": ""Ig==""
                      }
                    },
                    {
                      ""deletedRegion"": {
                        ""offset"": 725
                      },
                      ""insertedContent"": {
                        ""binary"": ""Ig==""
                      }
                    }
                  ]
                }
              ]
            }
          ]
        },
        {
          ""ruleId"": ""WEB1066"",
          ""message"": {
            ""arguments"": [
              ""DIV""
            ]
          },
          ""ruleMessageId"": ""default"",
          ""locations"": [
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38,
                  ""offset"": 803,
                  ""length"": 34,
                  ""snippet"": {
                    ""text"": ""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>""
                  }
                }
              }
            }
          ],
          ""fixes"": [
            {
              ""description"": {
                ""text"": ""Convert tag name to lowercase.""
              },
              ""fileChanges"": [
                {
                  ""fileLocation"": {
                    ""uri"": ""http://localhost:34420/HtmlFixes.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""offset"": 804,
                        ""length"": 3
                      },
                      ""insertedContent"": {
                        ""binary"": ""ZGl2""
                      }
                    }
                  ]
                }
              ]
            }
          ]
        }
      ],
      ""resources"": {
        ""rules"": {
          ""WEB1079.AttributeValueIsNotQuoted"": {
            ""id"": ""WEB1079"",
            ""shortDescription"": {
              ""text"": ""The attribute value is not quoted.""
            },
            ""messageStrings"": {
              ""default"": ""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.""
            }
          },
          ""WEB1066.TagNameIsNotLowercase"": {
            ""id"": ""WEB1066"",
            ""shortDescription"": {
              ""text"": ""The tag name is not lowercase.""
            },
            ""messageStrings"": {
              ""default"": ""Convert the name of the <{0}> tag to lowercase.""
            }
          }
        }
      },
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\""\r\n  },\r\n  \""results\"": [\r\n    {\r\n      \""ruleId\"": \""WEB1079\"",\r\n      \""formattedRuleMessage\"": {\r\n        \""formatId\"": \""default\"",\r\n        \""arguments\"": [\r\n          \""shape\""\r\n        ]\r\n      },\r\n      \""locations\"": [\r\n        {\r\n          \""analysisTarget\"": {\r\n            \""uri\"": \""http://localhost:34420/HtmlFixes.html\""\r\n          },\r\n          \""resultFile\"": {\r\n            \""uri\"": \""http://localhost:34420/HtmlFixes.html\"",\r\n            \""region\"": {\r\n              \""startLine\"": 20,\r\n              \""startColumn\"": 69,\r\n              \""endColumn\"": 74,\r\n              \""offset\"": 720,\r\n              \""length\"": 5\r\n            }\r\n          }\r\n        }\r\n      ],\r\n      \""snippet\"": \""<area alt=\\\""Here is some text\\\"" coords=\\\""10 20 20\\\"" href=\\\""moon.html\\\"" shape=circle xweb:fixindex=\\\""0\\\"" />\"",\r\n      \""fixes\"": [\r\n        {\r\n          \""description\"": \""Wrap attribute values in single quotes.\"",\r\n          \""fileChanges\"": [\r\n            {\r\n              \""uri\"": \""http://localhost:34420/HtmlFixes.html\"",\r\n              \""replacements\"": [\r\n                {\r\n                  \""offset\"": 720,\r\n                  \""insertedBytes\"": \""Jw==\""\r\n                },\r\n                {\r\n                  \""offset\"": 725,\r\n                  \""insertedBytes\"": \""Jw==\""\r\n                }\r\n              ]\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \""description\"": \""Wrap attribute value in double quotes.\"",\r\n          \""fileChanges\"": [\r\n            {\r\n              \""uri\"": \""http://localhost:34420/HtmlFixes.html\"",\r\n              \""replacements\"": [\r\n                {\r\n                  \""offset\"": 720,\r\n                  \""insertedBytes\"": \""Ig==\""\r\n                },\r\n                {\r\n                  \""offset\"": 725,\r\n                  \""insertedBytes\"": \""Ig==\""\r\n                }\r\n              ]\r\n            }\r\n          ]\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \""ruleId\"": \""WEB1066\"",\r\n      \""formattedRuleMessage\"": {\r\n        \""formatId\"": \""default\"",\r\n        \""arguments\"": [\r\n          \""DIV\""\r\n        ]\r\n      },\r\n      \""locations\"": [\r\n        {\r\n          \""analysisTarget\"": {\r\n            \""uri\"": \""http://localhost:34420/HtmlFixes.html\""\r\n          },\r\n          \""resultFile\"": {\r\n            \""uri\"": \""http://localhost:34420/HtmlFixes.html\"",\r\n            \""region\"": {\r\n              \""startLine\"": 24,\r\n              \""startColumn\"": 4,\r\n              \""endColumn\"": 38,\r\n              \""offset\"": 803,\r\n              \""length\"": 34\r\n            }\r\n          }\r\n        }\r\n      ],\r\n      \""snippet\"": \""<DIV id=\\\""test1\\\"" xweb:fixindex=\\\""0\\\""></DIV>\"",\r\n      \""fixes\"": [\r\n        {\r\n          \""description\"": \""Convert tag name to lowercase.\"",\r\n          \""fileChanges\"": [\r\n            {\r\n              \""uri\"": \""http://localhost:34420/HtmlFixes.html\"",\r\n              \""replacements\"": [\r\n                {\r\n                  \""offset\"": 804,\r\n                  \""deletedLength\"": 3,\r\n                  \""insertedBytes\"": \""ZGl2\""\r\n                }\r\n              ]\r\n            }\r\n          ]\r\n        }\r\n      ]\r\n    }\r\n  ],\r\n  \""rules\"": {\r\n    \""WEB1079.AttributeValueIsNotQuoted\"": {\r\n      \""id\"": \""WEB1079\"",\r\n      \""shortDescription\"": \""The attribute value is not quoted.\"",\r\n      \""messageFormats\"": {\r\n        \""default\"": \""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.\""\r\n      }\r\n    },\r\n    \""WEB1066.TagNameIsNotLowercase\"": {\r\n      \""id\"": \""WEB1066\"",\r\n      \""shortDescription\"": \""The tag name is not lowercase.\"",\r\n      \""messageFormats\"": {\r\n        \""default\"": \""Convert the name of the <{0}> tag to lowercase.\""\r\n      }\r\n    }\r\n  }\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_CodeFlows()
        {
            string V1LogText =
@"{
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [
        {
          ""ruleId"": ""C2001"",
          ""message"": ""Variable \""ptr\"" declared."",
          ""snippet"": ""add_core(ptr, offset, val);"",
          ""locations"": [
            {
              ""analysisTarget"": {
                ""uri"": ""file:///home/buildAgent/src/collections/list.cpp""
              },
              ""resultFile"": {
                ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 1,
                  ""endLine"": 1,
                  ""endColumn"": 28,
                  ""length"": 27
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add"",
              ""decoratedName"": ""?add@list@collections@@QAEXH@Z""
            }
          ],
          ""codeFlows"": [
            {
              ""message"": ""Path from declaration to usage"",
              ""locations"": [
                {
                  ""kind"": ""declaration"",
                  ""importance"": ""essential"",
                  ""message"": ""Variable \""ptr\"" declared."",
                  ""snippet"": ""int *ptr;"",
                  ""physicalLocation"": {
                    ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                    ""region"": {
                      ""startLine"": 15
                    }
                  },
                  ""fullyQualifiedLogicalName"": ""collections::list::add"",
                  ""module"": ""platform"",
                  ""threadId"": 52,
                  ""taintKind"": ""sink"",
                  ""target"": ""foo::bar"",
                  ""targetKey"": ""collections::list::add"",
                  ""values"": [
                    ""id"",
                    ""name"",
                    ""param3""
                  ]
                },
                {
                  ""annotations"": [
                    {
                      ""message"": ""This is a test annotation"",
                      ""locations"": [
                        {
                          ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                          ""region"": {
                            ""startLine"": 40
                          }
                        },
                        {
                          ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                          ""region"": {
                            ""startLine"": 240
                          }
                        }
                      ]
                    },
                    {
                      ""message"": ""This is a second test annotation"",
                      ""locations"": [
                        {
                          ""uri"": ""file:///home/buildAgent/src/collections/foo.cpp"",
                          ""region"": {
                            ""startLine"": 128
                          }
                        }
                      ]
                    }
                  ],
                  ""step"": 1,
                  ""kind"": ""assignment"",
                  ""importance"": ""unimportant"",
                  ""snippet"": ""offset = 0;"",
                  ""physicalLocation"": {
                    ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                    ""region"": {
                      ""startLine"": 15
                    }
                  },
                  ""fullyQualifiedLogicalName"": ""collections::list::add"",
                  ""module"": ""platform"",
                  ""threadId"": 52
                },
                {
                  ""step"": 2,
                  ""kind"": ""callReturn"",
                  ""importance"": ""essential"",
                  ""message"": ""Uninitialized variable \""ptr\"" passed to method \""add_core\""."",
                  ""snippet"": ""add_core(ptr, offset, val)"",
                  ""state"": {
                    ""Foo"": ""bar""
                  },
                  ""target"": ""collections::list::add_core"",
                  ""physicalLocation"": {
                    ""uri"": ""file:///home/buildAgent/src/collections/list.h"",
                    ""region"": {
                      ""startLine"": 25
                    }
                  },
                  ""fullyQualifiedLogicalName"": ""collections::list::add"",
                  ""module"": ""platform"",
                  ""threadId"": 52
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
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
          ""decoratedName"": ""?add@list@collections@@QAEXH@Z""
        }
      },
      ""results"": [
        {
          ""ruleId"": ""C2001"",
          ""message"": {
            ""text"": ""Variable \""ptr\"" declared.""
          },
          ""analysisTarget"": {
            ""uri"": ""file:///home/buildAgent/src/collections/list.cpp""
          },
          ""locations"": [
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                },
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 1,
                  ""endLine"": 1,
                  ""endColumn"": 28,
                  ""length"": 27,
                  ""snippet"": {
                    ""text"": ""add_core(ptr, offset, val);""
                  }
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add""
            }
          ],
          ""codeFlows"": [
            {
              ""message"": {
                ""text"": ""Path from declaration to usage""
              },
              ""threadFlows"": [
                {
                  ""locations"": [
                    {
                      ""step"": 1,
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                          },
                          ""region"": {
                            ""startLine"": 15,
                            ""snippet"": {
                              ""text"": ""int *ptr;""
                            }
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""collections::list::add"",
                        ""message"": {
                          ""text"": ""Variable \""ptr\"" declared.""
                        }
                      },
                      ""module"": ""platform"",
                      ""importance"": ""essential""
                    },
                    {
                      ""step"": 2,
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                          },
                          ""region"": {
                            ""startLine"": 15,
                            ""snippet"": {
                              ""text"": ""offset = 0;""
                            }
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""collections::list::add"",
                        ""annotations"": [
                          {
                            ""startLine"": 40,
                            ""message"": {
                              ""text"": ""This is a test annotation""
                            }
                          },
                          {
                            ""startLine"": 240,
                            ""message"": {
                              ""text"": ""This is a test annotation""
                            }
                          }
                        ]
                      },
                      ""module"": ""platform""
                    },
                    {
                      ""step"": 3,
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///home/buildAgent/src/collections/list.h""
                          },
                          ""region"": {
                            ""startLine"": 25,
                            ""snippet"": {
                              ""text"": ""add_core(ptr, offset, val)""
                            }
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""collections::list::add"",
                        ""message"": {
                          ""text"": ""Uninitialized variable \""ptr\"" passed to method \""add_core\"".""
                        }
                      },
                      ""module"": ""platform"",
                      ""state"": {
                        ""Foo"": ""bar""
                      },
                      ""importance"": ""essential""
                    }
                  ]
                }
              ]
            }
          ]
        }
      ],
      ""properties"": {
        ""sarifv1/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": [\r\n    {\r\n      \""ruleId\"": \""C2001\"",\r\n      \""message\"": \""Variable \\\""ptr\\\"" declared.\"",\r\n      \""locations\"": [\r\n        {\r\n          \""analysisTarget\"": {\r\n            \""uri\"": \""file:///home/buildAgent/src/collections/list.cpp\""\r\n          },\r\n          \""resultFile\"": {\r\n            \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n            \""region\"": {\r\n              \""startLine\"": 1,\r\n              \""startColumn\"": 1,\r\n              \""endLine\"": 1,\r\n              \""endColumn\"": 28,\r\n              \""length\"": 27\r\n            }\r\n          },\r\n          \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n          \""decoratedName\"": \""?add@list@collections@@QAEXH@Z\""\r\n        }\r\n      ],\r\n      \""snippet\"": \""add_core(ptr, offset, val);\"",\r\n      \""codeFlows\"": [\r\n        {\r\n          \""message\"": \""Path from declaration to usage\"",\r\n          \""locations\"": [\r\n            {\r\n              \""physicalLocation\"": {\r\n                \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n                \""region\"": {\r\n                  \""startLine\"": 15\r\n                }\r\n              },\r\n              \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n              \""module\"": \""platform\"",\r\n              \""threadId\"": 52,\r\n              \""message\"": \""Variable \\\""ptr\\\"" declared.\"",\r\n              \""kind\"": \""declaration\"",\r\n              \""taintKind\"": 1,\r\n              \""target\"": \""foo::bar\"",\r\n              \""values\"": [\r\n                \""id\"",\r\n                \""name\"",\r\n                \""param3\""\r\n              ],\r\n              \""targetKey\"": \""collections::list::add\"",\r\n              \""importance\"": \""essential\"",\r\n              \""snippet\"": \""int *ptr;\""\r\n            },\r\n            {\r\n              \""step\"": 1,\r\n              \""physicalLocation\"": {\r\n                \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n                \""region\"": {\r\n                  \""startLine\"": 15\r\n                }\r\n              },\r\n              \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n              \""module\"": \""platform\"",\r\n              \""threadId\"": 52,\r\n              \""kind\"": \""assignment\"",\r\n              \""importance\"": \""unimportant\"",\r\n              \""snippet\"": \""offset = 0;\"",\r\n              \""annotations\"": [\r\n                {\r\n                  \""message\"": \""This is a test annotation\"",\r\n                  \""locations\"": [\r\n                    {\r\n                      \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n                      \""region\"": {\r\n                        \""startLine\"": 40\r\n                      }\r\n                    },\r\n                    {\r\n                      \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n                      \""region\"": {\r\n                        \""startLine\"": 240\r\n                      }\r\n                    }\r\n                  ]\r\n                },\r\n                {\r\n                  \""message\"": \""This is a second test annotation\"",\r\n                  \""locations\"": [\r\n                    {\r\n                      \""uri\"": \""file:///home/buildAgent/src/collections/foo.cpp\"",\r\n                      \""region\"": {\r\n                        \""startLine\"": 128\r\n                      }\r\n                    }\r\n                  ]\r\n                }\r\n              ]\r\n            },\r\n            {\r\n              \""step\"": 2,\r\n              \""physicalLocation\"": {\r\n                \""uri\"": \""file:///home/buildAgent/src/collections/list.h\"",\r\n                \""region\"": {\r\n                  \""startLine\"": 25\r\n                }\r\n              },\r\n              \""fullyQualifiedLogicalName\"": \""collections::list::add\"",\r\n              \""module\"": \""platform\"",\r\n              \""threadId\"": 52,\r\n              \""message\"": \""Uninitialized variable \\\""ptr\\\"" passed to method \\\""add_core\\\"".\"",\r\n              \""kind\"": \""callReturn\"",\r\n              \""target\"": \""collections::list::add_core\"",\r\n              \""state\"": {\r\n                \""Foo\"": \""bar\""\r\n              },\r\n              \""importance\"": \""essential\"",\r\n              \""snippet\"": \""add_core(ptr, offset, val)\""\r\n            }\r\n          ]\r\n        }\r\n      ]\r\n    }\r\n  ]\r\n}""
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }
    }
}
