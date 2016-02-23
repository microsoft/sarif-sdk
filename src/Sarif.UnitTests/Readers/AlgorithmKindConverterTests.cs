// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Sdk;
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
            string expected = "{\"version\":\"1.0.0-beta.1\",\"runLogs\":[{\"toolInfo\":{\"name\":null},\"runInfo\":{\"analysisTargets\":[{\"uri\":null,\"hashes\":[{\"value\":null,\"algorithm\":\"Groestl\"}]}]},\"results\":[{\"ruleId\":null,\"locations\":null}]}]}";
            string actual = GetJson(uut =>
            {
                var runInfo = new RunInfo();

                runInfo.AnalysisTargets = new[] {
                    new FileReference()
                    {
                         Hashes = new[]
                         {
                             new Hash()
                             {
                                Algorithm = AlgorithmKind.Groestl
                             },
                         }
                    }
                };

                uut.WriteToolAndRunInfo(s_defaultToolInfo, runInfo);
                uut.WriteResult(s_defaultResult);
            });
            Assert.AreEqual(expected, actual);
        }
    }
}
