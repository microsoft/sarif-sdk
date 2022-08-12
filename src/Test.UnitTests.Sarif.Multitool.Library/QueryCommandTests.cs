// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using Microsoft.CodeAnalysis.Sarif.Query;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class QueryCommandTests
    {
        private static readonly TestAssetResourceExtractor s_extractor = new TestAssetResourceExtractor(typeof(QueryCommandTests));

        [Fact]
        public void QueryCommand_Basics()
        {
            string filePath = "elfie-arriba.sarif";
            File.WriteAllText(filePath, s_extractor.GetResourceText(filePath));

            // All Results: No filter
            RunAndVerifyCount(5, new QueryOptions() { Expression = "", InputFilePath = filePath });

            // Rule filtering
            RunAndVerifyCount(1, new QueryOptions() { Expression = "RuleId = 'CSCAN0020/0'", InputFilePath = filePath });
            RunAndVerifyCount(4, new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", InputFilePath = filePath });

            // Level filtering
            RunAndVerifyCount(1, new QueryOptions() { Expression = "Level != Error", InputFilePath = filePath });
            RunAndVerifyCount(1, new QueryOptions() { Expression = "Level != Error && RuleId = CSCAN0060/0", InputFilePath = filePath });

            // Suppression filtering
            RunAndVerifyCount(1, new QueryOptions() { Expression = "IsSuppressed == True", InputFilePath = filePath });
            RunAndVerifyCount(1, new QueryOptions() { Expression = "IsSuppressed == True && RuleId = CSCAN0060/0", InputFilePath = filePath });

            // Intersection w/no matches
            RunAndVerifyCount(0, new QueryOptions() { Expression = "Level != Error && RuleId != CSCAN0060/0", InputFilePath = filePath });

            // Verify parsing errors (unknown enum, operator, column name)
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Level != UnknownValue", InputFilePath = filePath }));
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Level ** Error", InputFilePath = filePath }));
            Assert.Throws<QueryParseException>(() => RunAndVerifyCount(0, new QueryOptions() { Expression = "Leveler != Error", InputFilePath = filePath }));

            // Verify threshold logging
            Assert.Equal(0, new QueryCommand().RunWithoutCatch(new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", NonZeroExitCodeIfCountOver = 4, InputFilePath = filePath }));
            Assert.Equal(2, new QueryCommand().RunWithoutCatch(new QueryOptions() { Expression = "RuleId = 'CSCAN0060/0'", NonZeroExitCodeIfCountOver = 3, InputFilePath = filePath }));

            // Verify output file
            string outputFilePath = "elfie-arriba.CSCAN0020.actual.sarif";
            RunAndVerifyCount(1, new QueryOptions() { Expression = "RuleId = 'CSCAN0020/0'", InputFilePath = filePath, OutputFilePath = outputFilePath, Minify = false, Force = true });

            string expected = s_extractor.GetResourceText("elfie-arriba.CSCAN0020.sarif");
            string actual = File.ReadAllText(outputFilePath);
            Assert.Equal(expected, actual);
        }

        private void RunAndVerifyCount(int expectedCount, QueryOptions options)
        {
            options.ReturnCount = true;
            int exitCode = new QueryCommand().RunWithoutCatch(options);
            Assert.Equal(expectedCount, exitCode);
        }
    }
}
