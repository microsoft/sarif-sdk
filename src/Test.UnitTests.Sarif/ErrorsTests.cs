// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ErrorsTests
    {
        [Fact]
        public void Errors_NoPluginsConfigured()
        {
            var context = new TestAnalysisContext();
            var logger = new TestMessageLogger();
            context.Logger = logger;

            Errors.LogNoPluginsConfigured(context);

            logger.Messages.Should().BeNull();
            logger.ToolNotifications.Should().BeNull();
            logger.ConfigurationNotifications.Count.Should().Be(1);
            logger.ConfigurationNotifications[0].Descriptor.Id.Should().BeEquivalentTo(Errors.ERR997_NoPluginsConfigured);

            context.RuntimeErrors.Should().Be(RuntimeConditions.NoRulesLoaded);
        }
    }
}
