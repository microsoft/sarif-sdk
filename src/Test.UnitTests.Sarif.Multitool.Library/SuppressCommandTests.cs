// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Moq;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SuppressCommandTests
    {
        [Fact]
        public void SuppressCommand_ShouldReturnFailure_WhenBadArgumentsAreSupplied()
        {
            const string outputPath = @"c:\output.sarif";
            var optionsTestCases = new SuppressOptions[]
            {
                new SuppressOptions
                {
                    ExpiryInDays = -1
                },
                new SuppressOptions
                {
                    ExpiryInDays = 1,
                    Justification = string.Empty
                },
                new SuppressOptions
                {
                    ExpiryInDays = 1,
                    Justification = "some justification",
                    Status = SuppressionStatus.Rejected
                },
                new SuppressOptions
                {
                    ExpiryInDays = 1,
                    Justification = "some justification",
                    Status = SuppressionStatus.Accepted,
                    SarifOutputVersion = SarifVersion.Unknown
                },
                new SuppressOptions
                {
                    ExpiryInDays = -1,
                    Justification = "some justification",
                    OutputFilePath = outputPath,
                    Status = SuppressionStatus.Accepted
                },
            };

            var mock = new Mock<IFileSystem>();
            mock.Setup(f => f.FileExists(outputPath))
                .Returns(false);

            foreach (SuppressOptions options in optionsTestCases)
            {
                var command = new SuppressCommand();
                command.Run(options).Should().Be(CommandBase.FAILURE);
            }
        }

        [Fact]
        public void SuppressCommand_ShouldReturnSuccess_WhenCorrectArgumentsAreSupplied()
        {
            var optionsTestCases = new SuppressOptions[]
            {
                new SuppressOptions
                {
                    Alias = "some alias",
                    InputFilePath = @"C:\input.sarif",
                    OutputFilePath = @"C:\output.sarif",
                    Justification = "some justification",
                    Status = SuppressionStatus.Accepted
                },
                new SuppressOptions
                {
                    InputFilePath = @"C:\input.sarif",
                    OutputFilePath = @"C:\output.sarif",
                    Justification = "some justification",
                    Status = SuppressionStatus.UnderReview
                },
                new SuppressOptions
                {
                    Guids = true,
                    InputFilePath = @"C:\input.sarif",
                    OutputFilePath = @"C:\output.sarif",
                    Justification = "some justification",
                    Status = SuppressionStatus.Accepted
                },
                new SuppressOptions
                {
                    Guids = true,
                    ExpiryInDays = 5,
                    Timestamps = true,
                    InputFilePath = @"C:\input.sarif",
                    OutputFilePath = @"C:\output.sarif",
                    Justification = "some justification",
                    Status = SuppressionStatus.Accepted
                },
            };

            foreach (SuppressOptions options in optionsTestCases)
            {
                VerifySuppressCommand(options);
            }
        }

        private static void VerifySuppressCommand(SuppressOptions options)
        {
            var current = new SarifLog
            {
                Runs = new List<Run>
                {
                    new Run
                    {
                        Results = new List<Result>
                        {
                            new Result
                            {
                                RuleId = "Test0001"
                            }
                        }
                    }
                }
            };

            var transformedContents = new StringBuilder();
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem
                .Setup(x => x.FileReadAllText(options.InputFilePath))
                .Returns(JsonConvert.SerializeObject(current));

            mockFileSystem
                .Setup(x => x.FileCreate(options.OutputFilePath))
                .Returns(() => new MemoryStreamToStringBuilder(transformedContents));

            var command = new SuppressCommand(mockFileSystem.Object);
            command.Run(options).Should().Be(CommandBase.SUCCESS);

            SarifLog suppressed = JsonConvert.DeserializeObject<SarifLog>(transformedContents.ToString());
            suppressed.Runs[0].Results[0].Suppressions.Should().NotBeNullOrEmpty();

            Suppression suppression = suppressed.Runs[0].Results[0].Suppressions[0];
            suppression.Status.Should().Be(options.Status);
            suppression.Justification.Should().Be(options.Justification);

            if (!string.IsNullOrWhiteSpace(options.Alias))
            {
                suppression.GetProperty("alias").Should().Be(options.Alias);
            }

            if (options.Guids && suppression.TryGetProperty("guid", out Guid guid))
            {
                guid.Should().NotBeEmpty();
            }

            if (options.Timestamps && suppression.TryGetProperty("timeUtc", out DateTime timeUtc))
            {
                timeUtc.Should().BeCloseTo(DateTime.UtcNow);
            }

            if (options.ExpiryInDays > 0 && suppression.TryGetProperty("expiryUtc", out DateTime expiryUtc))
            {
                expiryUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(options.ExpiryInDays));
            }
        }
    }
}
