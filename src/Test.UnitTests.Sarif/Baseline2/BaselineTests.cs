// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class BaselineTests
    {
        /// <summary>
        /// Issue present in scan 1 and absent in scans 2 and 3
        /// </summary>
        private const string FirstIssueID = "147643124";

        /// <summary>
        /// Issue present in scans 1 and 2 and absent in scan 3
        /// </summary>
        private const string SecondIssueID = "92983764820";

        /// <summary>
        /// Issue present in scans 1, 2, and 3
        /// </summary>
        private const string ThirdIssueID = "385068264";

        /// <summary>
        /// Issue present in scan 2 and absent in scans 1 and 3
        /// </summary>
        private const string FourthIssueID = "9287462834";

        /// <summary>
        /// Issue present in scan 3 and absent in scans 1 and 2
        /// </summary>
        private const string FifthIssueID = "5515126421";

        /// <summary>
        /// Issue absent in scan 2 and present in scans 1 and 3
        /// </summary>
        private const string SixthIssueID = "23994857";

        private static readonly IList<Result> s_matchedResults;

        static BaselineTests()
        {
            // Perform result matching on the test files (two "baseline" files and one "current" file).
            // This only needs to happen once (we can use the same result set in each of the tests),
            // so do it in the static ctor.
            IEnumerable<SarifLog> baselineLogs = new[] { ReadRun(0), ReadRun(1) };
            SarifLog currentLog = ReadRun(2);

            ISarifLogMatcher baseliner = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            IEnumerable<SarifLog> matchedLogs = baseliner.Match(baselineLogs, new[] { currentLog });

            // ISarifLogMatcher.Match returns a separate log for each tool. Since the test files
            // all contain a single run from the same tool, Match returns only one log, with one run.
            matchedLogs.Count().Should().Be(1);
            IList<Run> runs = matchedLogs.Single().Runs;

            runs.Count.Should().Be(1);
            s_matchedResults = runs.Single().Results;
        }

        private static readonly Assembly s_testAssembly = typeof(BaselineTests).Assembly;
        private const string ResourceStreamNameFormat =
            "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.TestData.Baseline.ToolRun-{0}.sarif";

        private static SarifLog ReadRun(int runNumber)
        {
            string resourceStreamName = string.Format(CultureInfo.InvariantCulture, ResourceStreamNameFormat, runNumber);

            using (Stream resourceStream = s_testAssembly.GetManifestResourceStream(resourceStreamName))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                string fileContent = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<SarifLog>(fileContent);
            }
        }

        [Theory]
        [InlineData(FirstIssueID, BaselineState.Absent)]
        [InlineData(SecondIssueID, BaselineState.Absent)]
        [InlineData(ThirdIssueID, BaselineState.Unchanged)]
        [InlineData(FourthIssueID, BaselineState.Absent)]
        [InlineData(FifthIssueID, BaselineState.New)]
        [InlineData(SixthIssueID, BaselineState.Unchanged)]
        public void MatchResultReturnsExpectedBaselineState(
            string issueId,
            BaselineState expectedBaselineState)
        {
            Result matchingResult = s_matchedResults.FirstOrDefault(result => result.Guid == issueId);

            matchingResult.BaselineState.Should().Be(expectedBaselineState);
        }
    }
}
