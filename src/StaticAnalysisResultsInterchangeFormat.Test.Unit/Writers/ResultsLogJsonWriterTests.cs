// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers
{
    [TestClass]
    public class ResultsLogJsonWriterTests
    {
        private static readonly RunInfo s_defaultRunInfo = new RunInfo();
        private static readonly ToolInfo s_defaultToolInfo = new ToolInfo();
        private static readonly Result s_defaultIssue = new Result();

        private static string GetJson(Action<ResultsLogJsonWriter> testContent)
        {
            StringBuilder result = new StringBuilder();
            using (var str = new StringWriter(result))
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultsLogJsonWriter(json))
            {
                testContent(uut);
            }

            return result.ToString();
        }

        [TestMethod]
        public void IssueLogJsonWriter_DefaultIsEmpty()
        {
            Assert.AreEqual(String.Empty, GetJson(delegate { }));
        }

        [TestMethod]
        public void IssueLogJsonWriter_AcceptsIssuesAndToolInfo()
        {
            string expected = "{\"version\":\"1.0\",\"toolInfo\":{\"toolName\":null},\"issues\":[{\"locations\":null,\"fullMessage\":null}]}";
            string actual = GetJson(uut =>
            {
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
                uut.WriteResult(s_defaultIssue);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IssueLogJsonWriter_RequiresToolInfoBeforeIssues()
        {
            GetJson(uut => uut.WriteResult(s_defaultIssue));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IssueLogJsonWriter_ToolInfoMayNotBeWrittenMoreThanOnce()
        {
            GetJson(uut =>
            {
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogJsonWriter_RequiresNonNullToolInfo()
        {
            GetJson(uut => uut.WriteToolAndRunInfo(null, s_defaultRunInfo));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogJsonWriter_RequiresNonNullRunInfo()
        {
            GetJson(uut => uut.WriteToolAndRunInfo(s_defaultToolInfo, null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogJsonWriter_RequiresNonNullIssue()
        {
            GetJson(uut =>
            {
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
                uut.WriteResult(null);
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void IssueLogJsonWriter_CannotWriteToolInfoToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultsLogJsonWriter(json))
            {
                uut.Dispose();
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void IssueLogJsonWriter_CannotWriteIssuesToDisposedWriter()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultsLogJsonWriter(json))
            {
                uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
                uut.Dispose();
                uut.WriteResult(s_defaultIssue);
            }
        }

        [TestMethod]
        public void IssueLogJsonWriter_MultipleDisposeAllowed()
        {
            using (var str = new StringWriter())
            using (var json = new JsonTextWriter(str))
            using (var uut = new ResultsLogJsonWriter(json))
            {
                // Assert no exception thrown
                uut.Dispose();
                uut.Dispose();
                uut.Dispose();
            }
        }
    }
}
