// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class MergeStageTests
    {
        private readonly ITestOutputHelper output;

        public MergeStageTests(ITestOutputHelper outputHelper)
        {
            output = outputHelper;
        }

        readonly GenericFoldAction<SarifLog> Merge = (GenericFoldAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge);

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        public void MergeStage_SingleFile_ReturnedUnchanged(int runs)
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runs);

            SarifLog processed = Merge.Fold(new List<SarifLog>() { log });

            processed.Should().BeEquivalentTo(log);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        public void MergeStage_MultipleFiles_MergeCorrectly(int fileCount)
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            List<SarifLog> logs = new List<SarifLog>();
            for (int i = 0; i < fileCount; i++)
            {
                logs.Add(RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(5)));
            }

            SarifLog merged = Merge.Fold(logs);

            foreach (SarifLog log in logs)
            {
                if (log.Runs != null)
                {
                    log.Runs.Should().BeSubsetOf(merged.Runs);
                }
            }
        }
    }
}
