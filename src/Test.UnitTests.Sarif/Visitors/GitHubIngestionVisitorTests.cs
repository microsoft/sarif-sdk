﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class GitHubIngestionVisitorTests : FileDiffingUnitTests
    {
        public GitHubIngestionVisitorTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        protected override string ConstructTestOutputFromInputResource(string inputResourceName, object parameter)
        {
            string logContents = GetResourceText(inputResourceName);
            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(logContents);

            var visitor = new GitHubIngestionVisitor();
            visitor.Visit(sarifLog);

            return JsonConvert.SerializeObject(sarifLog, SarifTransformerUtilities.JsonSettingsIndented);
        }

        [Fact]
        public void GitHubDspIngestionVisitor_FiltersNonErrorResults()
            => RunTest("NonErrorResults.sarif");

        [Fact]
        public void GitHubDspIngestionVisitor_LimitsNumberOfResults()
        {
            int prevMaxResults = GitHubIngestionVisitor.s_MaxResults;

            try
            {
                GitHubIngestionVisitor.s_MaxResults = 2;
                RunTest("TooManyResults.sarif");
            }
            finally
            {
                GitHubIngestionVisitor.s_MaxResults = prevMaxResults;
            }
        }

        [Fact]
        public void GitHubDspIngestionVisitor_RemovesArtifactsAndRetainsIndirectArtifactLocations()
            => RunTest("WithArtifacts.sarif");

        [Fact]
        public void GitHubDspIngestionVisitor_MovesFingerprintsToPartialFingerprints()
            => RunTest("Fingerprints.sarif");

        [Fact]
        public void GitHubDspIngestionVisitor_InlinesThreadFlowLocations()
            => RunTest("ThreadFlowLocations.sarif");
    }
}
