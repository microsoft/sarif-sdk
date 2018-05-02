// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

        [Fact]
        public void SarifTransformerTests_ToCurrent_Minimum()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            Run run = v2Log.Runs[0];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.Name.Should().Be("CodeScanner");
            run.Tool.SemanticVersion.Should().Be("2.1.0");
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithTwoRuns()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""tool"": {
                        ""name"": ""CodeScanner"",
                        ""semanticVersion"": ""2.1.0""
                      },
                      ""results"": [
                      ]
                    },
                    {
                      ""tool"": {
                        ""name"": ""AssetScanner"",
                        ""semanticVersion"": ""1.7.2""
                      },
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(2);

            Run run = v2Log.Runs[0];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.Name.Should().Be("CodeScanner");
            run.Tool.SemanticVersion.Should().Be("2.1.0");

            run = v2Log.Runs[1];

            run.Results.Should().NotBeNull();
            run.Results.Count.Should().Be(0);

            run.Tool.Should().NotBeNull();
            run.Tool.SemanticVersion.Should().Be("1.7.2");
            run.Tool.Name.Should().Be("AssetScanner");
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithLogicalLocations()
        {
            string v1LogText =
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            IDictionary<string, LogicalLocation> logicalLocations = v2Log.Runs[0].LogicalLocations;

            logicalLocations.Should().NotBeNull();
            logicalLocations.Count.Should().Be(3);

            logicalLocations["collections::list::add"].Name.Should().Be("add");
            logicalLocations["collections::list::add"].Kind.Should().Be("function");
            logicalLocations["collections::list::add"].ParentKey.Should().Be("collections::list");

            logicalLocations["collections::list"].Name.Should().Be("list");
            logicalLocations["collections::list"].Kind.Should().Be("type");
            logicalLocations["collections::list"].ParentKey.Should().Be("collections");

            logicalLocations["collections"].Name.Should().Be("collections");
            logicalLocations["collections"].Kind.Should().Be("namespace");
            logicalLocations["collections"].ParentKey.Should().BeNull();
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithFiles()
        {
            string v1LogText =
              @"{
                  ""version"": ""1.0.0"",
                  ""runs"": [
                    {
                      ""files"": {
                        ""file:///home/list.txt"": {
                          ""mimeType"": ""text/plain"",
                          ""length"": 43,
                          ""contents"": ""The quick brown fox jumps over the lazy dog"",
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            IDictionary<string, FileData> files = v2Log.Runs[0].Files;

            files.Should().NotBeNull();
            files.Count.Should().Be(3);

            FileData fileData = files["file:///home/list.txt"];

            fileData.MimeType.Should().Be("text/plain");
            fileData.Length.Should().Be(43);
            fileData.FileLocation.Should().BeNull();
            fileData.Contents.Should().NotBeNull();
            fileData.Contents.Text.Should().Be("The quick brown fox jumps over the lazy dog");
            fileData.Hashes.Should().NotBeNull();
            fileData.Hashes[0].Algorithm.Should().Be("sha-256");
            fileData.Hashes[0].Value.Should().Be("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");
            fileData.Properties.Should().BeNull();

            fileData = files["file:///home/buildAgent/bin/app.zip"];

            fileData.MimeType.Should().Be("application/zip");
            fileData.Length.Should().Be(0);
            fileData.FileLocation.Should().BeNull();
            fileData.Contents.Should().NotBeNull();
            fileData.Contents.Binary.Should().BeNull();
            fileData.Hashes.Should().BeNull();
            fileData.Properties.Should().NotBeNull();
            fileData.Properties["my_key"].SerializedValue.Should().Be("\"some value\"");

            fileData = files["file:///home/buildAgent/bin/app.zip#/docs/intro.docx"];

            fileData.MimeType.Should().Be("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            fileData.ParentKey.Should().Be("file:///home/buildAgent/bin/app.zip");
            fileData.Offset.Should().Be(17522);
            fileData.Length.Should().Be(4050);
            fileData.FileLocation.Should().NotBeNull();
            fileData.FileLocation.Uri.Should().NotBeNull();
            fileData.FileLocation.Uri.OriginalString.Should().Be("file:///docs/intro.docx");
            fileData.Contents.Should().NotBeNull();
            fileData.Contents.Binary.Should().BeNull();
            fileData.Properties.Should().BeNull();
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWitRules()
        {
            string v1LogText =
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);
            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsV2);

            v2Log.Runs.Should().NotBeNull();
            v2Log.Runs.Count.Should().Be(1);

            Run run = v2Log.Runs[0];

            run.Resources.Should().NotBeNull();
            run.Resources.Rules.Should().NotBeNull();
            run.Resources.Rules.Count.Should().Be(3);

            Rule rule = run.Resources.Rules["C2001"];

            rule.Id.Should().Be("C2001");
            rule.Name.Should().BeNull();
            rule.ShortDescription.Should().NotBeNull();
            rule.ShortDescription.Text.Should().Be("A variable was used without being initialized.");
            rule.FullDescription.Should().BeNull();
            rule.Configuration.Should().BeNull();
            rule.HelpLocation.Should().BeNull();
            rule.MessageStrings.Should().NotBeNull();
            rule.MessageStrings.Count.Should().Be(1);
            rule.MessageStrings["default"].Should().Be("Variable \"{0}\" was used without being initialized.");
            rule.Properties.Should().NotBeNull();
            rule.Properties["some_key"].SerializedValue.Should().Be("\"FoxForceFive\"");

            rule = run.Resources.Rules["C2002"];

            rule.Id.Should().Be("C2002");
            rule.Name.Should().BeNull();
            rule.ShortDescription.Should().BeNull();
            rule.FullDescription.Should().NotBeNull();
            rule.FullDescription.Text.Should().Be("Catfish season continuous hen lamb include dose copy grant.");
            rule.Configuration.Should().NotBeNull();
            rule.Configuration.Enabled.Should().Be(true);
            rule.Configuration.DefaultLevel.Should().Be(RuleConfigurationDefaultLevel.Error);
            rule.HelpLocation.Should().NotBeNull();
            rule.HelpLocation.Uri.ToString().Should().Be("http://www.domain.com/rules/c2002.html");
            rule.MessageStrings.Should().BeNull();
            rule.Properties.Should().BeNull();

            rule = run.Resources.Rules["C2003"];

            rule.Id.Should().Be("C2003");
            rule.Name.Should().NotBeNull();
            rule.Name.Text.Should().Be("Rule C2003");
            rule.ShortDescription.Should().NotBeNull();
            rule.ShortDescription.Text.Should().Be("Rules were meant to be broken.");
            rule.FullDescription.Should().NotBeNull();
            rule.FullDescription.Text.Should().Be("Rent internal rebellion competence biography photograph.");
            rule.Configuration.Should().NotBeNull();
            rule.Configuration.Enabled.Should().Be(false);
            rule.Configuration.DefaultLevel.Should().Be(RuleConfigurationDefaultLevel.Note);
            rule.HelpLocation.Should().BeNull();
            rule.MessageStrings.Should().BeNull();
            rule.Properties.Should().BeNull();
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithBasicInvocation()
        {
            string v1LogText =
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsV2);
            string v2LogExpectedText =
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

            v2LogText.Should().Be(v2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithInvocationAndNotifications()
        {
            string v1LogText =
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
                            ""stack"": {
                              ""frames"": [
                                {
                                  ""message"": ""Exception thrown"",
                                  ""module"": ""RuleLibrary"",
                                  ""threadId"": 52,
                                  ""fullyQualifiedLogicalName"": ""Rules.SecureHashAlgorithmRule.Evaluate"",
                                  ""address"": 10092852
                                },
                                {
                                  ""module"": ""ExecutionEngine"",
                                  ""threadId"": 52,
                                  ""fullyQualifiedLogicalName"": ""ExecutionEngine.Engine.EvaluateRule"",
                                  ""address"": 10073356
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsV2);
            string v2LogExpectedText =
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
      ""results"": []
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_OneRunWithNotificationsButNoInvocations()
        {
            string v1LogText =
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
                      ""results"": [
                      ]
                    }
                  ]
                }";

            SarifLog v2Log = TransformVersionOneToCurrent(v1LogText);

            string v2LogText = JsonConvert.SerializeObject(v2Log, SarifTransformerUtilities.JsonSettingsV2);
            string v2LogExpectedText =
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

            v2LogText.Should().Be(v2LogExpectedText);
        }
    }
}
