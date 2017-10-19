// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Moq;
using Xunit;
using FluentAssertions;


namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverterTests
    {
        private TSLintLog CreateTestTSLintLog()
        {
            TSLintLog tsLintLog = new TSLintLog();

            TSLintLogEntry testEntry = new TSLintLogEntry()
            {
                Failure = "failure.test.value",
                Name = "name.test.value",
                RuleName = "ruleName.test.value",
                RuleSeverity = "WARN"
            };

            TSLintLogFix testFix = new TSLintLogFix()
            {
                InnerLength = 5,
                InnerStart = 10,
                InnerText = "fix.innerText.test.value"
            };

            testEntry.Fixes = new List<TSLintLogFix>() { testFix };

            TSLintLogPosition testStartPos = new TSLintLogPosition()
            {
                Character = 1,
                Line = 2,
                Position = 3
            };
            testEntry.StartPosition = testStartPos;

            TSLintLogPosition testEndPos = new TSLintLogPosition()
            {
                Character = 11,
                Line = 12,
                Position = 13
            };
            testEntry.EndPosition = testEndPos;

            tsLintLog.Add(testEntry);

            return tsLintLog;
        }

        private Result CreateTestResult()
        {
            Result testResult = new Result()
            {
                RuleId = "ruleName.test.value",
                Message = "failure.test.value",
                Level = ResultLevel.Warning
            };

            Region region = new Region()
            {
                StartLine = 3,
                StartColumn = 2,
                EndLine = 13,
                EndColumn = 12,

                Offset = 3,
                Length = 11
            };
            PhysicalLocation physLoc = new PhysicalLocation()
            {
                Uri = new Uri("name.test.value", UriKind.Relative),
                Region = region
            };
            Location location = new Location()
            {
                AnalysisTarget = physLoc
            };

            testResult.Locations = new List<Location>()
            {
                location
            };

            Replacement replacement = new Replacement()
            {
                Offset = 10,
                DeletedLength = 5,
                InsertedBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes("fix.innerText.test.value"))
            };

            testResult.Fixes = new List<Fix>()
            {
                new Fix()
                {
                    FileChanges = new List<FileChange>()
                    {
                        new FileChange()
                        {
                            Uri = new Uri("name.test.value", UriKind.Relative),
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

            Action action = () => converter.Convert(null, mockWriter.Object, LoggingOptions.None);
            action.ShouldThrow<ArgumentNullException>();

            action = () => converter.Convert(new MemoryStream(), null, LoggingOptions.None);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void TSLintConverter_Convert_WhenInputIsValid_Passes()
        {
            var mockLoader = new Mock<ITSLintLoader>();

            mockLoader.Setup(loader => loader.ReadLog(It.IsAny<Stream>())).Returns(CreateTestTSLintLog());

            var mockWriter = new Mock<IResultLogWriter>();
            mockWriter.Setup(writer => writer.Initialize(It.IsAny<string>(), It.IsAny<string>()));
            mockWriter.Setup(writer => writer.WriteTool(It.IsAny<Tool>()));
            mockWriter.Setup(writer => writer.WriteFiles(It.IsAny<IDictionary<string, FileData>>()));
            mockWriter.Setup(writer => writer.OpenResults());
            mockWriter.Setup(writer => writer.CloseResults());
            mockWriter.Setup(writer => writer.WriteResults(It.IsAny<List<Result>>()));

            var converter = new TSLintConverter(mockLoader.Object);

            converter.Convert(new MemoryStream(), mockWriter.Object, LoggingOptions.None);

            mockLoader.Verify(loader => loader.ReadLog(It.IsAny<Stream>()), Times.Once);
            mockWriter.Verify(writer => writer.Initialize(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockWriter.Verify(writer => writer.WriteTool(It.IsAny<Tool>()), Times.Once);
            mockWriter.Verify(writer => writer.WriteFiles(It.IsAny<IDictionary<string, FileData>>()), Times.Once);
            mockWriter.Verify(writer => writer.OpenResults(), Times.Once);
            mockWriter.Verify(writer => writer.CloseResults(), Times.Once);
            mockWriter.Verify(writer => writer.WriteResults(It.IsAny<List<Result>>()), Times.Once);
        }

        [Fact]
        public void TSLintConverter_CreateResult_WhenInputIsNull_ThrowsArgumentNullException()
        {
            var converter = new TSLintConverter();

            Action action = () => converter.CreateResult(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void TSLintConverter_CreateResult_CreatesExpectedResult()
        {
            var converter = new TSLintConverter();
            TSLintLog tSLintLog = CreateTestTSLintLog();

            Result actualResult = converter.CreateResult(tSLintLog[0]);
            Result expectedResult = CreateTestResult();

            Result.ValueComparer.Equals(actualResult, expectedResult).Should().BeTrue();
        }
    }
}
