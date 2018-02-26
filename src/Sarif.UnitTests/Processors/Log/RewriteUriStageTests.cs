// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class RewriteUriStageTests
    {
        

        [Fact]
        public void RewriteUri_RewritesOnlyApplicableResults()
        {
            var RewriteUri = SarifLogProcessorFactory.GetActionStage(SarifLogAction.RebaseUri, new string[] { "SRCROOT", @"C:\src" });
            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        public void RewriteUri_RewritesMultipleRuns(int runCount)
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();
            Random random = new Random(randomSeed);
            SarifLog log = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runCount);

            var RewriteUri = SarifLogProcessorFactory.GetActionStage(SarifLogAction.RebaseUri, new string[] { "SRCROOT", @"C:\src" });

            IEnumerable<SarifLog> rewritten = RewriteUri.Act(new List<SarifLog>() { log }.AsEnumerable());

            rewritten.Should().HaveCount(1);

            SarifLog rewrittenLog = rewritten.Single();

            foreach(var run in rewrittenLog.Runs)
            {
                run.Properties.Should().ContainKey("");
            }

        }

        [Fact]
        public void RewriteUri_RewritesMultipleFiles()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void RewriteUri_EmptyFile_SucceedsWithoutChangingFile()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void RewriteUri_NoApplicableFiles_AddsVariableToRun()
        {
            throw new NotImplementedException();
        }

    }
}
