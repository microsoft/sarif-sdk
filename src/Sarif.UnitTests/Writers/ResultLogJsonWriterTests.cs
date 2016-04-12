// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    [TestClass]
    public class ResultLogJsonWriterTests
    {
        private static readonly Tool s_defaultTool = new Tool();
        private static readonly Result s_defaultResult = new Result();

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
            string expected = @"{""version"":""1.0.0-beta.3"",""runs"":[{}]}";
            Assert.AreEqual(expected, GetJson(delegate { }));
        }

        [TestMethod]
        public void ResultLogJsonWriter_AcceptsResultAndTool()
        {
            string expected = "{\"version\":\"1.0.0-beta.3\",\"runs\":[{\"tool\":{\"name\":null},\"results\":[{}]}]}";
            string actual = GetJson(uut =>
            {
                uut.WriteTool(s_defaultTool);
                uut.WriteResult(s_defaultResult);
            });
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResultLogJsonWriter_toolMayNotBeWrittenMoreThanOnce()
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
    }
}
