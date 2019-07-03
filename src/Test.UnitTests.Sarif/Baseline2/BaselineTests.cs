// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

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

        private static readonly Assembly s_testAssembly = typeof(BaselineTests).Assembly;
        private const string ResourceStreamNameFormat =
            "Microsoft.CodeAnalysis.Test.UnitTests.Sarif.TestData.Baseline.ToolRun-{0}.sarif";

        [Theory]
        [InlineData(FirstIssueID, BaselineState.Absent)]
        [InlineData(SecondIssueID, BaselineState.Absent)]
        [InlineData(ThirdIssueID, BaselineState.Unchanged)]
        [InlineData(FourthIssueID, BaselineState.Absent)]
        [InlineData(FifthIssueID, BaselineState.New)]
        [InlineData(SixthIssueID, BaselineState.Unchanged)]
        public void MatchResultReturnsExpectedBaselineState(
            string issueID,
            BaselineState expectedBaselineState)
        {
            IEnumerable<SarifLog> baselineLogs = ReadBaselineLogs();
            SarifLog currentLog = ReadCurrentLog();

            IEnumerable<SarifLog> matchedLog = MatchResults(baselineLogs, currentLog);
            Result matchingResult = matchedLog.Select(r => r.Runs.FirstOrDefault().Results.FirstOrDefault(result => result.Guid == issueID)).FirstOrDefault();

            matchingResult.BaselineState.Should().Be(expectedBaselineState);
        }

        private static IEnumerable<SarifLog> ReadBaselineLogs()
            => new[] { ReadRun(0), ReadRun(1) };

        private static SarifLog ReadCurrentLog()
            => ReadRun(2);

        private IEnumerable<SarifLog> MatchResults(IEnumerable<SarifLog> baselineLogs, SarifLog currentLog)
        {
            ISarifLogMatcher baseliner = ResultMatchingBaselinerFactory.GetDefaultResultMatchingBaseliner();
            return baseliner.Match(baselineLogs, new[] { currentLog });
        }

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
    }
}
