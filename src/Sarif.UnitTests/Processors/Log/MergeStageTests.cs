// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class MergeStageTests
    {
        GenericFoldAction<SarifLog> Merge = (GenericFoldAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge);
        
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        public void MergeStage_SingleFile_ReturnedUnchanged(int runs)
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);
            SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runs);

            SarifLog processed = Merge.Fold(new List<SarifLog>() { log });

            processed.ShouldBeEquivalentTo(log, $"Merge should not change the contents of a single log file.  Seed: {randomSeed}");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(10)]
        public void MergeStage_MultipleFiles_MergeCorrectly(int fileCount)
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails by forcibly setting
            // the randomSeed to a particular value.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);

            List<SarifLog> logs = new List<SarifLog>();
            for (int i = 0; i < fileCount; i++)
            {
                logs.Add(RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, random.Next(5)));
            }

            SarifLog merged = Merge.Fold(logs);
            
            foreach(var log in logs)
            {
                if (log.Runs != null)
                {
                    log.Runs.Should().BeSubsetOf(merged.Runs, $"Merged log should contain all contents of files to merge.  Seed: {randomSeed}");
                }
            }
        }
    }
}
