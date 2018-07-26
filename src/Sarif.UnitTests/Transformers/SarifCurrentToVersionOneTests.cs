﻿// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Transformers
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[]}
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
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[]}
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
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[]}
      }
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""AssetScanner"",""semanticVersion"":""1.7.2""},""results"":[]}
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
          ""kind"": ""namespace""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""logicalLocations"":{""collections::list::add"":{""name"":""add"",""parentKey"":""collections::list"",""kind"":""function""},""collections::list"":{""name"":""list"",""parentKey"":""collections"",""kind"":""type""},""collections"":{""kind"":""namespace""}},""results"":[]}
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
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""files"":{""file:///home/list.txt"":{""length"":43,""mimeType"":""text/plain"",""contents"":{""text"":""The quick brown fox jumps over the lazy dog"",""binary"":""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==""},""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha-256""}]},""file:///home/buildAgent/bin/app.zip"":{""mimeType"":""application/zip"",""properties"":{""my_key"":""some value""}},""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"":{""fileLocation"":{""uri"":""file:///docs/intro.docx""},""parentKey"":""file:///home/buildAgent/bin/app.zip"",""offset"":17522,""length"":4050,""mimeType"":""application/vnd.openxmlformats-officedocument.wordprocessingml.document"",""contents"":{}}},""results"":[]}
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
      ""results"": [
        {
          ""ruleId"": ""C2009""
        },
        {
          ""ruleId"": ""C2002""
        },
        {
          ""ruleId"": ""C2003""
        },
        {
          ""ruleId"": ""C2003-1""
        }
      ],
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
            ""helpUri"": ""http://www.domain.com/rules/c2002.html""
          },
          ""C2003-1"": {
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
      ""results"": [
        {
          ""ruleId"": ""C2009""
        },
        {
          ""ruleId"": ""C2002""
        },
        {
          ""ruleId"": ""C2003""
        },
        {
          ""ruleId"": ""C2003"",
          ""ruleKey"": ""C2003-1""
        }
      ],
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
        ""C2003-1"": {
          ""id"": ""C2003"",
          ""name"": ""Rule C2003"",
          ""shortDescription"": ""Rules were meant to be broken."",
          ""fullDescription"": ""Rent internal rebellion competence biography photograph.""
        }
      },
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[{""ruleId"":""C2009""},{""ruleId"":""C2002""},{""ruleId"":""C2003""},{""ruleId"":""C2003-1""}],""resources"":{""rules"":{""C2001"":{""id"":""C2001"",""shortDescription"":{""text"":""A variable was used without being initialized.""},""messageStrings"":{""default"":""Variable \""{0}\"" was used without being initialized.""},""properties"":{""some_key"":""FoxForceFive""}},""C2002"":{""id"":""C2002"",""fullDescription"":{""text"":""Catfish season continuous hen lamb include dose copy grant.""},""configuration"":{""enabled"":true,""defaultLevel"":""error""},""helpUri"":""http://www.domain.com/rules/c2002.html""},""C2003-1"":{""id"":""C2003"",""name"":{""text"":""Rule C2003""},""shortDescription"":{""text"":""Rules were meant to be broken.""},""fullDescription"":{""text"":""Rent internal rebellion competence biography photograph.""}}}}}
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
      ""files"": {
        ""collections.rsp"": {
          ""uri"": ""collections.rsp""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocations"":[{""commandLine"":""CodeScanner @collections.rsp"",""responseFiles"":[{""uri"":""collections.rsp""}],""startTime"":""2016-07-16T14:18:25.000Z"",""endTime"":""2016-07-16T14:19:01.000Z"",""machine"":""BLD01"",""account"":""buildAgent"",""processId"":1218,""executableLocation"":{""uri"":""/bin/tools/CodeScanner""},""workingDirectory"":""/home/buildAgent/src"",""environmentVariables"":{""PATH"":""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin"",""HOME"":""/home/buildAgent"",""TZ"":""EST""}}],""files"":{""collections.rsp"":{""fileLocation"":{""uri"":""collections.rsp""},""contents"":{""text"":""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999""}}},""results"":[]}
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
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocations"":[{""commandLine"":""CodeScanner @collections.rsp"",""toolNotifications"":[{""id"":""CTN0001"",""message"":{""text"":""Run started.""},""level"":""note""},{""id"":""CTN9999"",""ruleId"":""C2152"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///home/buildAgent/src/crypto/hash.cpp""}},""message"":{""text"":""Exception evaluating rule \""C2152\"". Rule disabled; run continues.""},""level"":""error"",""threadId"":52,""time"":""2016-07-16T14:18:43.119Z"",""exception"":{""kind"":""ExecutionEngine.RuleFailureException"",""message"":""Unhandled exception during rule evaluation."",""innerExceptions"":[{""kind"":""System.ArgumentException"",""message"":""length is < 0""}]}},{""id"":""CTN0002"",""message"":{""text"":""Run ended.""},""level"":""note""}],""configurationNotifications"":[{""id"":""UnknownRule"",""ruleId"":""ABC0001"",""message"":{""text"":""Could not disable rule \""ABC0001\"" because there is no rule with that id.""}}]}],""results"":[]}
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_NotificationExceptionWithStack()
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
          ""kind"": ""another kind""
        },
        ""Rules.SecureHashAlgorithmRule.Register-0"": {
          ""name"": ""Register""
        },
        ""ExecutionEngine.Engine.EvaluateRule"": {
          ""name"": ""EvaluateRule""
        }
      },
      ""results"": [],
      ""toolNotifications"": [
        {
          ""id"": ""CTN0001"",
          ""message"": ""Unhandled exception."",
          ""level"": ""error"",
          ""exception"": {
            ""kind"": ""ExecutionEngine.RuleFailureException"",
            ""message"": ""Unhandled exception during rule evaluation."",
            ""stack"": {
              ""message"": ""This is the stack messasge."",
              ""frames"": [
                {
                  ""message"": ""Exception thrown"",
                  ""uri"": ""file:///C:/src/main.cs"",
                  ""line"": 15,
                  ""column"": 9,
                  ""module"": ""RuleLibrary"",
                  ""threadId"": 52,
                  ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Evaluate"",
                  ""address"": 10092852
                },
                {
                  ""uri"": ""file:///C:/src/main.cs"",
                  ""module"": ""RuleLibrary"",
                  ""threadId"": 52,
                  ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Register"",
                  ""logicalLocationKey"": ""Rules.SecureHashAlgorithmRule.Register-0"",
                  ""address"": 1002485
                },
                {
                  ""uri"": ""file:///C:/src/utils.cs"",
                  ""module"": ""ExecutionEngine"",
                  ""threadId"": 52,
                  ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule"",
                  ""address"": 10073356,
                  ""offset"": 10475
                },
                {
                  ""uri"": ""file:///C:/src/foobar.cs"",
                  ""module"": ""ExecutionEngine"",
                  ""threadId"": 52,
                  ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule"",
                  ""logicalLocationKey"": ""ExecutionEngine.Engine.FooBar"",
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
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocations"":[{""toolNotifications"":[{""id"":""CTN0001"",""message"":{""text"":""Unhandled exception.""},""level"":""error"",""exception"":{""kind"":""ExecutionEngine.RuleFailureException"",""message"":""Unhandled exception during rule evaluation."",""stack"":{""message"":{""text"":""This is the stack messasge.""},""frames"":[{""location"":{""physicalLocation"":{""fileLocation"":{""uri"":""file:///C:/src/main.cs""},""region"":{""startLine"":15,""startColumn"":9}},""fullyQualifiedLogicalName"":""Rules.SecureHashAlgorithmRule.Evaluate"",""message"":{""text"":""Exception thrown""}},""module"":""RuleLibrary"",""threadId"":52,""address"":10092852},{""location"":{""physicalLocation"":{""fileLocation"":{""uri"":""file:///C:/src/main.cs""}},""fullyQualifiedLogicalName"":""Rules.SecureHashAlgorithmRule.Register-0""},""module"":""RuleLibrary"",""threadId"":52,""address"":1002485},{""location"":{""physicalLocation"":{""fileLocation"":{""uri"":""file:///C:/src/utils.cs""}},""fullyQualifiedLogicalName"":""ExecutionEngine.Engine.EvaluateRule""},""module"":""ExecutionEngine"",""threadId"":52,""address"":10073356,""offset"":10475},{""location"":{""physicalLocation"":{""fileLocation"":{""uri"":""file:///C:/src/foobar.cs""}},""fullyQualifiedLogicalName"":""ExecutionEngine.Engine.FooBar""},""module"":""ExecutionEngine"",""threadId"":52,""address"":10073356,""offset"":10475}]},""innerExceptions"":[{""kind"":""System.ArgumentException"",""message"":""length is < 0""}]}}]}],""logicalLocations"":{""Rules.SecureHashAlgorithmRule.Evaluate"":{""name"":""Evaluate"",""kind"":""some kind""},""Rules.SecureHashAlgorithmRule.Register"":{""name"":""InvalidName""},""ExecutionEngine.Engine.FooBar"":{""name"":""EvaluateRule"",""fullyQualifiedName"":""ExecutionEngine.Engine.EvaluateRule"",""kind"":""another kind""},""Rules.SecureHashAlgorithmRule.Register-0"":{""name"":""Register"",""fullyQualifiedName"":""Rules.SecureHashAlgorithmRule.Register""},""ExecutionEngine.Engine.EvaluateRule"":{""name"":""EvaluateRule""}},""results"":[]}
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_ResultLocations()
        {
            const string V2LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner""
      },
      ""logicalLocations"": {
        ""collections::list::add"": {
          ""name"": ""add"",
          ""kind"": ""function"",
          ""decoratedName"": ""?add@list@collections@@QAEXH@Z"",
          ""parentKey"": ""collections::list""
        },
        ""collections::list"": {
          ""name"": ""list"",
          ""kind"": ""type"",
          ""parentKey"": ""collections""
        },
        ""collections"": {
          ""kind"": ""namespace""
        }
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
                  ""byteOffset"": 720,
                  ""byteLength"": 5,
                  ""snippet"": {
                    ""text"": ""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />""
                  }
                }
              }
            }
          ]
        },
        {
          ""ruleId"": ""WEB1066"",
          ""analysisTarget"": {
            ""uri"": ""http://localhost:34420/HtmlFixes.html""
          },
          ""message"": {
            ""arguments"": [
              ""DIV""
            ]
          },
          ""ruleMessageId"": ""default"",
          ""locations"": [
            {
              ""fullyQualifiedLogicalName"": ""collections::list::add"",
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38,
                  ""byteOffset"": 803,
                  ""byteLength"": 34,
                  ""snippet"": {
                    ""text"": ""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>""
                  }
                }
              }
            }
          ],
          ""relatedLocations"": [
            {
              ""message"": {
                ""text"": ""Second location with this violation""
              },
              ""annotations"": [
                {
                  ""byteOffset"": 1345,
                  ""byteLength"": 22,
                  ""message"": {
                    ""text"": ""Foxes, quick and brown""
                  }
                }
              ],
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""http://localhost:34420/HtmlFixes.html""
                },
                ""region"": {
                  ""startLine"": 56,
                  ""startColumn"": 8,
                  ""endColumn"": 78,
                  ""byteOffset"": 1124,
                  ""byteLength"": 22,
                  ""snippet"": {
                    ""text"": ""<DIV id=\""test3\""></DIV>""
                  }
                }
              }
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
        ""name"": ""CodeScanner""
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
      ""results"": [
        {
          ""ruleId"": ""WEB1079"",
          ""formattedRuleMessage"": {
            ""formatId"": ""default"",
            ""arguments"": [
              ""shape""
            ]
          },
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                ""region"": {
                  ""startLine"": 20,
                  ""startColumn"": 69,
                  ""endColumn"": 74
                }
              }
            }
          ],
          ""snippet"": ""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />""
        },
        {
          ""ruleId"": ""WEB1066"",
          ""formattedRuleMessage"": {
            ""formatId"": ""default"",
            ""arguments"": [
              ""DIV""
            ]
          },
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
                  ""endColumn"": 38
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add"",
              ""decoratedName"": ""?add@list@collections@@QAEXH@Z""
            }
          ],
          ""snippet"": ""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>"",
          ""relatedLocations"": [
            {
              ""physicalLocation"": {
                ""uri"": ""http://localhost:34420/HtmlFixes.html"",
                ""region"": {
                  ""startLine"": 56,
                  ""startColumn"": 8,
                  ""endColumn"": 78
                }
              },
              ""message"": ""Second location with this violation"",
              ""snippet"": ""<DIV id=\""test3\""></DIV>"",
              ""annotations"": [
                {
                  ""message"": ""Foxes, quick and brown"",
                  ""locations"": [
                    {
                      ""region"": {
                        ""offset"": 1345,
                        ""length"": 22
                      }
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
      },
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner""},""logicalLocations"":{""collections::list::add"":{""name"":""add"",""decoratedName"":""?add@list@collections@@QAEXH@Z"",""parentKey"":""collections::list"",""kind"":""function""},""collections::list"":{""name"":""list"",""parentKey"":""collections"",""kind"":""type""},""collections"":{""kind"":""namespace""}},""results"":[{""ruleId"":""WEB1079"",""message"":{""arguments"":[""shape""]},""ruleMessageId"":""default"",""locations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""region"":{""startLine"":20,""startColumn"":69,""endColumn"":74,""byteOffset"":720,""byteLength"":5,""snippet"":{""text"":""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />""}}}}]},{""ruleId"":""WEB1066"",""message"":{""arguments"":[""DIV""]},""ruleMessageId"":""default"",""analysisTarget"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""locations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""region"":{""startLine"":24,""startColumn"":4,""endColumn"":38,""byteOffset"":803,""byteLength"":34,""snippet"":{""text"":""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>""}}},""fullyQualifiedLogicalName"":""collections::list::add""}],""relatedLocations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""region"":{""startLine"":56,""startColumn"":8,""endColumn"":78,""byteOffset"":1124,""byteLength"":22,""snippet"":{""text"":""<DIV id=\""test3\""></DIV>""}}},""message"":{""text"":""Second location with this violation""},""annotations"":[{""byteOffset"":1345,""byteLength"":22,""message"":{""text"":""Foxes, quick and brown""}}]}]}],""resources"":{""rules"":{""WEB1079.AttributeValueIsNotQuoted"":{""id"":""WEB1079"",""shortDescription"":{""text"":""The attribute value is not quoted.""},""messageStrings"":{""default"":""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.""}},""WEB1066.TagNameIsNotLowercase"":{""id"":""WEB1066"",""shortDescription"":{""text"":""The tag name is not lowercase.""},""messageStrings"":{""default"":""Convert the name of the <{0}> tag to lowercase.""}}}}}
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_TwoResultsWithFixes()
        {
            const string V2LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner""
      },
      ""files"": {
        ""http://localhost:34420/HtmlFixes.html"": {
          ""length"": 43,
          ""enoding"": ""utf-8"",
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
        ""http://localhost:34420/HtmlTextFix.html"": {
          ""length"": 87,
          ""encoding"": ""foo-bar"",
          ""mimeType"": ""text/plain"",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha-256""
            }
          ]
        },
        ""http://localhost:34420/HtmlTextFixNoEncoding.html"": {
          ""length"": 87,
          ""mimeType"": ""text/plain"",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha-256""
            }
          ]
        }
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
                        ""byteOffset"": 720
                      },
                      ""insertedContent"": {
                        ""binary"": ""Jw==""
                      }
                    },
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 725
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
                        ""byteOffset"": 720
                      },
                      ""insertedContent"": {
                        ""binary"": ""Ig==""
                      }
                    },
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 725
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
                  ""uri"": ""http://localhost:34420/HtmlTextFix.html""
                },
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38,
                  ""byteOffset"": 803,
                  ""byteLength"": 34,
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
                    ""uri"": ""http://localhost:34420/HtmlTextFix.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 125,
                        ""byteLength"": 3
                      },
                      ""insertedContent"": {
                        ""text"": ""<div>""
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
                  ""uri"": ""http://localhost:34420/HtmlTextFixNoEncoding.html""
                },
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38,
                  ""byteOffset"": 803,
                  ""byteLength"": 34,
                  ""snippet"": {
                    ""text"": ""<DIV id=\""test2\"" xweb:fixindex=\""0\""></DIV>""
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
                    ""uri"": ""http://localhost:34420/HtmlTextFixNoEncoding.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 125,
                        ""byteLength"": 3
                      },
                      ""insertedContent"": {
                        ""text"": ""<div>""
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
                  ""uri"": ""/wwwroot/vdir/HtmlTextFixNoEntry.html""
                },
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38,
                  ""byteOffset"": 803,
                  ""byteLength"": 34,
                  ""snippet"": {
                    ""text"": ""<DIV id=\""test3\"" xweb:fixindex=\""0\""></DIV>""
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
                    ""uri"": ""/wwwroot/vdir/HtmlTextFixNoEntry.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 125,
                        ""byteLength"": 3
                      },
                      ""insertedContent"": {
                        ""text"": ""<div>""
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
        ""name"": ""CodeScanner""
      },
      ""files"": {
        ""http://localhost:34420/HtmlFixes.html"": {
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
        ""http://localhost:34420/HtmlTextFix.html"": {
          ""length"": 87,
          ""mimeType"": ""text/plain"",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha256""
            }
          ]
        },
        ""http://localhost:34420/HtmlTextFixNoEncoding.html"": {
          ""length"": 87,
          ""mimeType"": ""text/plain"",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha256""
            }
          ]
        }
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
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""http://localhost:34420/HtmlTextFix.html"",
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38
                }
              }
            }
          ],
          ""snippet"": ""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>""
        },
        {
          ""ruleId"": ""WEB1066"",
          ""formattedRuleMessage"": {
            ""formatId"": ""default"",
            ""arguments"": [
              ""DIV""
            ]
          },
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""http://localhost:34420/HtmlTextFixNoEncoding.html"",
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38
                }
              }
            }
          ],
          ""snippet"": ""<DIV id=\""test2\"" xweb:fixindex=\""0\""></DIV>""
        },
        {
          ""ruleId"": ""WEB1066"",
          ""formattedRuleMessage"": {
            ""formatId"": ""default"",
            ""arguments"": [
              ""DIV""
            ]
          },
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""/wwwroot/vdir/HtmlTextFixNoEntry.html"",
                ""region"": {
                  ""startLine"": 24,
                  ""startColumn"": 4,
                  ""endColumn"": 38
                }
              }
            }
          ],
          ""snippet"": ""<DIV id=\""test3\"" xweb:fixindex=\""0\""></DIV>""
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
      },
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner""},""files"":{""http://localhost:34420/HtmlFixes.html"":{""length"":43,""mimeType"":""text/plain"",""contents"":{""text"":""The quick brown fox jumps over the lazy dog"",""binary"":""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==""},""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha-256""}]},""http://localhost:34420/HtmlTextFix.html"":{""length"":87,""mimeType"":""text/plain"",""encoding"":""foo-bar"",""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha-256""}]},""http://localhost:34420/HtmlTextFixNoEncoding.html"":{""length"":87,""mimeType"":""text/plain"",""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha-256""}]}},""results"":[{""ruleId"":""WEB1079"",""message"":{""arguments"":[""shape""]},""ruleMessageId"":""default"",""fixes"":[{""description"":{""text"":""Wrap attribute values in single quotes.""},""fileChanges"":[{""fileLocation"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""replacements"":[{""deletedRegion"":{""byteOffset"":720},""insertedContent"":{""binary"":""Jw==""}},{""deletedRegion"":{""byteOffset"":725},""insertedContent"":{""binary"":""Jw==""}}]}]},{""description"":{""text"":""Wrap attribute value in double quotes.""},""fileChanges"":[{""fileLocation"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""replacements"":[{""deletedRegion"":{""byteOffset"":720},""insertedContent"":{""binary"":""Ig==""}},{""deletedRegion"":{""byteOffset"":725},""insertedContent"":{""binary"":""Ig==""}}]}]}]},{""ruleId"":""WEB1066"",""message"":{""arguments"":[""DIV""]},""ruleMessageId"":""default"",""locations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""http://localhost:34420/HtmlTextFix.html""},""region"":{""startLine"":24,""startColumn"":4,""endColumn"":38,""byteOffset"":803,""byteLength"":34,""snippet"":{""text"":""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>""}}}}],""fixes"":[{""description"":{""text"":""Convert tag name to lowercase.""},""fileChanges"":[{""fileLocation"":{""uri"":""http://localhost:34420/HtmlTextFix.html""},""replacements"":[{""deletedRegion"":{""byteOffset"":125,""byteLength"":3},""insertedContent"":{""text"":""<div>""}}]}]}]},{""ruleId"":""WEB1066"",""message"":{""arguments"":[""DIV""]},""ruleMessageId"":""default"",""locations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""http://localhost:34420/HtmlTextFixNoEncoding.html""},""region"":{""startLine"":24,""startColumn"":4,""endColumn"":38,""byteOffset"":803,""byteLength"":34,""snippet"":{""text"":""<DIV id=\""test2\"" xweb:fixindex=\""0\""></DIV>""}}}}],""fixes"":[{""description"":{""text"":""Convert tag name to lowercase.""},""fileChanges"":[{""fileLocation"":{""uri"":""http://localhost:34420/HtmlTextFixNoEncoding.html""},""replacements"":[{""deletedRegion"":{""byteOffset"":125,""byteLength"":3},""insertedContent"":{""text"":""<div>""}}]}]}]},{""ruleId"":""WEB1066"",""message"":{""arguments"":[""DIV""]},""ruleMessageId"":""default"",""locations"":[{""physicalLocation"":{""fileLocation"":{""uri"":""/wwwroot/vdir/HtmlTextFixNoEntry.html""},""region"":{""startLine"":24,""startColumn"":4,""endColumn"":38,""byteOffset"":803,""byteLength"":34,""snippet"":{""text"":""<DIV id=\""test3\"" xweb:fixindex=\""0\""></DIV>""}}}}],""fixes"":[{""description"":{""text"":""Convert tag name to lowercase.""},""fileChanges"":[{""fileLocation"":{""uri"":""/wwwroot/vdir/HtmlTextFixNoEntry.html""},""replacements"":[{""deletedRegion"":{""byteOffset"":125,""byteLength"":3},""insertedContent"":{""text"":""<div>""}}]}]}]}],""resources"":{""rules"":{""WEB1079.AttributeValueIsNotQuoted"":{""id"":""WEB1079"",""shortDescription"":{""text"":""The attribute value is not quoted.""},""messageStrings"":{""default"":""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.""}},""WEB1066.TagNameIsNotLowercase"":{""id"":""WEB1066"",""shortDescription"":{""text"":""The tag name is not lowercase.""},""messageStrings"":{""default"":""Convert the name of the <{0}> tag to lowercase.""}}}}}
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Regions()
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
              ""id"": ""RegionTest1"",
              ""message"": {
                ""text"": ""Ambiguous region end with no start values""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/base/driver.cs""
                },
                ""region"": {
                  ""charOffset"": 8
                }
              }
            },
            {
              ""id"": ""RegionTest2"",
              ""message"": {
                ""text"": ""startColumn/Line same as charOffset""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/base/driver.cs""
                },
                ""region"": {
                  ""startLine"": 2,
                  ""startColumn"": 4,
                  ""charOffset"": 9
                }
              }
            },
            {
              ""id"": ""RegionTest3"",
              ""message"": {
                ""text"": ""startColumn/Line same as charOffset""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/base/driver.cs""
                },
                ""region"": {
                  ""startLine"": 2,
                  ""startColumn"": 1,
                  ""charOffset"": 6
                }
              }
            },
            {
              ""id"": ""RegionTest4"",
              ""message"": {
                ""text"": ""startLine and startColumn only""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/base/driver.cs""
                },
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 2
                }
              }
            },
            {
              ""id"": ""RegionTest5"",
              ""message"": {
                ""text"": ""startLine only""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/ui/client.cs""
                },
                ""region"": {
                  ""startLine"": 2
                }
              }
            },
            {
              ""id"": ""RegionTest6"",
              ""message"": {
                ""text"": ""One whole line""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/ui/client.cs""
                },
                ""region"": {
                  ""startLine"": 2,
                  ""endLine"": 3,
                  ""message"": {
                    ""text"": ""This region includes all of line 2, excluding the trailing newline sequence""
                  }
                }
              }
            },
            {
              ""id"": ""RegionTest7"",
              ""message"": {
                ""text"": ""Unicode text property""
              },
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///src/ui/unicodeText.cs""
                },
                ""region"": {
                  ""charOffset"": 8,
                  ""charLength"": 3
                }
              }
            }
          ]
        }
      ],
      ""files"": {
        ""file:///src/base/driver.cs"": {
          ""fileLocation"": {
            ""uri"": ""file:///src/base/driver.cs""
          },
          ""encoding"": ""unicode"",
          ""contents"": {
            ""binary"": ""YQBiAGMAZAANAAoAZQBmAGcADQAKAGgAaQBqAGsADQAKAGwAbQBuAA==""
          }
        },
        ""file:///src/ui/client.cs"": {
          ""fileLocation"": {
            ""uri"": ""file:///src/ui/client.cs""
          },
          ""mimeType"": ""text/x-csharp"",
          ""contents"": {
            ""text"": ""The quick brown fox\r\njumps over the laazy dog""
          }
        },
        ""file:///src/ui/unicodeText.cs"": {
          ""fileLocation"": {
            ""uri"": ""file:///src/ui/unicodeText.cs""
          },
          ""mimeType"": ""text/x-csharp"",
          ""encoding"": ""unicode"",
          ""contents"": {
            ""text"": ""The quick brown fox\r\njumps over the laazy dog""
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
      ""invocation"": {},
      ""files"": {
        ""file:///src/base/driver.cs"": {
          ""uri"": ""file:///src/base/driver.cs"",
          ""contents"": ""YQBiAGMAZAANAAoAZQBmAGcADQAKAGgAaQBqAGsADQAKAGwAbQBuAA==""
        },
        ""file:///src/ui/client.cs"": {
          ""uri"": ""file:///src/ui/client.cs"",
          ""mimeType"": ""text/x-csharp"",
          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveA0KanVtcHMgb3ZlciB0aGUgbGFhenkgZG9n""
        },
        ""file:///src/ui/unicodeText.cs"": {
          ""uri"": ""file:///src/ui/unicodeText.cs"",
          ""mimeType"": ""text/x-csharp"",
          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveA0KanVtcHMgb3ZlciB0aGUgbGFhenkgZG9n""
        }
      },
      ""results"": [],
      ""toolNotifications"": [
        {
          ""id"": ""RegionTest1"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/base/driver.cs"",
            ""region"": {
              ""offset"": 16
            }
          },
          ""message"": ""Ambiguous region end with no start values""
        },
        {
          ""id"": ""RegionTest2"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/base/driver.cs"",
            ""region"": {
              ""startLine"": 2,
              ""startColumn"": 4
            }
          },
          ""message"": ""startColumn/Line same as charOffset""
        },
        {
          ""id"": ""RegionTest3"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/base/driver.cs"",
            ""region"": {
              ""startLine"": 2,
              ""startColumn"": 1
            }
          },
          ""message"": ""startColumn/Line same as charOffset""
        },
        {
          ""id"": ""RegionTest4"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/base/driver.cs"",
            ""region"": {
              ""startLine"": 1,
              ""startColumn"": 2
            }
          },
          ""message"": ""startLine and startColumn only""
        },
        {
          ""id"": ""RegionTest5"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/ui/client.cs"",
            ""region"": {
              ""startLine"": 2,
              ""startColumn"": 1
            }
          },
          ""message"": ""startLine only""
        },
        {
          ""id"": ""RegionTest6"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/ui/client.cs"",
            ""region"": {
              ""startLine"": 2,
              ""startColumn"": 1,
              ""endLine"": 3
            }
          },
          ""message"": ""One whole line""
        },
        {
          ""id"": ""RegionTest7"",
          ""physicalLocation"": {
            ""uri"": ""file:///src/ui/unicodeText.cs"",
            ""region"": {
              ""offset"": 16,
              ""length"": 6
            }
          },
          ""message"": ""Unicode text property""
        }
      ],
      ""properties"": {
        ""sarifv2/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocations"":[{""toolNotifications"":[{""id"":""RegionTest1"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/base/driver.cs""},""region"":{""charOffset"":8}},""message"":{""text"":""Ambiguous region end with no start values""}},{""id"":""RegionTest2"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/base/driver.cs""},""region"":{""startLine"":2,""startColumn"":4,""charOffset"":9}},""message"":{""text"":""startColumn/Line same as charOffset""}},{""id"":""RegionTest3"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/base/driver.cs""},""region"":{""startLine"":2,""startColumn"":1,""charOffset"":6}},""message"":{""text"":""startColumn/Line same as charOffset""}},{""id"":""RegionTest4"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/base/driver.cs""},""region"":{""startLine"":1,""startColumn"":2}},""message"":{""text"":""startLine and startColumn only""}},{""id"":""RegionTest5"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/ui/client.cs""},""region"":{""startLine"":2}},""message"":{""text"":""startLine only""}},{""id"":""RegionTest6"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/ui/client.cs""},""region"":{""startLine"":2,""endLine"":3,""message"":{""text"":""This region includes all of line 2, excluding the trailing newline sequence""}}},""message"":{""text"":""One whole line""}},{""id"":""RegionTest7"",""physicalLocation"":{""fileLocation"":{""uri"":""file:///src/ui/unicodeText.cs""},""region"":{""charOffset"":8,""charLength"":3}},""message"":{""text"":""Unicode text property""}}]}],""files"":{""file:///src/base/driver.cs"":{""fileLocation"":{""uri"":""file:///src/base/driver.cs""},""contents"":{""binary"":""YQBiAGMAZAANAAoAZQBmAGcADQAKAGgAaQBqAGsADQAKAGwAbQBuAA==""},""encoding"":""unicode""},""file:///src/ui/client.cs"":{""fileLocation"":{""uri"":""file:///src/ui/client.cs""},""mimeType"":""text/x-csharp"",""contents"":{""text"":""The quick brown fox\r\njumps over the laazy dog""}},""file:///src/ui/unicodeText.cs"":{""fileLocation"":{""uri"":""file:///src/ui/unicodeText.cs""},""mimeType"":""text/x-csharp"",""contents"":{""text"":""The quick brown fox\r\njumps over the laazy dog""},""encoding"":""unicode""}},""results"":[]}
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }
    }
}