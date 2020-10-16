// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Moq;
using Xunit;


namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverterTests
    {
        private const string InputJson = @"
        [
            {
                ""endPosition"": {
                    ""character"": 1,
                    ""line"" : 113,
                    ""position"": 4429
                },

                ""failure"": ""file should end with a newline"",
                ""fix"": {
                    ""innerStart"": 4429,
                    ""innerLength"": 0,
                    ""innerText"": ""\r\n""
                },

                ""name"": ""SecureApp/js/index.d.ts"",
                ""ruleName"": ""eofline"",
                ""ruleSeverity"": ""ERROR"",

                ""startPosition"": {
                    ""character"":1,
                    ""line"": 113,
                    ""position"": 4429
                }
            }
        ]";

        private TSLintLogEntry CreateTestLogEntry()
        {
            return new TSLintLogEntry
            {
                Failure = "failure.test.value",
                Name = "name.test.value",
                RuleName = "ruleName.test.value",
                RuleSeverity = "WARN",
                Fixes = new List<TSLintLogFix>()
                {
                    new TSLintLogFix
                    {
                        InnerLength = 5,
                        InnerStart = 10,
                        InnerText = "fix.innerText.test.value"
                    }
                },

                StartPosition = new TSLintLogPosition
                {
                    Character = 1,
                    Line = 2,
                    Position = 3
                },

                EndPosition = new TSLintLogPosition
                {
                    Character = 11,
                    Line = 12,
                    Position = 13
                }
            };
        }

        private Result CreateTestResult()
        {
            Result testResult = new Result()
            {
                RuleId = "ruleName.test.value",
                Message = new Message { Text = "failure.test.value" },
                Level = FailureLevel.Warning,
                Kind = ResultKind.Fail
            };

            Region region = new Region()
            {
                StartLine = 3,
                StartColumn = 2,
                EndLine = 13,
                EndColumn = 12,

                CharOffset = 3,
                CharLength = 10
            };
            PhysicalLocation physLoc = new PhysicalLocation()
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = new Uri("name.test.value", UriKind.Relative)
                },
                Region = region
            };
            Location location = new Location()
            {
                PhysicalLocation = physLoc
            };

            testResult.Locations = new List<Location>()
            {
                location
            };

            Replacement replacement = new Replacement()
            {
                DeletedRegion = new Region
                {
                    CharLength = 5,
                    CharOffset = 10
                },
                InsertedContent = new ArtifactContent
                {
                    Text = "fix.innerText.test.value"
                }
            };

            testResult.Fixes = new List<Fix>()
            {
                new Fix()
                {
                    ArtifactChanges = new List<ArtifactChange>()
                    {
                        new ArtifactChange()
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri("name.test.value", UriKind.Relative)
                            },
                            Replacements = new List<Replacement>()
                            {
                                replacement
                            }
                        }
                    }
                }
            };

            return testResult;
        }

        [Fact]
        public void TSLintConverter_Convert_WhenInputIsNull_ThrowsArgumentNullException()
        {
            var converter = new TSLintConverter();

            var mockWriter = new Mock<IResultLogWriter>();

            Action action = () => converter.Convert(null, mockWriter.Object, OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();

            action = () => converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TSLintConverter_Convert_WhenInputIsValid_Passes()
        {
            byte[] data = Encoding.UTF8.GetBytes(InputJson);
            MemoryStream stream = new MemoryStream(data);

            var mockWriter = new Mock<IResultLogWriter>();
            mockWriter.Setup(writer => writer.Initialize(It.IsAny<Run>()));
            mockWriter.Setup(writer => writer.WriteArtifacts(It.IsAny<IList<Artifact>>()));
            mockWriter.Setup(writer => writer.OpenResults());
            mockWriter.Setup(writer => writer.CloseResults());
            mockWriter.Setup(writer => writer.WriteResults(It.IsAny<List<Result>>()));

            var converter = new TSLintConverter();

            converter.Convert(stream, mockWriter.Object, OptionallyEmittedData.None);

            mockWriter.Verify(writer => writer.Initialize(It.IsAny<Run>()), Times.Once);
            mockWriter.Verify(writer => writer.WriteArtifacts(It.IsAny<IList<Artifact>>()), Times.Never);
            mockWriter.Verify(writer => writer.OpenResults(), Times.Once);
            mockWriter.Verify(writer => writer.CloseResults(), Times.Once);
            mockWriter.Verify(writer => writer.WriteResults(It.IsAny<List<Result>>()), Times.Once);
        }

        [Fact]
        public void TSLintConverter_CreateResult_WhenInputIsNull_ThrowsArgumentNullException()
        {
            var converter = new TSLintConverter();

            Action action = () => converter.CreateResult(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TSLintConverter_CreateResult_CreatesExpectedResult()
        {
            var converter = new TSLintConverter();
            TSLintLogEntry tSLintLogEntry = CreateTestLogEntry();

            Result actualResult = converter.CreateResult(tSLintLogEntry);
            Result expectedResult = CreateTestResult();

            Result.ValueComparer.Equals(actualResult, expectedResult).Should().BeTrue();
        }
    }
}
