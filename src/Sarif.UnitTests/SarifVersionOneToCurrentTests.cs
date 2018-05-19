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
                      ""results"": []
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
      ""results"": []
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
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

            v2LogText.Should().Be(v2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_MinimumWithPropertyAndTags()
        {
            string v1LogText =
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
        ""semanticVersion"": ""2.1.0"",
        ""properties"": {
          ""foo"": ""bar"",
          ""tags"": [
  ""1"",
  ""2""
]
        }
      },
      ""results"": []
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
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
                      ""results"": []
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
      ""results"": []
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
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
      ""results"": []
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
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
                      ""results"": []
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
      }
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
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
                      ""results"": []
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
                      ""results"": []
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

        [Fact]
        public void SarifTransformerTests_ToCurrent_NotificationExceptionWithStack()
        {
            string v1LogText =
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

            v2LogText.Should().Be(v2LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToCurrent_BasicResult()
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
                                  ""endColumn"": 1
                                }
                              },
                              ""fullyQualifiedLogicalName"": ""collections::list::add"",
                              ""decoratedName"": ""?add@list@collections@@QAEXH@Z""
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
          ""suppressionStates"": [""suppressedExternally""],
          ""baselineState"": ""existing"",
          ""properties"": {
            ""sarifv1/formattedRuleMessage"": {""formatId"":""default"",""arguments"":[""ptr""]}
          }
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
      }
    }
  ]
}";

            v2LogText.Should().Be(v2LogExpectedText);
        }
    }
}
