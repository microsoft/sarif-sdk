// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class ResultLogJsonWriterTests : JsonTests
    {
        [Fact]
        public void ResultLogJsonWriter_AcceptsResultAndTool()
        {
            string expected = CreateCurrentV2SarifLogText(resultCount: 1);

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteResult(DefaultResult);
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_AcceptsEmptyResultList()
        {
            string expected = CreateCurrentV2SarifLogText(resultCount: 0, null, true);

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);
                uut.WriteResults(new Result[0]);
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
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
            Result[] results = new[] { DefaultResult };

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
                Assert.Throws<InvalidOperationException>(() => uut.Initialize(new Run() { Tool = DefaultTool }));
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
            CommandLine = "/a /b c.dll",
            ExecutionSuccessful = true
        };

        [Fact]
        public void ResultLogJsonWriter_WritesInvocation()
        {
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 0,
                (log) =>
                {
                    log.Runs[0].Invocations = new List<Invocation> { s_invocation };
                });

            string actual = GetJson(uut =>
            {
                var run = new Run()
                {
                    Tool = DefaultTool
                };
                uut.Initialize(run);
                uut.WriteInvocations(new[] { s_invocation });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_WritesAutomationDetails()
        {
            string instanceGuid = Guid.NewGuid().ToString();
            string automationLogicalId = Guid.NewGuid().ToString();
            string instanceId = automationLogicalId + "/" + instanceGuid;

            string expected = CreateCurrentV2SarifLogText(
                resultCount: 0,
                (log) =>
                {
                    log.Runs[0].Invocations = new List<Invocation> { s_invocation };
                    log.Runs[0].AutomationDetails = new RunAutomationDetails
                    {
                        Guid = instanceGuid,
                        Id = instanceId
                    };
                });

            string actual = GetJson(uut =>
            {
                var run = new Run()
                {
                    AutomationDetails = new RunAutomationDetails
                    {
                        Guid = instanceGuid,
                        Id = automationLogicalId + "/" + instanceGuid
                    },
                    Tool = DefaultTool,
                };
                uut.Initialize(run);
                uut.WriteInvocations(new[] { s_invocation });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
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
                Descriptor = new ReportingDescriptorReference
                {
                    Id = "NOT0001",
                },
                AssociatedRule = new ReportingDescriptorReference
                {
                    Id = "TST0001",
                },
                Level = FailureLevel.Error,
                Message = new Message { Text = "This is a test" },
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri("file:///C:/src/a.cs")
                            },
                            Region = new Region
                            {
                                StartLine = 3,
                                StartColumn = 12
                            }
                        }
                    }
                },
                TimeUtc = DateTime.ParseExact("04/29/2016", ShortDateFormat, CultureInfo.InvariantCulture),
                Exception = new ExceptionData
                {
                    Kind = "System.AggregateException",
                    Message = "Bad thing",
                    InnerExceptions = new[]
                    {
                        new ExceptionData
                        {
                            Kind = "System.ArgumentNullException",
                            Message = "x cannot be null",
                            Stack = new Stack
                            {
                                Frames = new StackFrame[]
                                {
                                    new StackFrame
                                    {
                                        Module = "a.dll",
                                        Location = new Location
                                        {
                                            LogicalLocation = new LogicalLocation
                                            {
                                                FullyQualifiedName = "N1.N2.C.M1"
                                            },
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                ArtifactLocation = new ArtifactLocation
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
                                            LogicalLocation = new LogicalLocation
                                            {
                                                FullyQualifiedName = "N1.N2.C.M2"
                                            },
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                ArtifactLocation = new ArtifactLocation
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


        [Fact]
        public void ResultLogJsonWriter_WritesConfigurationNotifications()
        {
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 0,
                (log) =>
                {
                    log.Runs[0].Invocations = new List<Invocation>
                    {
                        new Invocation
                        {
                            ToolConfigurationNotifications = new List<Notification>(s_notifications),
                            ExecutionSuccessful = true
                        }
                    };
                });

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                var invocation = new Invocation
                {
                    ToolConfigurationNotifications = s_notifications,
                    ExecutionSuccessful = true
                };
                uut.WriteInvocations(new[] { invocation });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_WritesToolNotifications()
        {
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 0,
                (log) =>
                {
                    log.Runs[0].Invocations = new List<Invocation>
                    {
                        new Invocation
                        {
                            ToolExecutionNotifications = new List<Notification>(s_notifications),
                            ExecutionSuccessful = true
                        }
                    };
                });

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                var invocation = new Invocation
                {
                    ToolExecutionNotifications = s_notifications,
                    ExecutionSuccessful = true
                };
                uut.WriteInvocations(new[] { invocation });
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);
        }

        [Fact]
        public void ResultLogJsonWriter_CannotWriteToolTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                var run = new Run() { };
                uut.Initialize(run);
                uut.WriteTool(DefaultTool);
                Assert.Throws<InvalidOperationException>(() => uut.WriteTool(DefaultTool));
            }
        }
    }
}
