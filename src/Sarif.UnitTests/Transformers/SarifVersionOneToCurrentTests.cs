// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Transformers
{
    public class SarifVersionOneToCurrentTests
    {
        private static readonly Assembly ThisAssembly;
        private static readonly string OutputFolderPath;
        private const string TestLogResourceNameRoot = "Microsoft.CodeAnalysis.Sarif.UnitTests.Transformers.TestLogs";

        static SarifVersionOneToCurrentTests()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            OutputFolderPath = Path.Combine(Path.GetDirectoryName(ThisAssembly.Location), "UnitTestOutput");

            if (Directory.Exists(OutputFolderPath))
            {
                Directory.Delete(OutputFolderPath, recursive: true);
            }

            Directory.CreateDirectory(OutputFolderPath);
        }

        private static SarifLogVersionOne GetSarifLogVersionOne(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLogVersionOne>(logText, SarifTransformerUtilities.JsonSettingsV1Indented);
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
            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsIndented);
            v2LogText.Should().Be(v2LogExpectedText);
        }

        private static void VerifyVersionOneToCurrentTransformationFromResource(string v1InputResourceName, string v2ExpectedResourceName)
        {
            string v1LogText = GetResourceText($"v1.{v1InputResourceName}");
            string v2ExpectedLogText = GetResourceText($"v2.{v2ExpectedResourceName}");

            v2ExpectedLogText = Utilities.UpdateVersionNumberToCurrent(v2ExpectedLogText);

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);
            string v2ActualLogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsIndented);

            if (v2ExpectedLogText != v2ActualLogText)
            {
                // Write the expected and actual log text to disk
                File.WriteAllText(GetOutputFilePath(v2ExpectedResourceName, "expected"), v2ExpectedLogText);
                File.WriteAllText(GetOutputFilePath(v2ExpectedResourceName, "actual"), v2ActualLogText);
            }

            v2ActualLogText.Should().Be(v2ExpectedLogText);
        }

        private static string GetOutputFilePath(string resourceName, string differentiator)
        {
            string fileName = string.Format("{0}.{1}.sarif",
                                                Path.GetFileNameWithoutExtension(resourceName),
                                                differentiator);
            return Path.Combine(OutputFolderPath, fileName);
        }

        private static string GetResourceText(string resourceName)
        {
            string text = null;

            using (Stream stream = ThisAssembly.GetManifestResourceStream($"{TestLogResourceNameRoot}.{resourceName}"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }

            return text;
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_RestoreFromPropertyBag()
        {
            const string V1LogText =
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

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      }
    }
  ]
}";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""properties"": {
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""}}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithTwoRuns()
        {
            const string V1LogText =
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

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": {""tool"":{""name"":""AssetScanner"",""semanticVersion"":""1.7.2""},""results"":[]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithPropertyAndTags()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0"",""properties"":{""foo"":""bar"",""tags"":[
  ""1"",
  ""2""
]}},""results"":[]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithLogicalLocations()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""logicalLocations"":{""collections::list::add"":{""name"":""add"",""parentKey"":""collections::list"",""kind"":""function""},""collections::list"":{""name"":""list"",""parentKey"":""collections"",""kind"":""type""},""collections"":{""name"":""collections"",""kind"":""namespace""}},""results"":[]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithFiles()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""files"":{""file:///home/list.txt"":{""length"":43,""mimeType"":""text/plain"",""contents"":""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha256""}]},""file:///home/buildAgent/bin/app.zip"":{""mimeType"":""application/zip"",""properties"":{""my_key"":""some value""}},""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"":{""uri"":""file:///docs/intro.docx"",""parentKey"":""file:///home/buildAgent/bin/app.zip"",""offset"":17522,""length"":4050,""mimeType"":""application/vnd.openxmlformats-officedocument.wordprocessingml.document""}},""results"":[]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWitRules()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
      ""results"": [
        {
          ""ruleId"": ""C2001""
        },
        {
          ""ruleId"": ""C2001"",
          ""ruleKey"": ""C2001""
        },
        {
          ""ruleId"": ""C2002"",
          ""ruleKey"": ""C2002-1""
        },
        {
          ""ruleKey"": ""C2003""
        }
      ]
    }
  ]
}";

            const string V2LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [
        {
          ""ruleId"": ""C2001""
        },
        {
          ""ruleId"": ""C2001""
        },
        {
          ""ruleId"": ""C2002-1""
        },
        {
          ""ruleId"": ""C2003""
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
          },
          ""C2002-1"": {
            ""id"": ""C2002""
          }
        }
      },
      ""properties"": {
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[{""ruleId"":""C2001""},{""ruleId"":""C2001"",""ruleKey"":""C2001""},{""ruleId"":""C2002"",""ruleKey"":""C2002-1""},{""ruleKey"":""C2003""}],""rules"":{""C2001"":{""id"":""C2001"",""shortDescription"":""A variable was used without being initialized."",""messageFormats"":{""default"":""Variable \""{0}\"" was used without being initialized.""},""properties"":{""some_key"":""FoxForceFive""}},""C2002"":{""id"":""C2002"",""fullDescription"":""Catfish season continuous hen lamb include dose copy grant."",""configuration"":""enabled"",""defaultLevel"":""error"",""helpUri"":""http://www.domain.com/rules/c2002.html""},""C2003"":{""id"":""C2003"",""name"":""Rule C2003"",""shortDescription"":""Rules were meant to be broken."",""fullDescription"":""Rent internal rebellion competence biography photograph."",""configuration"":""disabled"",""defaultLevel"":""pass""}}}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithBasicInvocation()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
          ""workingDirectory"": {
            ""uri"": ""/home/buildAgent/src""
          },
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocation"":{""commandLine"":""CodeScanner @collections.rsp"",""responseFiles"":{""collections.rsp"":""-input src/collections/*.cpp -log out/collections.sarif -rules all -disable C9999""},""startTime"":""2016-07-16T14:18:25.000Z"",""endTime"":""2016-07-16T14:19:01.000Z"",""machine"":""BLD01"",""account"":""buildAgent"",""processId"":1218,""fileName"":""/bin/tools/CodeScanner"",""workingDirectory"":""/home/buildAgent/src"",""environmentVariables"":{""PATH"":""/usr/local/bin:/bin:/bin/tools:/home/buildAgent/bin"",""HOME"":""/home/buildAgent"",""TZ"":""EST""}},""results"":[]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithInvocationAndNotifications()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
      ""results"": [],
      ""properties"": {
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""invocation"":{""commandLine"":""CodeScanner @collections.rsp""},""results"":[],""toolNotifications"":[{""id"":""CTN0001"",""message"":""Run started."",""level"":""note""},{""id"":""CTN9999"",""ruleId"":""C2152"",""physicalLocation"":{""uri"":""file:///home/buildAgent/src/crypto/hash.cpp""},""message"":""Exception evaluating rule \""C2152\"". Rule disabled; run continues."",""level"":""error"",""threadId"":52,""time"":""2016-07-16T14:18:43.119Z"",""exception"":{""kind"":""ExecutionEngine.RuleFailureException"",""message"":""Unhandled exception during rule evaluation."",""innerExceptions"":[{""kind"":""System.ArgumentException"",""message"":""length is < 0""}]}},{""id"":""CTN0002"",""message"":""Run ended."",""level"":""note""}],""configurationNotifications"":[{""id"":""UnknownRule"",""ruleId"":""ABC0001"",""message"":""Could not disable rule \""ABC0001\"" because there is no rule with that id.""}]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithNotificationsButNoInvocations()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""results"":[],""toolNotifications"":[{""id"":""CTN0001"",""message"":""Run started."",""level"":""note""}],""configurationNotifications"":[{""id"":""UnknownRule"",""ruleId"":""ABC0001"",""message"":""Could not disable rule \""ABC0001\"" because there is no rule with that id.""}]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_NotificationExceptionWithStack()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
                            ""startColumn"": 9,
                            ""endColumn"": 9
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""logicalLocations"":{""Rules.SecureHashAlgorithmRule.Evaluate"":{""name"":""Evaluate"",""kind"":""some kind""},""Rules.SecureHashAlgorithmRule.Register"":{""name"":""InvalidName""},""ExecutionEngine.Engine.FooBar"":{""name"":""FooBar"",""kind"":""another kind""}},""results"":[],""toolNotifications"":[{""id"":""CTN0001"",""message"":""Unhandled exception."",""level"":""error"",""exception"":{""kind"":""ExecutionEngine.RuleFailureException"",""message"":""Unhandled exception during rule evaluation."",""stack"":{""message"":""This is the stack messasge."",""frames"":[{""message"":""Exception thrown"",""uri"":""file:///C:/src/main.cs"",""line"":15,""column"":9,""module"":""RuleLibrary"",""threadId"":52,""fullyQualifiedLogicalName"":""Rules.SecureHashAlgorithmRule.Evaluate"",""address"":10092852},{""uri"":""file:///C:/src/main.cs"",""module"":""RuleLibrary"",""threadId"":52,""fullyQualifiedLogicalName"":""Rules.SecureHashAlgorithmRule.Register"",""address"":1002485},{""uri"":""file:///C:/src/utils.cs"",""module"":""ExecutionEngine"",""threadId"":52,""fullyQualifiedLogicalName"":""ExecutionEngine.Engine.EvaluateRule"",""address"":10073356,""offset"":10475},{""uri"":""file:///C:/src/foobar.cs"",""module"":""ExecutionEngine"",""threadId"":52,""fullyQualifiedLogicalName"":""ExecutionEngine.Engine.EvaluateRule"",""logicalLocationKey"":""ExecutionEngine.Engine.FooBar"",""address"":10073356,""offset"":10475}]},""innerExceptions"":[{""kind"":""System.ArgumentException"",""message"":""length is < 0""}]}}]}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_BasicResult()
        {
            const string V1LogText =
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
          ""mimeType"": ""text/plain"",
          ""length"": 43,
          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",
          ""hashes"": [
            {
              ""algorithm"": ""sha256"",
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592""
            }
          ]
        }
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
                ""uri"": ""file:///home/list.txt"",
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 5,
                  ""length"": 4
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add""
            },
            {
              ""message"": ""\""count\"" was declared here."",
              ""physicalLocation"": {
                ""uri"": ""file:///home/list.txt"",
                ""region"": {
                  ""offset"": 12,
                  ""length"": 3
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
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
        }
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
            ""messageId"": ""default"",
            ""arguments"": [
              ""ptr""
            ]
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
                  ""byteLength"": 27,
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
                  ""uri"": ""file:///home/list.txt""
                },
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 5,
                  ""byteLength"": 4
                }
              },
              ""fullyQualifiedLogicalName"": ""collections::list::add"",
              ""message"": {
                ""text"": ""\""count\"" was declared here.""
              }
            },
            {
              ""physicalLocation"": {
                ""fileLocation"": {
                  ""uri"": ""file:///home/list.txt""
                },
                ""region"": {
                  ""byteOffset"": 12,
                  ""byteLength"": 3
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner"",""semanticVersion"":""2.1.0""},""files"":{""file:///home/list.txt"":{""length"":43,""mimeType"":""text/plain"",""contents"":""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",""hashes"":[{""value"":""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",""algorithm"":""sha256""}]}},""logicalLocations"":{""collections::list::add"":{""name"":""add"",""parentKey"":""collections::list"",""kind"":""function""},""collections::list"":{""name"":""list"",""parentKey"":""collections"",""kind"":""type""},""collections"":{""name"":""collections"",""kind"":""namespace""}},""results"":[{""ruleId"":""C2001"",""level"":""error"",""formattedRuleMessage"":{""formatId"":""default"",""arguments"":[""ptr""]},""locations"":[{""analysisTarget"":{""uri"":""file:///home/buildAgent/src/collections/list.cpp""},""resultFile"":{""uri"":""file:///home/buildAgent/src/collections/list.h"",""region"":{""startLine"":1,""startColumn"":1,""endLine"":1,""endColumn"":28,""length"":27}},""fullyQualifiedLogicalName"":""collections::list::add"",""decoratedName"":""?add@list@collections@@QAEXH@Z""}],""snippet"":""add_core(ptr, offset, val);"",""relatedLocations"":[{""physicalLocation"":{""uri"":""file:///home/list.txt"",""region"":{""startLine"":1,""startColumn"":5,""length"":4}},""fullyQualifiedLogicalName"":""collections::list::add"",""message"":""\""count\"" was declared here.""},{""physicalLocation"":{""uri"":""file:///home/list.txt"",""region"":{""offset"":12,""length"":3}},""fullyQualifiedLogicalName"":""collections::list::add"",""message"":""\""count\"" was declared here.""}],""suppressionStates"":2,""baselineState"":""existing""}],""rules"":{""C2001"":{""id"":""C2001"",""shortDescription"":""A variable was used without being initialized."",""fullDescription"":""A variable was used without being initialized. This can result in runtime errors such as null reference exceptions."",""messageFormats"":{""default"":""Variable \""{0}\"" was used without being initialized.""}}}}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_TwoResultsWithFixes()
        {
            const string V1LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
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
  ""$schema"": ""http://json.schemastore.org/sarif-" + SarifUtilities.VCurrent + @""",
  ""version"": """ + SarifUtilities.VCurrent + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner""
      },
      ""results"": [
        {
          ""ruleId"": ""WEB1079"",
          ""message"": {
            ""messageId"": ""default"",
            ""arguments"": [
              ""shape""
            ]
          },
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
            ""messageId"": ""default"",
            ""arguments"": [
              ""DIV""
            ]
          },
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
                    ""uri"": ""http://localhost:34420/HtmlFixes.html""
                  },
                  ""replacements"": [
                    {
                      ""deletedRegion"": {
                        ""byteOffset"": 804,
                        ""byteLength"": 3
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
        ""sarifv1/run"": {""tool"":{""name"":""CodeScanner""},""results"":[{""ruleId"":""WEB1079"",""formattedRuleMessage"":{""formatId"":""default"",""arguments"":[""shape""]},""locations"":[{""analysisTarget"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""resultFile"":{""uri"":""http://localhost:34420/HtmlFixes.html"",""region"":{""startLine"":20,""startColumn"":69,""endColumn"":74,""offset"":720,""length"":5}}}],""snippet"":""<area alt=\""Here is some text\"" coords=\""10 20 20\"" href=\""moon.html\"" shape=circle xweb:fixindex=\""0\"" />"",""fixes"":[{""description"":""Wrap attribute values in single quotes."",""fileChanges"":[{""uri"":""http://localhost:34420/HtmlFixes.html"",""replacements"":[{""offset"":720,""insertedBytes"":""Jw==""},{""offset"":725,""insertedBytes"":""Jw==""}]}]},{""description"":""Wrap attribute value in double quotes."",""fileChanges"":[{""uri"":""http://localhost:34420/HtmlFixes.html"",""replacements"":[{""offset"":720,""insertedBytes"":""Ig==""},{""offset"":725,""insertedBytes"":""Ig==""}]}]}]},{""ruleId"":""WEB1066"",""formattedRuleMessage"":{""formatId"":""default"",""arguments"":[""DIV""]},""locations"":[{""analysisTarget"":{""uri"":""http://localhost:34420/HtmlFixes.html""},""resultFile"":{""uri"":""http://localhost:34420/HtmlFixes.html"",""region"":{""startLine"":24,""startColumn"":4,""endColumn"":38,""offset"":803,""length"":34}}}],""snippet"":""<DIV id=\""test1\"" xweb:fixindex=\""0\""></DIV>"",""fixes"":[{""description"":""Convert tag name to lowercase."",""fileChanges"":[{""uri"":""http://localhost:34420/HtmlFixes.html"",""replacements"":[{""offset"":804,""deletedLength"":3,""insertedBytes"":""ZGl2""}]}]}]}],""rules"":{""WEB1079.AttributeValueIsNotQuoted"":{""id"":""WEB1079"",""shortDescription"":""The attribute value is not quoted."",""messageFormats"":{""default"":""The  value of the '{0}' attribute is not quoted. Wrap the attribute value in single or double quotes.""}},""WEB1066.TagNameIsNotLowercase"":{""id"":""WEB1066"",""shortDescription"":""The tag name is not lowercase."",""messageFormats"":{""default"":""Convert the name of the <{0}> tag to lowercase.""}}}}
      }
    }
  ]
}";

            VerifyVersionOneToCurrentTransformation(V1LogText, V2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_CodeFlows()
        {
            VerifyVersionOneToCurrentTransformationFromResource(v1InputResourceName: "CodeFlows.sarif",
                                                                v2ExpectedResourceName: "CodeFlows.sarif");
        }
    }
}
