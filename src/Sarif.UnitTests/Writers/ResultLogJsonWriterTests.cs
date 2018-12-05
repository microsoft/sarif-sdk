// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ResultLogJsonWriterTests : JsonTests
    {
        [Fact]
        public void ResultLogJsonWriter_AcceptsResultAndTool()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {
          ""message"": {
            ""text"": ""Some testing occurred.""
          }
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteResult(DefaultResult);
            });

            actual.Should().BeCrossPlatformEquivalent(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_DoNotInitializeMoreThanOnce()
        {
            Assert.Throws<InvalidOperationException>(() => 
                GetJson(uut =>
                {
                    var run = new Run() { Tool = DefaultTool };
                    uut.Initialize(run);
                    uut.Initialize(run);
                })
            );
        }

        [Fact]
        public void ResultLogJsonWriter_ResultsMayNotBeWrittenMoreThanOnce()
        {
            var results = new[] { DefaultResult };

            Assert.Throws<InvalidOperationException>(() => 
                GetJson(uut =>
                {
                    var run = new Run() { Tool = DefaultTool };
                    uut.Initialize(run);

                    uut.OpenResults();
                    uut.WriteResults(results);
                    uut.CloseResults();

                    uut.OpenResults();
                    uut.WriteResults(results);
                    uut.CloseResults();
                })
            );
        }

        [Fact]
        public void ResultLogJsonWriter_RequiresNonNullTool()
        {
            Assert.Throws<ArgumentNullException>(() =>
            GetJson(uut =>
            {
                var run = new Run() { Tool = null };
                uut.Initialize(run);
            }));
       }

        [Fact]
        public void ResultLogJsonWriter_RequiresNonNullResult()
        {
            Assert.Throws<ArgumentNullException>(() => 
                GetJson(uut =>
                {
                    var run = new Run() { Tool = DefaultTool };
                    uut.Initialize(run);
                    uut.WriteResult(null);
                })
            );
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteToolToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.Dispose();
                Assert.Throws<InvalidOperationException>(() => uut.Initialize(new Run() { Tool = DefaultTool}));
            }
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteResultsToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.Dispose();
                Assert.Throws<InvalidOperationException>(() => uut.WriteResult(DefaultResult));
            }
        }

        [Fact]
        public void ResultLogJsonWriter_MultipleDisposeAllowed()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                // Assert no exception thrown
                uut.Dispose();
                uut.Dispose();
                uut.Dispose();
            }
        }

        public static readonly Invocation s_invocation = new Invocation
        {
            Machine = "MY_MACHINE",
            CommandLine = "/a /b c.dll"
        };

        [Fact]
        public void ResultLogJsonWriter_WritesInvocation()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""invocations"": [
        {
          ""commandLine"": ""/a /b c.dll"",
          ""machine"": ""MY_MACHINE""
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run()
                {
                    Tool = DefaultTool
                };
                uut.Initialize(run);
                uut.WriteInvocations(new[] { s_invocation });
            });

            actual.Should().BeCrossPlatformEquivalent(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_WritesAutomationDetails()
        {
            string instanceGuid = Guid.NewGuid().ToString();
            string automationLogicalId = Guid.NewGuid().ToString();
            string instanceId = automationLogicalId + "/" + instanceGuid;

            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""id"": {
        ""instanceId"": """ + instanceId + @""",
        ""instanceGuid"": """ + instanceGuid + @"""
      },
      ""tool"": {
        ""name"": null
      },
      ""invocations"": [
        {
          ""commandLine"": ""/a /b c.dll"",
          ""machine"": ""MY_MACHINE""
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run()
                {
                    Id = new RunAutomationDetails
                    {
                        InstanceGuid = instanceGuid,
                        InstanceId = automationLogicalId + "/" + instanceGuid
                    },
                    Tool = DefaultTool,
                };
                uut.Initialize(run);
                uut.WriteInvocations(new[] { s_invocation });
            });

            actual.Should().BeCrossPlatformEquivalent(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteInvocationTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                var run = new Run() { Tool = DefaultTool, Invocations = new[] { s_invocation } };
                uut.Initialize(run);
                Assert.Throws<InvalidOperationException>(() => uut.Initialize(run));
            }
        }

        private const string ShortDateFormat = "d";
        private static readonly Notification[] s_notifications = new[]
        {
            new Notification
            {
                Id = "NOT0001",
                RuleId = "TST0001",
                Level = NotificationLevel.Error,
                Message = new Message { Text = "This is a test" },
                PhysicalLocation = new PhysicalLocation
                {
                    FileLocation = new FileLocation
                    {
                        Uri = new Uri("file:///C:/src/a.cs")
                    },
                    Region = new Region
                    {
                        StartLine = 3,
                        StartColumn = 12
                    }
                },
                TimeUtc = DateTime.ParseExact("04/29/2016", ShortDateFormat, CultureInfo.InvariantCulture),
                Exception = new ExceptionData
                {
                    Kind = "System.AggregateException",
                    Message = "Bad thing".ToMessage(),
                    InnerExceptions = new[]
                    {
                        new ExceptionData
                        {
                            Kind = "System.ArgumentNullException",
                            Message = "x cannot be null".ToMessage(),
                            Stack = new Stack
                            {
                                Frames = new StackFrame[]
                                {
                                    new StackFrame
                                    {
                                        Module = "a.dll",
                                        Location = new Location
                                        {
                                            FullyQualifiedLogicalName = "N1.N2.C.M1",
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                FileLocation = new FileLocation
                                                {
                                                    Uri = new Uri("file:///C:/src/a.cs")
                                                },
                                                Region = new Region
                                                {
                                                    StartLine = 10
                                                }
                                            }
                                        }
                                    },

                                    new StackFrame
                                    {
                                        Module = "a.dll",
                                        Location = new Location
                                        {
                                            FullyQualifiedLogicalName = "N1.N2.C.M2",
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                FileLocation = new FileLocation
                                                {
                                                    Uri = new Uri("file:///C:/src/a.cs")
                                                },
                                                Region = new Region
                                                {
                                                    StartLine = 6
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        private const string SerializedNotification =
@"        {
          ""id"": ""NOT0001"",
          ""ruleId"": ""TST0001"",
          ""physicalLocation"": {
            ""fileLocation"": {
              ""uri"": ""file:///C:/src/a.cs""
            },
            ""region"": {
              ""startLine"": 3,
              ""startColumn"": 12
            }
          },
          ""message"": {
            ""text"": ""This is a test""
          },
          ""level"": ""error"",
          ""timeUtc"": ""2016-04-29T00:00:00.000Z"",
          ""exception"": {
            ""kind"": ""System.AggregateException"",
            ""message"": {
              ""text"": ""Bad thing""
            },
            ""innerExceptions"": [
              {
                ""kind"": ""System.ArgumentNullException"",
                ""message"": {
                  ""text"": ""x cannot be null""
                },
                ""stack"": {
                  ""frames"": [
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/a.cs""
                          },
                          ""region"": {
                            ""startLine"": 10
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""N1.N2.C.M1""
                      },
                      ""module"": ""a.dll""
                    },
                    {
                      ""location"": {
                        ""physicalLocation"": {
                          ""fileLocation"": {
                            ""uri"": ""file:///C:/src/a.cs""
                          },
                          ""region"": {
                            ""startLine"": 6
                          }
                        },
                        ""fullyQualifiedLogicalName"": ""N1.N2.C.M2""
                      },
                      ""module"": ""a.dll""
                    }
                  ]
                }
              }
            ]
          }
        }";

        [Fact]
        public void ResultLogJsonWriter_WritesConfigurationNotifications()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""configurationNotifications"": [
" + SerializedNotification + @"
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteConfigurationNotifications(s_notifications);
            });

            actual.Should().BeCrossPlatformEquivalent(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_WritesToolNotifications()
        {
            string expected =
@"{
  ""$schema"": """ + SarifUtilities.SarifSchemaUri + @""",
  ""version"": """ + SarifUtilities.SemanticVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""toolNotifications"": [
" + SerializedNotification + @"
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteToolNotifications(s_notifications);
            });

            actual.Should().BeCrossPlatformEquivalent(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteToolNotificationsTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteToolNotifications(s_notifications);
                Assert.Throws<InvalidOperationException>(() => uut.WriteToolNotifications(s_notifications));
            }
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteConfigurationNotificationsTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteConfigurationNotifications(s_notifications);
                Assert.Throws<InvalidOperationException>(() => uut.WriteConfigurationNotifications(s_notifications));
            }
        }
    }
}
