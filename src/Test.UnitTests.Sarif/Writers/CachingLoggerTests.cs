// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

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

            var logger = new CachingLogger(testAnalyzeOptions.FailureLevels, testAnalyzeOptions.ResultKinds, 0);
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

            var logger = new CachingLogger(testAnalyzeOptions.FailureLevels, testAnalyzeOptions.ResultKinds, 0);

            Assert.Throws<ArgumentNullException>(() => logger.Log(null, result01, null));
            Assert.Throws<ArgumentNullException>(() => logger.Log(rule01, null, null));

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0002";

            Assert.Throws<ArgumentException>(() => logger.Log(rule01, result01, null));

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0001";

            // Validate simple insert
            logger.Log(rule01, result01, null);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);

            // Updating value from a specific key
            logger.Log(rule01, result01, null);
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

            var logger = new CachingLogger(testAnalyzeOptions.FailureLevels, testAnalyzeOptions.ResultKinds, 0);

            rule01.Id = "TEST0001";
            result01.RuleId = "TEST0001/001";

            // Validate simple insert
            logger.Log(rule01, result01, null);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);

            // Updating value from a specific key
            logger.Log(rule01, result01, null);
            logger.Results.Should().HaveCount(1);
            logger.Results.Should().ContainKey(rule01);
            logger.Results[rule01].Should().HaveCount(2);
        }

        [Fact]
        public void SarifLogger_LimitResults()
        {
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(Random, 2, 500, RandomDataFields.None, 4);

            var logger = new CachingLogger(BaseLogger.ErrorWarning, BaseLogger.Fail, 2);

            foreach (Run run in sarifLog.Runs)
            {
                foreach (Result result in run.Results)
                {
                    logger.Log(result.GetRule(run), result, null);
                }
            }

            //2 runs x 5 rules
            logger.Results.Count.Should().BeLessThanOrEqualTo(10);
            foreach (KeyValuePair<ReportingDescriptor, IList<Tuple<Result, int?>>> resultSet in logger.Results)
            {
                //4 files x 2 instances
                resultSet.Value.Count.Should().BeLessThanOrEqualTo(8);
            }
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
