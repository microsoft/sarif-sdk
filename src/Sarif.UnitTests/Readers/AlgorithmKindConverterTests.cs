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
        private static readonly Run s_defaultRun = new Run();
        private static readonly Tool s_defaultTool = new Tool();
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
            string expected = "{\"version\":\"1.0.0-beta.4\",\"runs\":[{\"tool\":{\"name\":null},\"files\":{\"http://abc/\":[{\"hashes\":[{\"value\":null,\"algorithm\":\"Groestl\"}]}]},\"results\":[{}]}]}";
            string actual = GetJson(uut =>
            {
                var run = new Run();

                uut.WriteTool(s_defaultTool);

                var files = new Dictionary<string, IList<FileData>> {
                    ["http://abc/"] = new List<FileData>
                    {
                        new FileData()
                        {
                            Hashes = new List<Hash>
                            {
                                new Hash()
                                {
                                   Algorithm = AlgorithmKind.Groestl
                                }
                            }
                        }
                    }
                };

                uut.WriteFiles(files);

                uut.WriteResults(new[] { s_defaultResult });
            });
            Assert.AreEqual(expected, actual);
        }
    }
}
