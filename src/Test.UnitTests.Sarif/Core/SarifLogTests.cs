// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Core
{
    public class SarifLogTests
    {
        private readonly ITestOutputHelper output;

        public SarifLogTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SarifLog_DoesNotSerializeNonNullEmptyCollections()
        {
            var run = new Run
            {
                Graphs = new Graph[] { },
                Artifacts = new Artifact[] { },
                Invocations = new Invocation[] { },
                LogicalLocations = new LogicalLocation[] { }
            };

            run.Graphs.Should().NotBeNull();
            run.Artifacts.Should().NotBeNull();
            run.Invocations.Should().NotBeNull();
            run.LogicalLocations.Should().NotBeNull();

            run = SerializeAndDeserialize(run);

            // Certain non-null but entirely empty collections should not
            // be persisted during serialization. As a result, these properties
            // should be null after round-tripping, reflecting the actual
            // (i.e., entirely absent) representation on disk when saved.

            run.Graphs.Should().BeNull();
            run.Artifacts.Should().BeNull();
            run.Invocations.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            // If arrays are non-empty but only contain object instances
            // that consist of nothing but default values, these also
            // should not be persisted to disk
            run.Graphs = new Graph[] { new Graph() };
            run.Artifacts = new Artifact[] { new Artifact() };
            run.LogicalLocations = new LogicalLocation[] { new LogicalLocation() };

            // Invocations are special, they have a required property,
            // ExecutionSuccessful. This means even an entirely default instance
            // should be retained when serialized.
            run.Invocations = new Invocation[] { new Invocation() };

            run = SerializeAndDeserialize(run);

            run.Graphs.Should().BeNull();
            run.Artifacts.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            run.Invocations.Should().NotBeNull();
        }

        [Fact]
        public void SarifLog_ApplyPoliciesShouldNotThrowWhenRunsDoesNotExist()
        {
            var sarifLog = new SarifLog();
            Action action = () => sarifLog.ApplyPolicies();

            action.Should().NotThrow();
        }

        [Fact]
        public void SarifLog_SplitPerRun()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);

            int expectedLogCount = sarifLog.Runs.First().Results.Any() ? 1 : 0;
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(expectedLogCount);

            sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 3);

            expectedLogCount = sarifLog.Runs.Count(run => run.Results.Any());
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(expectedLogCount);
        }

        [Fact]
        public void SarifLog_SplitPerResult()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, runCount: 1, resultCount: 5);
            int countOfDistinctRules = sarifLog.Runs[0].Results.Select(r => r.RuleId).Distinct().Count();
            IList<SarifLog> logs = sarifLog.Split(SplittingStrategy.PerResult).ToList();
            logs.Count.Should().Be(countOfDistinctRules);
            foreach (SarifLog log in logs)
            {
                // optimized partitioned log should only include rules referenced by its results
                log.Runs.Count.Should().Be(1);
                int ruleCount = log.Runs[0].Results.Select(r => r.RuleId).Distinct().Count();
                ruleCount.Should().Be(log.Runs[0].Tool.Driver.Rules.Count);
            }
        }

        [Fact]
        public void SarifLog_SplitPerTarget()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            IList<SarifLog> logs = sarifLog.Split(SplittingStrategy.PerRunPerTarget).ToList();
            logs.Count.Should().Be(
                sarifLog.Runs[0].Results.Select(r => r.Locations[0].PhysicalLocation.ArtifactLocation.Uri).Distinct().Count());
            foreach (SarifLog log in logs)
            {
                // optimized partitioned log should only include rules referenced by its results
                log.Runs.Count.Should().Be(1);
                int ruleCount = log.Runs[0].Results.Select(r => r.RuleId).Distinct().Count();
                ruleCount.Should().Be(log.Runs[0].Tool.Driver.Rules.Count);

                // verify result's RuleIndex reference to right rule
                foreach (Result result in log.Runs[0].Results)
                {
                    result.RuleId.Should().Be(
                        log.Runs[0].Tool.Driver.Rules.ElementAt(result.RuleIndex).Id);
                }
            }
        }

        [Fact]
        public void SarifLog_SplitPerTarget_WithEmptyLocations()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog sarifLog = null;

            while (true)
            {
                sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
                if (sarifLog.Runs[0].Results.Count > 0) { break; }
            }

            // set random result's location to empty
            IList<Result> results = sarifLog.Runs.First().Results;
            Result randomResult = results.ElementAt(random.Next(results.Count));
            randomResult.Locations = null;
            randomResult = results.ElementAt(random.Next(results.Count));
            if (randomResult.Locations?.FirstOrDefault()?.PhysicalLocation != null)
            {
                randomResult.Locations.FirstOrDefault().PhysicalLocation = null;
            }
            randomResult = results.ElementAt(random.Next(results.Count));
            if (randomResult.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation != null)
            {
                randomResult.Locations.FirstOrDefault().PhysicalLocation.ArtifactLocation = null;
            }
            randomResult = results.ElementAt(random.Next(results.Count));
            if (randomResult.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation?.Uri != null)
            {
                randomResult.Locations.FirstOrDefault().PhysicalLocation.ArtifactLocation.Uri = null;
            }

            IList<SarifLog> logs = sarifLog.Split(SplittingStrategy.PerRunPerTarget).ToList();
            logs.Count.Should().Be(
                sarifLog.Runs[0].Results.Select(r => r.Locations?.FirstOrDefault()?.PhysicalLocation?.ArtifactLocation?.Uri).Distinct().Count());
            foreach (SarifLog log in logs)
            {
                // optimized partitioned log should only include rules referenced by its results
                log.Runs.Count.Should().Be(1);
                int ruleCount = log.Runs[0].Results.Select(r => r.RuleId).Distinct().Count();
                ruleCount.Should().Be(log.Runs[0].Tool.Driver.Rules.Count);

                // verify result's RuleIndex reference to right rule
                foreach (Result result in log.Runs[0].Results)
                {
                    result.RuleId.Should().Be(
                        log.Runs[0].Tool.Driver.Rules.ElementAt(result.RuleIndex).Id);
                }
            }
        }

        [Fact]
        public void SarifLog_LoadDeferred()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            string sarifLogText = JsonConvert.SerializeObject(sarifLog);
            byte[] byteArray = Encoding.ASCII.GetBytes(sarifLogText);
            using var stream = new MemoryStream(byteArray);
            var newSarifLog = SarifLog.Load(stream, deferred: true);
            newSarifLog.Runs[0].Tool.Driver.Name.Should().Be(sarifLog.Runs[0].Tool.Driver.Name);
            newSarifLog.Runs[0].Results.Count.Should().Be(sarifLog.Runs[0].Results.Count);
        }

        [Fact]
        public async Task SarifLog_PostStream_WithInvalidParameters_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SarifLog.Post(postUri: null,
                                    new MemoryStream(),
                                    new HttpClient());
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SarifLog.Post(new Uri("https://github.com/microsoft/sarif-sdk"),
                                    null,
                                    new HttpClient());
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SarifLog.Post(new Uri("https://github.com/microsoft/sarif-sdk"),
                                    new MemoryStream(),
                                    null);
            });
        }

        [Fact]
        public async Task SarifLog_PostFile_WithInvalidParameters_ShouldThrowException()
        {
            string filePath = string.Empty;
            var fileSystem = new Mock<IFileSystem>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await SarifLog.Post(postUri: null,
                                    filePath,
                                    fileSystem.Object,
                                    httpClient: null);
            });

            filePath = "SomeFile.txt";
            fileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await SarifLog.Post(postUri: null,
                                    filePath,
                                    fileSystem.Object,
                                    httpClient: null);
            });
        }

        [Fact]
        public async Task SarifLog_PostFile_PostTests()
        {
            using var assertionScope = new AssertionScope();

            var postUri = new Uri("https://sarif-post/example.com");
            var httpMock = new HttpMockHelper();
            string filePath = "SomeFile.sarif";
            var fileSystem = new Mock<IFileSystem>();
            fileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            // This method creates a bare-bones SARIF file with no results and notifications.
            var stream = (MemoryStream)CreateSarifLogStream();
            fileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());
            (bool, string) logPosted = await SarifLog.Post(postUri,
                                                           filePath,
                                                           fileSystem.Object,
                                                           httpClient: null);
            logPosted.Item1.Should().BeFalse("with no results or notifications");
            logPosted.Item2.Should().Contain("was skipped");

            string content = $"{Guid.NewGuid()}";
            var httpContent = new StringContent(content, Encoding.UTF8, "text/plain");

            stream = (MemoryStream)CreateSarifLogStreamWithResult();
            fileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());
            httpMock.Mock(HttpMockHelper.CreateOKResponse(httpContent));
            logPosted = await SarifLog.Post(postUri,
                                            filePath,
                                            fileSystem.Object,
                                            httpClient: new HttpClient(httpMock));
            logPosted.Item1.Should().BeTrue("with results");
            logPosted.Item2.Should().Contain("status code 'OK'");
            logPosted.Item2.Should().Contain(content);

            stream = (MemoryStream)CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel.Error);
            fileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());
            httpMock.Mock(HttpMockHelper.CreateOKResponse(httpContent));
            logPosted = await SarifLog.Post(postUri,
                                            filePath,
                                            fileSystem.Object,
                                            httpClient: new HttpClient(httpMock));
            logPosted.Item1.Should().BeTrue("with error level ToolExecutionNotifications");
            logPosted.Item2.Should().Contain("status code 'OK'");
            logPosted.Item2.Should().Contain(content);

            stream = (MemoryStream)CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel.Warning);
            fileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());
            logPosted = await SarifLog.Post(postUri,
                                            filePath,
                                            fileSystem.Object,
                                            httpClient: new HttpClient(httpMock));
            logPosted.Item1.Should().BeFalse("with warning level ToolExecutionNotifications");
            logPosted.Item2.Should().Contain("was skipped");

            stream = (MemoryStream)CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel.Error);
            fileSystem
                .Setup(f => f.FileReadAllBytes(It.IsAny<string>()))
                .Returns(stream.ToArray());
            httpMock.Mock(HttpMockHelper.CreateBadRequestResponse());
            logPosted = await SarifLog.Post(postUri,
                                            filePath,
                                            fileSystem.Object,
                                            httpClient: new HttpClient(httpMock));
            logPosted.Item1.Should().BeFalse("the server returns a BadRequest even though there are error level ToolExecutionNotifications");
            logPosted.Item2.Should().Contain("status code 'BadRequest'");
        }

        [Fact]
        public void SarifLog_SaveToMemoryStreamRoundtrips()
        {
            SarifLog sarifLog = GetSarifLogWithMinimalUniqueData();

            var memoryStream = new MemoryStream();
            sarifLog.Save(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStream);
            SarifLog newSarifLog = JsonConvert.DeserializeObject<SarifLog>(reader.ReadToEnd());

            newSarifLog.Should().BeEquivalentTo(sarifLog);
        }

        [Fact]
        public void SarifLog_SaveToStreamWriterRoundtrips()
        {
            SarifLog sarifLog = GetSarifLogWithMinimalUniqueData();

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream); ;
            sarifLog.Save(writer);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memoryStream);
            SarifLog newSarifLog = JsonConvert.DeserializeObject<SarifLog>(reader.ReadToEnd());

            newSarifLog.Should().BeEquivalentTo(sarifLog);
        }

        private static SarifLog GetSarifLogWithMinimalUniqueData()
        {
            var sarifLog = new SarifLog() { Runs = new[] { new Run() { } } };
            sarifLog.Runs[0].SetProperty("test", Guid.NewGuid().ToString());
            return sarifLog;
        }

        [Fact]
        public async Task SarifLog_Post_BadRequestResponse()
        {
            var postUri = new Uri("https://sarif-post/example.com");
            var httpMock = new HttpMockHelper();

            httpMock.Mock(HttpMockHelper.CreateBadRequestResponse());

            HttpResponseMessage response =
                await SarifLog.Post(postUri,
                    CreateSarifLogStream(),
                    new HttpClient(httpMock));

            Assert.Throws<HttpRequestException>(() =>
            {
                response.EnsureSuccessStatusCode();
            });
        }

        [Fact]
        public async Task SarifLog_Post_OkRequestResponseFromStream()
        {
            var postUri = new Uri("https://sarif-post/example.com");
            var httpMock = new HttpMockHelper();

            httpMock.Mock(HttpMockHelper.CreateOKResponse());

            try
            {
                await SarifLog.Post(postUri,
                                    CreateSarifLogStream(),
                                    new HttpClient(httpMock));
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Unhandled exception: {ex}");
            }
        }

        [Fact]
        public async Task SarifLog_Post_OkRequestResponseFromFilePath()
        {
            var postUri = new Uri("https://sarif-post/example.com");
            var httpMock = new HttpMockHelper();

            string filePath = "SomeFile.txt";
            var fileSystem = new Mock<IFileSystem>();
            fileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            fileSystem
                .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                .Returns(CreateSarifLogStream());

            httpMock.Mock(HttpMockHelper.CreateOKResponse());

            try
            {
                await SarifLog.Post(postUri,
                                    filePath,
                                    fileSystem.Object,
                                    new HttpClient(httpMock));
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Unhandled exception: {ex}");
            }
            httpMock.Clear();
        }

        private Stream CreateSarifLogStream()
        {
            var memoryStream = new MemoryStream();
            new SarifLog().Save(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Stream CreateSarifLogStreamWithResult()
        {
            var memoryStream = new MemoryStream();
            var sarifLog = new SarifLog();
            var result = new Result() { Message = new Message() { Text = "A sample result message." } };
            var run = new Run();
            run.Results = new Result[] { result };
            sarifLog.Runs = new Run[] { run };
            sarifLog.Save(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Stream CreateSarifLogStreamWithToolConfigurationNotifications(FailureLevel level)
        {
            var memoryStream = new MemoryStream();
            var sarifLog = new SarifLog();
            var run = new Run();
            run.Invocations = new Invocation[] { new Invocation()
            { ToolConfigurationNotifications = new Notification[] { new Notification() { Level = level } } } };
            sarifLog.Runs = new Run[] { run };
            sarifLog.Save(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Stream CreateSarifLogStreamWithToolExecutionNotifications(FailureLevel level)
        {
            var memoryStream = new MemoryStream();
            var sarifLog = new SarifLog();
            var run = new Run();
            run.Invocations = new Invocation[] { new Invocation()
            { ToolExecutionNotifications = new Notification[] { new Notification() { Level = level } } } };
            sarifLog.Runs = new Run[] { run };
            sarifLog.Save(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private Run SerializeAndDeserialize(Run run)
        {
            return JsonConvert.DeserializeObject<Run>(JsonConvert.SerializeObject(run));
        }
    }
}
