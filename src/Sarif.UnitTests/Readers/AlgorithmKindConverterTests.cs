// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    [TestClass]
    public class AlgorithmKindConverterTests : JsonTests
    {
        private static readonly Run s_defaultRun = new Run();
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
        public void AlgorithmKind_AllMembers()
        {
            var testTuples = new List<Tuple<AlgorithmKind, string>>
            {
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Authentihash, "authentihash"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Blake256, "blake256"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Blake512, "blake512"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Ecoh, "ecoh"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Fsb, "fsb"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Gost, "gost"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Groestl, "groestl"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Has160, "has160"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Haval, "haval"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.JH, "jh"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.MD2, "md2"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.MD4, "md4"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.MD5, "md5"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.MD6, "md6"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.RadioGatun, "radioGatun"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.RipeMD, "ripeMD"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.RipeMD128, "ripeMD128"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.RipeMD160, "ripeMD160"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.RipeMD320, "ripeMD320"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sdhash, "sdhash"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha1, "sha1"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha224, "sha224"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha256, "sha256"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha3, "sha3"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha384, "sha384"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Sha512, "sha512"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Skein, "skein"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Snefru, "snefru"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.SpectralHash, "spectralHash"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Ssdeep, "ssdeep"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Swifft, "swifft"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Tiger, "tiger"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Tlsh, "tlsh"),
                new Tuple<AlgorithmKind, string>(AlgorithmKind.Whirlpool, "whirlpool"),
            };

            // Algorithm.Unknown is handled specially in another test
            Assert.AreEqual(Enum.GetValues(typeof(AlgorithmKind)).Length - 1, testTuples.Count);

            foreach (var testTuple in testTuples)
            {
                string expected = 
                    "{\"$schema\":\"http://json.schemastore.org/sarif-1.0.0\",\"version\":\"1.0.0-beta.4\",\"runs\":[{\"tool\":{\"name\":null},\"files\":{\"http://abc/\":[{\"hashes\":[{\"value\":null,\"algorithm\":\"" +
                    testTuple.Item2 +  
                    "\"}]}]},\"results\":[{}]}]}";

                string actual = GetJson(uut =>
                {
                    var run = new Run();

                    uut.WriteTool(s_defaultTool);

                    var files = new Dictionary<string, IList<FileData>>
                    {
                        ["http://abc/"] = new List<FileData>
                        {
                        new FileData()
                        {
                            Hashes = new List<Hash>
                            {
                                new Hash()
                                {
                                   Algorithm = testTuple.Item1
                                }
                            }
                        }
                        }
                    };

                    uut.WriteFiles(files);

                    uut.WriteResults(new[] { s_defaultResult });
                });
                Assert.AreEqual(expected, actual);

                var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
                Assert.AreEqual(testTuple.Item1, sarifLog.Runs[0].Files.Values.First()[0].Hashes[0].Algorithm);
            }
        }
    }
}
