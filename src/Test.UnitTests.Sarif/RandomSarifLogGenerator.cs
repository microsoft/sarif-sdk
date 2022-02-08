// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Generate random sarif logs for testing.
    /// </summary>
    internal static class RandomSarifLogGenerator
    {
        public static string GeneratorBaseUri = @"C:\src\";

        public static Random GenerateRandomAndLog(ITestOutputHelper output, [CallerMemberName] string testName = "")
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();

            Random random = new Random(randomSeed);

            output.WriteLine($"TestName: {testName} has seed {randomSeed}");

            return random;
        }

        public static SarifLog GenerateSarifLogWithRuns(Random randomGen, int runCount, int? resultCount = null, RandomDataFields dataFields = RandomDataFields.None)
        {
            SarifLog log = new SarifLog();

            if (runCount > 0)
            {
                log.Runs = new List<Run>();
            }

            for (int i = 0; i < runCount; i++)
            {
                log.Runs.Add(GenerateRandomRun(randomGen, resultCount, dataFields));
            }

            return log;
        }

        public static Run GenerateRandomRun(Random random, int? resultCount = null, RandomDataFields dataFields = RandomDataFields.None)
        {
            List<string> ruleIds = new List<string>() { "TEST001", "TEST002", "TEST003", "TEST004", "TEST005" };
            List<Uri> filePaths = GenerateFakeFiles(GeneratorBaseUri, random.Next(20) + 1).Select(a => new Uri(a)).ToList();
            int results = resultCount == null ? random.Next(100) : (int)resultCount;

            return new Run()
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = "Test",
                        Version = "1.0",
                        Rules = new List<ReportingDescriptor>(GenerateRules(ruleIds))
                    }
                },
                Artifacts = GenerateFiles(filePaths),
                Results = GenerateFakeResults(random, ruleIds, filePaths, results, dataFields)
            };
        }

        public static Run GenerateRandomRunWithoutDuplicateIssues(Random random, IEqualityComparer<Result> comparer, int? resultCount = null)
        {
            Run run = GenerateRandomRun(random, resultCount);
            IList<Result> resultList = run.Results;
            List<Result> uniqueResults = new List<Result>();
            foreach (Result result in resultList)
            {
                if (!uniqueResults.Contains(result, comparer))
                {
                    uniqueResults.Add(result);
                }
            }
            run.Results = uniqueResults;
            return run;
        }

        public static IEnumerable<string> GenerateFakeFiles(string baseAddress, int count)
        {
            List<string> results = new List<string>();

            for (int i = 0; i < count; i++)
            {
                results.Add(Path.Combine(baseAddress, Guid.NewGuid().ToString()));
            }

            return results;
        }

        public static IList<Result> GenerateFakeResults(Random random, List<string> ruleIds, List<Uri> filePaths, int resultCount, RandomDataFields dataFields = RandomDataFields.None)
        {
            List<Result> results = new List<Result>();
            for (int i = 0; i < resultCount; i++)
            {
                int fileIndex = random.Next(filePaths.Count);
                int ruleIndex = random.Next(ruleIds.Count);
                results.Add(new Result()
                {
                    RuleId = ruleIds[ruleIndex],
                    RuleIndex = ruleIndex,
                    Locations = new Location[]
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation()
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = filePaths[fileIndex],
                                    Index = fileIndex
                                },
                            },
                            LogicalLocations = dataFields.HasFlag(RandomDataFields.LogicalLocation) ?
                                               GenerateLogicalLocations(random) :
                                               null,
                        }
                    },
                    CodeFlows = dataFields.HasFlag(RandomDataFields.CodeFlow) ?
                                GenerateCodeFlows(random, filePaths, dataFields) :
                                null,
                });
            }
            return results;
        }

        public static IList<Artifact> GenerateFiles(List<Uri> filePaths)
        {
            var files = new List<Artifact>();
            foreach (Uri path in filePaths)
            {
                files.Add(
                    new Artifact()
                    {
                        Location = new ArtifactLocation
                        {
                            Uri = path
                        }
                    });
            }
            return files;
        }

        public static IList<ReportingDescriptor> GenerateRules(List<string> ruleIds)
        {
            var rules = new List<ReportingDescriptor>();

            foreach (string ruleId in ruleIds)
            {
                rules.Add(
                    new ReportingDescriptor()
                    {
                        Id = ruleId,
                        FullDescription = new MultiformatMessageString
                        {
                            Text = "TestRule"
                        }
                    });
            }
            return rules;
        }

        public static IList<CodeFlow> GenerateCodeFlows(Random random, IList<Uri> artifacts, RandomDataFields dataFields)
        {
            if (artifacts?.Any() != true)
            {
                return null;
            }

            var codeFlow = new CodeFlow
            {
                Message = new Message { Text = "code flow message" },
                ThreadFlows = new[]
                {
                    new ThreadFlow
                    {
                        Message = new Message { Text = "thread flow message" },
                        Locations = dataFields.HasFlag(RandomDataFields.ThreadFlow) ?
                                    GenerateThreadFlowLocations(random, artifacts) :
                                    null,
                    },
                },
            };

            return new[] { codeFlow };
        }

        public static IList<ThreadFlowLocation> GenerateThreadFlowLocations(Random random, IList<Uri> artifacts)
        {
            var locations = new List<ThreadFlowLocation>();

            for (int i = 0; i < random.Next(10); i++)
            {
                locations.Add(new ThreadFlowLocation
                {
                    Importance = RandomEnumValue<ThreadFlowLocationImportance>(random),
                    Location = new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Index = random.Next(artifacts.Count),
                            },
                            Region = new Region
                            {
                                StartLine = random.Next(500),
                                StartColumn = random.Next(100),
                            },
                        },
                    },
                });
            }

            return locations;
        }

        public static IList<LogicalLocation> GenerateLogicalLocations(Random random)
        {
            var logicalLocations = new List<LogicalLocation>();
            for (int i = 0; i < random.Next(5); i++)
            {
                logicalLocations.Add(new LogicalLocation
                {
                    Name = $"Class{i}",
                    Index = i,
                    FullyQualifiedName = "namespaceA::namespaceB::namespaceC",
                    Kind = LogicalLocationKind.Type,
                });
            }

            return logicalLocations;
        }

        public static T RandomEnumValue<T>(Random random) where T : Enum
        {
            Array enums = Enum.GetValues(typeof(T));
            return (T)enums.GetValue(random.Next(enums.Length));
        }
    }

    [Flags]
    public enum RandomDataFields
    {
        None = 0,
        CodeFlow = 0b1,
        ThreadFlow = 0b10,
        LogicalLocation = 0b100,
    }
}
