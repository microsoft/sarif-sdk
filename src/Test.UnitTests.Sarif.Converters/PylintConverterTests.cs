// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Converters.PylintObjectModel;
using Moq;
using Xunit;

namespace Sarif.Converters.UnitTests
{
    public class PylintConverterTests
    {
        // Pylint is a static code analyzer on python source code.
        // The source code can be found in https://github.com/PyCQA/pylint
        // Running Pylint- pylint test_file.py --output-format=json

        private const string InputJson = @"
        [
            {
                ""type"": ""convention"",
                ""module"": ""kmeans"",
                ""line"": 21,
                ""column"": 0,
                ""path"": ""kmeans.py"",
                ""symbol"": ""wrong-import-order"",
                ""message"": ""standard import \""from time import time\"" should be placed before \""from sklearn.datasets import fetch_20newsgroups\"""",
                ""message-id"": ""C0411""
            }
        ]";

        private PylintLogEntry CreateTestLogEntry()
        {
            return new PylintLogEntry
            {
                Type = "convention",
                ModuleName = "test",
                Object = "",
                Line = "1",
                Column = "120",
                FilePath = "test.py",
                Symbol = "testSymbol",
                Message = "testMessage",
                MessageId = "C0412"
            };
        }

        private static Result CreateResult()
        {
            return new Result()
            {
                RuleId = "C0412(testSymbol)",
                Message = new Message { Text = "testMessage" },
                Level = FailureLevel.Warning,
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri("test.py", UriKind.RelativeOrAbsolute)
                            },
                            Region = new Region
                            {
                                StartLine = 1,
                                StartColumn = 120,
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void PylintConverter_Convert_WhenInputIsNull_ThrowsArgumentNullException()
        {
            var converter = new PylintConverter();
            var mockLogWriter = new Mock<IResultLogWriter>();

            Action action = () => converter.Convert(null, mockLogWriter.Object, OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PylintConverter_Convert_WhenInputIsValid_Passes()
        {
            byte[] data = Encoding.UTF8.GetBytes(InputJson);
            MemoryStream stream = new MemoryStream(data);

            var mockWriter = new Mock<IResultLogWriter>();
            mockWriter.Setup(writer => writer.Initialize(It.IsAny<Run>()));
            mockWriter.Setup(writer => writer.WriteArtifacts(It.IsAny<IList<Artifact>>()));
            mockWriter.Setup(writer => writer.OpenResults());
            mockWriter.Setup(writer => writer.CloseResults());
            mockWriter.Setup(writer => writer.WriteResults(It.IsAny<List<Result>>()));

            var converter = new PylintConverter();

            converter.Convert(stream, mockWriter.Object, OptionallyEmittedData.None);

            mockWriter.Verify(writer => writer.Initialize(It.IsAny<Run>()), Times.Once);
            mockWriter.Verify(writer => writer.WriteArtifacts(It.IsAny<IList<Artifact>>()), Times.Never);
            mockWriter.Verify(writer => writer.OpenResults(), Times.Once);
            mockWriter.Verify(writer => writer.CloseResults(), Times.Once);
            mockWriter.Verify(writer => writer.WriteResults(It.IsAny<List<Result>>()), Times.Once);
        }

        [Fact]
        public void PylintConverter_Convert_WhenOutputIsNull_ThrowsArgumentNullException()
        {
            var converter = new PylintConverter();

            Action action = () => converter.Convert(new MemoryStream(), null, OptionallyEmittedData.None);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PylintConverter_CreateResult_WhenInputIsNull_ThrowsArgumentNullException()
        {
            var converter = new PylintConverter();

            Action action = () => converter.CreateResult(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PylintConverter_CreateResult_CreatesExpectedResult()
        {
            var converter = new PylintConverter();
            PylintLogEntry PylintLog = CreateTestLogEntry();

            Result actualResult = converter.CreateResult(PylintLog);
            Result expectedResult = CreateResult();

            Result.ValueComparer.Equals(actualResult, expectedResult).Should().BeTrue();
        }
    }
}
