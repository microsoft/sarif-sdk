// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverterTests
    {
        public TSLintLog CreateTestTSLintLog()
        {
            TSLintLog tsLintLog = new TSLintLog();

            TSLintLogEntry testEntry = new TSLintLogEntry()
            {
                Failure = "failure.test.value",
                Name = "name.test.value",
                RuleName = "ruleName.test.value",
                RuleSeverity = "ruleSeverity.test.value"
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

        // This is necessary because Microsoft.CodeAnalysis.Sarif.Result doesn't implement IEquatable
        // Must be kept up to date with any changes to TSLintConverter.CreateResult
        public bool CompareResultToTestData(Result result)
        {
            if (!result.RuleId.Equals("ruleName.test.value")) return false;
            if (!result.Message.Equals("failure.test.value")) return false;

            if (result.Level != ResultLevel.NotApplicable) return false;

            if (result.Locations.Count != 1) return false;

            Region region = result.Locations[0].AnalysisTarget.Region;
            if (region.StartLine != 3) return false;
            if (region.StartColumn != 2) return false;
            if (region.EndLine != 13) return false;
            if (region.EndColumn != 12) return false;

            if (region.Offset != 3) return false;
            if (region.Length != 11) return false;

            if (!result.Locations[0].AnalysisTarget.Uri.OriginalString.Equals("name.test.value")) return false;

            if (result.Fixes.Count != 1) return false;
            if (result.Fixes[0].FileChanges.Count != 1) return false;
            if (result.Fixes[0].FileChanges[0].Replacements.Count != 1) return false;
            Replacement replacement = result.Fixes[0].FileChanges[0].Replacements[0];

            if (replacement.Offset != 10) return false;
            if (replacement.DeletedLength != 5) return false;
            if (!replacement.InsertedBytes.Equals("fix.innerText.test.value")) return false;

            return true;
        }

        //[Fact]
        //[Trait("Category", "E2E")]
        //public void SimpleE2ETest_ManualValidation()
        //{
        //    ToolFormatConverter converter = new ToolFormatConverter();
        //    converter.ConvertToStandardFormat(ToolFormat.TSLint, @"E:\data\TSLint\TSLint.Results.json", @"E:\data\TSLint\TSLint.Results.sarif");
        //}

        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintConverterTest_Convert_S2S_NullInput()
        {
            TSLintConverter converter = new TSLintConverter();

            Mock<IResultLogWriter> mockWriter = new Mock<IResultLogWriter>();

            Assert.Throws<ArgumentNullException>(() => converter.Convert(null, mockWriter.Object, LoggingOptions.None));
            Assert.Throws<ArgumentNullException>(() => converter.Convert(new MemoryStream(), null, LoggingOptions.None));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintConverterTest_Convert_S2S_ValidInput()
        {
            Mock<ITSLintLoader> mockLoader = new Mock<ITSLintLoader>();

            mockLoader.Setup(loader => loader.ReadLog(It.IsAny<Stream>())).Returns(CreateTestTSLintLog());

            Mock<IResultLogWriter> mockWriter = new Mock<IResultLogWriter>();
            mockWriter.Setup(writer => writer.Initialize(It.IsAny<string>(), It.IsAny<string>()));
            mockWriter.Setup(writer => writer.WriteTool(It.IsAny<Tool>()));
            mockWriter.Setup(writer => writer.WriteFiles(It.IsAny<IDictionary<string, FileData>>()));
            mockWriter.Setup(writer => writer.OpenResults());
            mockWriter.Setup(writer => writer.CloseResults());
            mockWriter.Setup(writer => writer.WriteResults(It.IsAny<List<Result>>()));

            TSLintConverter converter = new TSLintConverter(mockLoader.Object);

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
        [Trait("Category", "Unit")]
        public void TSLintConverterTest_HelperMethods_NullInput()
        {
            TSLintConverter converter = new TSLintConverter();

            Assert.Throws<ArgumentNullException>(() => converter.CreateResult(null));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TSLintConverterTest_HelperMethods_ValidInput()
        {
            TSLintConverter converter = new TSLintConverter();
            TSLintLog tSLintLog = CreateTestTSLintLog();

            Result actualResult = converter.CreateResult(tSLintLog[0]);

            Assert.True(CompareResultToTestData(actualResult));
        }

    }
}
