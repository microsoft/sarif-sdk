// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Writers
{
    public class SarifWriterTests
    {
        private static JsonSerializer BuildSarifSerializer()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            serializer.ContractResolver = SarifContractResolver.Instance;

            return serializer;
        }

        [Fact]
        public void SarifWriter_Empty()
        {
            JsonSerializer sarifSerializer = BuildSarifSerializer();

            using (SarifWriter writer = new SarifWriter(sarifSerializer, "SarifWriter_Empty.sarif", new SarifLog()))
            { }

            SarifLog log = null;
            using (JsonTextReader reader = new JsonTextReader(new StreamReader("SarifWriter_Empty.sarif")))
            {
                log = sarifSerializer.Deserialize<SarifLog>(reader);
            }

            Assert.Null(log.Runs);
        }

        [Fact]
        public void SarifWriter_Normal()
        {
            JsonSerializer sarifSerializer = BuildSarifSerializer();
            Uri sampleFileUri = new Uri(@"C:\Code\sarif-sdk\Sample.cs");

            // Write a Run, File, and Result
            using (SarifWriter writer = new SarifWriter(sarifSerializer, "SarifWriter_Normal.sarif", new SarifLog() { Version = SarifVersion.TwoZeroZero }))
            {
                Run run = new Run() { Tool = new Tool() { Name = "Sarif.UnitTests", Version = "1.0.0" } };
                writer.Write(run);

                FileData file = new FileData() { FileLocation = new FileLocation() { Uri = sampleFileUri } };
                writer.Write(file.FileLocation.Uri.OriginalString, file);
                
                Result result = new Result()
                {
                    AnalysisTarget = file.FileLocation,
                    RuleId = "ST1001",
                    Message = new Message() { Text = "Test Issue" }
                };
                writer.Write(result);

                // Make sure changing our object copies before Dispose doesn't break anything
                run.Files = null;
                run.Results = null;
                result.RuleId = "";
                file.FileLocation = null;
            }

            // Read back and verify
            SarifLog actual = null;
            using (JsonTextReader reader = new JsonTextReader(new StreamReader("SarifWriter_Normal.sarif")))
            {
                actual = sarifSerializer.Deserialize<SarifLog>(reader);
            }

            Assert.NotNull(actual.Runs);

            Run actualRun = actual.Runs[0];
            Assert.NotNull(actualRun.Files);
            Assert.NotNull(actualRun.Results);

            Assert.Equal(sampleFileUri, actualRun.Files.First().Value.FileLocation.Uri);
            Assert.Equal(sampleFileUri, actualRun.Results[0].AnalysisTarget.Uri);
            Assert.Equal("ST1001", actualRun.Results[0].RuleId);
            Assert.Equal("Test Issue", actualRun.Results[0].Message.Text);
        }
    }
}
