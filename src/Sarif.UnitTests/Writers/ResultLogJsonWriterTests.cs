// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [TestClass]
    public class ResultLogJsonWriterTests
    {
        private static readonly Run s_defaultRunInfo = new Run();
        private static readonly Tool s_defaultToolInfo = new Tool();
        private static readonly Result s_defaultIssue = new Result();

        private static string GetJson(Action<ResultLogJsonWriter> testContent)
        {
            StringBuilder result = new StringBuilder();
            using (var str = new StringWriter(result))
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                testContent(uut);
            }

            return result.ToString();
        }

        [TestMethod]
        public void ResultLogJsonWriter_DefaultIsEmpty()
        {
            string expected = @"{""version"":""1.0.0-beta.2"",""runLogs"":[{}]}";
            Assert.AreEqual(expected, GetJson(delegate { }));
        }

        [TestMethod]
        public void ResultLogJsonWriter_AcceptsIssuesAndToolInfo()
        {
            string expected = "{\"version\":\"1.0.0-beta.2\",\"runLogs\":[{\"tool\":{\"name\":null},\"run\":{},\"results\":[{}]}]}";
            string actual = GetJson(uut =>
            {
                uut.WriteTool(s_defaultToolInfo);
                uut.WriteRun(s_defaultRunInfo);
                uut.WriteResult(s_defaultIssue);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_toolMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteTool(s_defaultToolInfo);
                uut.WriteTool(s_defaultToolInfo);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_runMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteRun(s_defaultRunInfo);
                uut.WriteRun(s_defaultRunInfo);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullToolInfo()
        {
            GetJson(uut => uut.WriteTool(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullRunInfo()
        {
            GetJson(uut => uut.WriteRun(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullIssue()
        {
            GetJson(uut =>
            {
                uut.WriteTool(s_defaultToolInfo);
                uut.WriteRun(s_defaultRunInfo);
                uut.WriteResult(null);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteToolInfoToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.Dispose();
                uut.WriteTool(s_defaultToolInfo);
                uut.WriteRun(s_defaultRunInfo);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_CannotWriteIssuesToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteTool(s_defaultToolInfo);
                uut.WriteRun(s_defaultRunInfo); uut.Dispose();
                uut.WriteResult(s_defaultIssue);
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
    }
}
