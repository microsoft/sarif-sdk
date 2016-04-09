// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [TestClass]
    public class ResultLogObjectWriterTests
    {
        private static readonly Run s_defaultRunInfo = new Run();
        private static readonly Tool s_defaultToolInfo = new Tool();
        private static readonly Result s_defaultIssue = new Result();

        [TestMethod]
        public void ResultLogObjectWriter_DefaultIsEmpty()
        {
            var uut = new ResultLogObjectWriter();
            Assert.IsNull(uut.Tool);
            Assert.AreEqual(0, uut.IssueList.Count);
        }

        [TestMethod]
        public void ResultLogObjectWriter_AcceptsIssuesAndToolInfo()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultToolInfo);
            uut.WriteRun(s_defaultRunInfo);
            uut.WriteResult(s_defaultIssue);

            Assert.AreEqual(s_defaultToolInfo, uut.Tool);
            Assert.AreEqual(1, uut.IssueList.Count);
            Assert.AreEqual(s_defaultIssue, uut.IssueList[0]);
            Assert.AreEqual(s_defaultRunInfo, uut.Run);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_RequiresToolInfoBeforeIssues()
        {
            new ResultLogObjectWriter().WriteResult(s_defaultIssue);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_toolMayNotBeWrittenMoreThanOnce()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultToolInfo);
            uut.WriteTool(s_defaultToolInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_runMayNotBeWrittenMoreThanOnce()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteRun(s_defaultRunInfo);
            uut.WriteRun(s_defaultRunInfo);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogObjectWriter_RequiresNonNullToolInfo()
        {
            new ResultLogObjectWriter().WriteTool(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogObjectWriter_RequiresNonNullRunInfo()
        {
            new ResultLogObjectWriter().WriteRun(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogObjectWriter_RequiresNonNullIssue()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultToolInfo);
            uut.WriteRun(s_defaultRunInfo);
            uut.WriteResult(null);
        }
    }
}
