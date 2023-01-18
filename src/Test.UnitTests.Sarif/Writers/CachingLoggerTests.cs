// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class CachingLoggerTests
    {
        private static readonly Random Random = new Random();

        [Fact]
        public void CachingLogger_EmitNotificationsCorrectly()
        {
            string messageGuid = Guid.NewGuid().ToString();

            var notification = new Notification
            {
                Message = new Message
                {
                    Text = messageGuid
                }
            };

            TestAnalyzeOptions testAnalyzeOptions = new TestAnalyzeOptions();

            var logger = new CachingLogger(testAnalyzeOptions.Level, testAnalyzeOptions.Kind);
            logger.LogConfigurationNotification(notification);
            logger.ConfigurationNotifications.Should().HaveCount(1);

            logger.LogToolNotification(notification, associatedRule: null);
            logger.ToolNotifications.Should().HaveCount(1);
        }

        [Fact]
        public void CachingLogger_EmitResultsCorrectlyBasedOnRules()
        {
            Result result01 = GenerateResult();
            ReportingDescriptor rule01 = GenerateRule();

            TestAnalyzeOptions testAnalyzeOptions = new TestAnalyzeOptions();

            var logger = new CachingLogger(testAnalyzeOptions.Level, testAnalyzeOptions.Kind);

            Assert.Throws<ArgumentNullException>(() => logger.Log(null, result01));
            Assert.Throws<ArgumentNullException>(() => logger.Log(rule01, null));

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0002";

            Assert.Throws<ArgumentException>(() => logger.Log(rule01, result01));

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0001";

            // Validate simple insert
            logger.Log(rule01, result01);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);

            // Updating value from a specific key
            logger.Log(rule01, result01);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);
            logger.Results[rule01].Should().HaveCount(2);
        }

        [Fact]
        public void CachingLogger_ShouldEmitCorrectlyWhenResultContainsSubId()
        {
            Result result01 = GenerateResult();
            ReportingDescriptor rule01 = GenerateRule();

            TestAnalyzeOptions testAnalyzeOptions = new TestAnalyzeOptions();

            var logger = new CachingLogger(testAnalyzeOptions.Level, testAnalyzeOptions.Kind);

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0001/001";

            // Validate simple insert
            logger.Log(rule01, result01);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);

            // Updating value from a specific key
            logger.Log(rule01, result01);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);
            logger.Results[rule01].Should().HaveCount(2);
        }

        private static ReportingDescriptor GenerateRule()
        {
            return new ReportingDescriptor { Id = $"TEST00{Random.Next(100)}" };
        }

        private static Result GenerateResult()
        {
            string message = Guid.NewGuid().ToString();
            string uriText = Guid.NewGuid().ToString();

            Uri uri = new Uri(uriText, UriKind.RelativeOrAbsolute);

            return new Result
            {
                Level = FailureLevel.Error,
                Message = new Message { Text = message },
                Locations = new[]
                {
                    new Location
                    {
                         PhysicalLocation = new PhysicalLocation
                         {
                             ArtifactLocation = new ArtifactLocation
                             {
                                 Uri = uri
                             },
                             Region = new Region
                             {
                                CharOffset = 1,
                                CharLength = 10
                             }
                         }
                    }
                }
            };
        }
    }
}
