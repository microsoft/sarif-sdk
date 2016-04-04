// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    [TestClass]

    public class AlgorithmKindConverterTests
    {
        private static readonly RunInfo s_defaultRunInfo = new RunInfo();
        private static readonly ToolInfo s_defaultToolInfo = new ToolInfo();
        private static readonly Result s_defaultResult = new Result();

        public AlgorithmKindConverterTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
        }

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
        public void AlgorithmKindGroestl()
        {
            string expected = "{\"version\":\"1.0.0-beta.2\",\"runLogs\":[{\"toolInfo\":{\"name\":null},\"runInfo\":{\"fileInfo\":{\"http://abc\":[{\"uri\":\"http://abc\",\"hashes\":[{\"value\":null,\"algorithm\":\"Groestl\"}]}]}},\"results\":[{}]}]}";
            string actual = GetJson(uut =>
            {
                var runInfo = new RunInfo();

                runInfo.FileInfo = new Dictionary<string, IList<FileReference>> {
                    ["http://abc"] = new List<FileReference>
                    {
                        new FileReference()
                        {
                            Uri = new Uri("http://abc"),
                            Hashes = new[]
                            {
                                new Hash()
                                {
                                   Algorithm = AlgorithmKind.Groestl
                                }
                            }
                        }
                    }
                };

                uut.WriteToolInfo(s_defaultToolInfo);
                uut.WriteRunInfo(runInfo);
                uut.WriteResult(s_defaultResult);
            });
            Assert.AreEqual(expected, actual);
        }
    }
}
