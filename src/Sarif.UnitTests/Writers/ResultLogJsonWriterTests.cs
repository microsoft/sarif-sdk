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
        private static readonly RunInfo s_defaultRunInfo = new RunInfo();
        private static readonly ToolInfo s_defaultToolInfo = new ToolInfo();
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
            Assert.AreEqual(String.Empty, GetJson(delegate { }));
        }

        [TestMethod]
        public void ResultLogJsonWriter_AcceptsIssuesAndToolInfo()
        {
            string expected = "{\"version\":\"1.0.0-beta.1\",\"runLogs\":[{\"toolInfo\":{\"name\":null},\"runInfo\":{},\"results\":[{}]}]}";
            string actual = GetJson(uut =>
            {
                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteRunInfo(s_defaultRunInfo);
                uut.WriteResult(s_defaultIssue);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_RequiresToolInfoBeforeIssues()
        {
            GetJson(uut => uut.WriteResult(s_defaultIssue));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_ToolInfoMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteToolInfo(s_defaultToolInfo);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_RunInfoMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteRunInfo(s_defaultRunInfo);
                uut.WriteRunInfo(s_defaultRunInfo);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullToolInfo()
        {
            GetJson(uut => uut.WriteToolInfo(null));
        }

        [TestMethod]
        public void ResultLogJsonWriter_NullRunInfoIsOK()
        {
            GetJson(uut => uut.WriteRunInfo(null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogJsonWriter_RequiresNonNullIssue()
        {
            GetJson(uut =>
            {
                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteRunInfo(s_defaultRunInfo);
                uut.WriteResult(null);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ResultLogJsonWriter_CannotWriteToolInfoToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.Dispose();
                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteRunInfo(s_defaultRunInfo);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ResultLogJsonWriter_CannotWriteIssuesToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultLogJsonWriter(json))
            {
                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteRunInfo(s_defaultRunInfo); uut.Dispose();
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
