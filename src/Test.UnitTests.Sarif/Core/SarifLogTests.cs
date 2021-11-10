// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

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
            var random = new Random();
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(1);

            sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 3);
            sarifLog.Split(SplittingStrategy.PerRun).Should().HaveCount(3);
        }

        [Fact]
        public void SarifLog_SplitPerResult()
        {
            var random = new Random();
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
            var random = new Random();
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
            var random = new Random();
            SarifLog sarifLog = RandomSarifLogGenerator.GenerateSarifLogWithRuns(random, 1);

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
            byte[] data = new byte[4];
            RandomNumberGenerator.Fill(data);
            int seed = BitConverter.ToInt32(data);
            var random = new Random(seed);

            this.output.WriteLine($"The seed passed to the Random instance was : {seed}.");

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
        public async Task SarifLog_Post_WithValidParameters_ShouldNotThrownAnExceptionWhenRequestIsValid()
        {
            var postUri = new Uri("https://github.com/microsoft/sarif-sdk");
            var sarifLog = new SarifLog();
            var httpMock = new HttpMockHelper();
            var memoryStream = new MemoryStream();
            sarifLog.Save(memoryStream);

            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.BadRequestResponse);

            Exception exception = await Record.ExceptionAsync(async () =>
            {
                await SarifLog.Post(postUri,
                                    memoryStream,
                                    new HttpClient(httpMock));
            });
            exception.Should().BeOfType(typeof(HttpRequestException));
            httpMock.Clear();

            memoryStream = new MemoryStream();
            sarifLog.Save(memoryStream);
            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.OKResponse);

            exception = await Record.ExceptionAsync(async () =>
            {
                await SarifLog.Post(postUri,
                                    memoryStream,
                                    new HttpClient(httpMock));
            });
            exception.Should().BeNull();
            httpMock.Clear();

            string filePath = "SomeFile.txt";
            var fileSystem = new Mock<IFileSystem>();
            memoryStream = new MemoryStream();
            sarifLog.Save(memoryStream);

            fileSystem
                .Setup(f => f.FileExists(It.IsAny<string>()))
                .Returns(true);

            fileSystem
                .Setup(f => f.FileOpenRead(It.IsAny<string>()))
                .Returns(memoryStream);

            httpMock.Mock(
                new HttpRequestMessage(HttpMethod.Post, postUri) { Content = new StreamContent(memoryStream) },
                HttpMockHelper.OKResponse);

            exception = await Record.ExceptionAsync(async () =>
            {
                await SarifLog.Post(postUri,
                                    filePath,
                                    fileSystem.Object,
                                    new HttpClient(httpMock));
            });
            exception.Should().BeNull();
            httpMock.Clear();
        }

        private Run SerializeAndDeserialize(Run run)
        {
            return JsonConvert.DeserializeObject<Run>(JsonConvert.SerializeObject(run));
        }
    }
}
