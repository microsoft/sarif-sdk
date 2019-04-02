// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class WarningsTests
    {
        [Fact]
        public void Warnings_LogRuleWasExplicitlyDisabled()
        {
            var testLogger = new TestMessageLogger();
            var binaryAnalysisContext = new TestAnalysisContext();
            binaryAnalysisContext.Logger = testLogger;

            string ruleId = nameof(ruleId);

            Warnings.LogRuleExplicitlyDisabled(binaryAnalysisContext, ruleId);

            testLogger.Messages.Should().BeNull();
            testLogger.ToolNotifications.Should().BeNull();
            testLogger.ConfigurationNotifications.Count.Should().Equals(1);
            testLogger.ConfigurationNotifications[0].Descriptor.Id.Should().Equals(ruleId);
        }
    }
}
