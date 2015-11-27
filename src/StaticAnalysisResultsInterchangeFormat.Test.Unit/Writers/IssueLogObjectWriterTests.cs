// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.DataContracts;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Writers
{
    [TestClass]
    public class IssueLogObjectWriterTests
    {
        private static readonly RunInfo s_defaultRunInfo = new RunInfo();
        private static readonly ToolInfo s_defaultToolInfo = new ToolInfo();
        private static readonly Result s_defaultIssue = new Result();

        [TestMethod]
        public void IssueLogObjectWriter_DefaultIsEmpty()
        {
            var uut = new ResultsLogObjectWriter();
            Assert.IsNull(uut.ToolInfo);
            Assert.AreEqual(0, uut.IssueList.Count);
        }

        [TestMethod]
        public void IssueLogObjectWriter_AcceptsIssuesAndToolInfo()
        {
            var uut = new ResultsLogObjectWriter();
            uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
            uut.WriteResult(s_defaultIssue);

            Assert.AreEqual(s_defaultToolInfo, uut.ToolInfo);
            Assert.AreEqual(1, uut.IssueList.Count);
            Assert.AreEqual(s_defaultIssue, uut.IssueList[0]);
            Assert.AreEqual(s_defaultRunInfo, uut.RunInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IssueLogObjectWriter_RequiresToolInfoBeforeIssues()
        {
            new ResultsLogObjectWriter().WriteResult(s_defaultIssue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IssueLogObjectWriter_ToolInfoMayNotBeWrittenMoreThanOnce()
        {
            var uut = new ResultsLogObjectWriter();
            uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
            uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogObjectWriter_RequiresNonNullToolInfo()
        {
            new ResultsLogObjectWriter().WriteToolAndRunInfo(null, s_defaultRunInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogObjectWriter_RequiresNonNullRunInfo()
        {
            new ResultsLogObjectWriter().WriteToolAndRunInfo(s_defaultToolInfo, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IssueLogObjectWriter_RequiresNonNullIssue()
        {
            var uut = new ResultsLogObjectWriter();
            uut.WriteToolAndRunInfo(s_defaultToolInfo, s_defaultRunInfo);
            uut.WriteResult(null);
        }
    }
}
