// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [TestClass]
    public class ResultLogJsonWriterTests : JsonTests
    {
        private static readonly string SchemaVersion =
            SarifUtilities.ConvertToText(SarifVersion.OneZeroZeroBetaFour);

        private static readonly Tool s_defaultTool = new Tool();
        private static readonly Result s_defaultResult = new Result();

        private static string GetJson(Action<ResultLogJsonWriter> testContent)
        {
            StringBuilder result = new StringBuilder();
            using (var str = new StringWriter(result))
            using (var json = new JsonTextWriter(str) { Formatting = Formatting.Indented })
            using (var uut = new ResultLogJsonWriter(json))
            {
                testContent(uut);
            }

            return result.ToString();
        }

        [TestMethod]
        public void ResultLogJsonWriter_DefaultIsEmpty()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": """ + SchemaVersion + @""",
  ""runs"": [
    {}
  ]
}";
            Assert.AreEqual(expected, GetJson(delegate { }));
        }

        [TestMethod]
        public void ResultLogJsonWriter_AcceptsResultAndTool()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": """ + SchemaVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""results"": [
        {}
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteResult(s_defaultResult);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_ToolMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteTool(s_defaultTool);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_ResultsMayNotBeWrittenMoreThanOnce()
        {
            var results = new[] { s_defaultResult };

            GetJson(uut =>
            {
                uut.OpenResults();
                uut.WriteResults(results);
                uut.CloseResults();

                uut.OpenResults();
                uut.WriteResults(results);
                uut.CloseResults();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullTool()
        {
            GetJson(uut => uut.WriteTool(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullResult()
        {
            GetJson(uut =>
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteResult(null);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteToolToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.Dispose();
                uut.WriteTool(s_defaultTool);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteResultsToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteTool(s_defaultTool);
                uut.Dispose();
                uut.WriteResult(s_defaultResult);
            }
        }

        [TestMethod]
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

        [TestMethod]
        public void ResultLogJsonWriter_WritesInvocation()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": """ + SchemaVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""invocation"": {
        ""commandLine"": ""/a /b c.dll"",
        ""machine"": ""MY_MACHINE""
      }
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteInvocation(s_invocation);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteInvocationTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteInvocation(s_invocation);
                uut.WriteInvocation(s_invocation);
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
                Message = "This is a test",
                AnalysisTarget = new PhysicalLocation
                {
                    Uri = new Uri("file:///C:/src/a.cs"),
                    Region = new Region
                    {
                        StartLine = 3,
                        StartColumn = 12
                    }
                },
                Time = DateTime.ParseExact("04/29/2016", ShortDateFormat, CultureInfo.InvariantCulture),
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
                                        FullyQualifiedLogicalName = "N1.N2.C.M1",
                                        Line = 10
                                    },

                                    new StackFrame
                                    {
                                        Module = "a.dll",
                                        FullyQualifiedLogicalName = "N1.N2.C.M2",
                                        Line = 6
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
          ""analysisTarget"": {
            ""uri"": ""file:///C:/src/a.cs"",
            ""region"": {
              ""startLine"": 3,
              ""startColumn"": 12
            }
          },
          ""message"": ""This is a test"",
          ""level"": ""error"",
          ""time"": ""2016-04-29T00:00:00.00Z"",
          ""exception"": {
            ""kind"": ""System.AggregateException"",
            ""message"": ""Bad thing"",
            ""innerExceptions"": [
              {
                ""kind"": ""System.ArgumentNullException"",
                ""message"": ""x cannot be null"",
                ""stack"": {
                  ""frames"": [
                    {
                      ""line"": 10,
                      ""module"": ""a.dll"",
                      ""fullyQualifiedLogicalName"": ""N1.N2.C.M1""
                    },
                    {
                      ""line"": 6,
                      ""module"": ""a.dll"",
                      ""fullyQualifiedLogicalName"": ""N1.N2.C.M2""
                    }
                  ]
                }
              }
            ]
          }
        }";

        [TestMethod]
        public void ResultLogJsonWriter_WritesConfigurationNotifications()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": """ + SchemaVersion + @""",
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
                uut.WriteTool(s_defaultTool);
                uut.WriteConfigurationNotifications(s_notifications);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ResultLogJsonWriter_WritesToolNotifications()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": """ + SchemaVersion + @""",
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
                uut.WriteTool(s_defaultTool);
                uut.WriteToolNotifications(s_notifications);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteToolNotificationsTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteToolNotifications(s_notifications);
                uut.WriteToolNotifications(s_notifications);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteConfigurationNotificationsTwice()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteConfigurationNotifications(s_notifications);
                uut.WriteConfigurationNotifications(s_notifications);
            }
        }
    }
}
