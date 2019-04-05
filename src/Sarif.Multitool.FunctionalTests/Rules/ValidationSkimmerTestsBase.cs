// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public abstract class ValidationSkimmerTestsBase<TSkimmer> : SarifMultitoolTestBase
        where TSkimmer : Skimmer<SarifValidationContext>, new()
    {
        private readonly string _testDirectory;
        private const string ExpectedResultsPropertyName = "expectedResults";

        public ValidationSkimmerTestsBase()
        {
            string ruleName = typeof(TSkimmer).Name;
            _testDirectory = Path.Combine(Environment.CurrentDirectory, TestDataDirectory, ruleName);
        }

        protected void Verify(string testFileName, bool disablePrereleaseCompatibilityTransform = false)
        {
            //Verify(testFileName, disablePrereleaseCompatibilityTransform: false);

            string targetPath = Path.Combine(_testDirectory, testFileName);
            string actualFilePath = MakeActualFilePath(_testDirectory, testFileName);

            ValidateCommandTests.Verify(targetPath, ValidationCallback, disablePrereleaseCompatibilityTransform: false);
        }

        private void ValidationCallback(string testFileName, string actualLogContents)
        {
            string targetPath = Path.Combine(_testDirectory, testFileName);
            string inputLogContents = File.ReadAllText(targetPath);

            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents);

            bool expectedResultsArePresent = inputLog.Runs[0].TryGetProperty(ExpectedResultsPropertyName, out ExpectedValidationResults expectedResults);
            expectedResultsArePresent.Should().Be(true);

            SarifLog outputLog = JsonConvert.DeserializeObject<SarifLog>(actualLogContents);

            Verify(outputLog.Runs[0], expectedResults);
        }

        // Every validation message begins with a placeholder "{0}: " that specifies the
        // result location, for example, "runs[0].results[0].locations[0].physicalLocation".
        // Verify that those detected result locations match the expected locations.
        private void Verify(Run run, ExpectedValidationResults expectedResults)
        {
            HashSet<string> actualResultLocations = new HashSet<string>(run.Results.Select(r => r.Message.Arguments[0]));

            IEnumerable<string> unexpectedNewResultLocations = actualResultLocations.Except(expectedResults.ResultLocationPointers);
            unexpectedNewResultLocations.Count().Should().Be(0);

            IEnumerable<string> unexpectedlyAbsentResultLocations = expectedResults.ResultLocationPointers.Except(actualResultLocations);
            unexpectedlyAbsentResultLocations.Count().Should().Be(0);
        }
    }
}