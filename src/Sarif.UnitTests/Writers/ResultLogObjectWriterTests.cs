// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [TestClass]
    public class ResultLogObjectWriterTests
    {
        private static readonly Tool s_defaultTool = new Tool();
        private static readonly Result s_defaultResult = new Result();

        [TestMethod]
        public void ResultLogObjectWriter_DefaultIsEmpty()
        {
            var uut = new ResultLogObjectWriter();
            Assert.IsNull(uut.Tool);
            Assert.AreEqual(0, uut.ResultList.Count);
        }

        [TestMethod]
        public void ResultLogObjectWriter_AcceptsIssuesAndTool()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultTool);
            uut.WriteResult(s_defaultResult);

            Assert.AreEqual(s_defaultTool, uut.Tool);
            Assert.AreEqual(1, uut.ResultList.Count);
            Assert.AreEqual(s_defaultResult, uut.ResultList[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_RequiresToolBeforeIssues()
        {
            new ResultLogObjectWriter().WriteResult(s_defaultResult);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_ToolMayNotBeWrittenMoreThanOnce()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultTool);
            uut.WriteTool(s_defaultTool);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogObjectWriter_ResultsMayNotBeWrittenMoreThanOnce()
        {
            var uut = new ResultLogObjectWriter();
            var results = new[] { s_defaultResult };

            uut.OpenResults();
            uut.WriteResults(results);
            uut.CloseResults();

            uut.OpenResults();
            uut.WriteResults(results);
            uut.CloseResults();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogObjectWriter_RequiresNonNullTool()
        {
            new ResultLogObjectWriter().WriteTool(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResultLogObjectWriter_RequiresNonNullResult()
        {
            var uut = new ResultLogObjectWriter();
            uut.WriteTool(s_defaultTool);
            uut.WriteResult(null);
        }
    }
}
