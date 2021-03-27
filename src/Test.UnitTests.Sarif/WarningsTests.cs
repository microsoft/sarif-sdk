// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Moq;

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

        [Fact]
        public void Warnings_ObsoleteOption()
        {
            var testContext = new Mock<IAnalysisContext>();
            var testLogger = new Mock<IAnalysisLogger>();

            testLogger.Setup(x => x.LogConfigurationNotification(It.IsAny<Notification>()));
            testContext.SetupProperty(x => x.RuntimeErrors);
            testContext.Setup(x => x.Logger).Returns(testLogger.Object);

            Warnings.LogObsoleteOption(testContext.Object, "testObsoleteOption");
            testLogger.Verify(x => x.LogConfigurationNotification(It.IsAny<Notification>()), Times.Once);
            //  We don't verify the specifics of the output string

            Warnings.LogObsoleteOption(testContext.Object, "testObsoleteOption", "testReplacement");
            testLogger.Verify(x => x.LogConfigurationNotification(It.IsAny<Notification>()), Times.Exactly(2));
        }
    }
}
