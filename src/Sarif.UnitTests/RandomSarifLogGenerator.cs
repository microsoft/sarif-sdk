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

        public static Random GenerateRandomAndLog(ITestOutputHelper output, [CallerMemberName] string testName="")
        {
            // Slightly roundabout.  We want to randomly test this, but we also want to be able to repeat this if the test fails.
            int randomSeed = (new Random()).Next();

            Random random = new Random(randomSeed);

            output.WriteLine($"TestName: {testName} has seed {randomSeed}");

            return random;
        }

        public static SarifLog GenerateSarifLogWithRuns(Random randomGen, int runCount)
        {
            SarifLog log = new SarifLog();

            if (runCount > 0)
            {
                log.Runs = new List<Run>();
            }

            for (int i = 0; i < runCount; i++)
            {
                log.Runs.Add(GenerateRandomRun(randomGen));
            }

            return log;
        }

        public static Run GenerateRandomRun(Random random, int? resultCount = null)
        {
            Run run = new Run() { Resources = new Resources() };
            List<string> ruleIds = new List<string>() { "TEST001", "TEST002", "TEST003", "TEST004", "TEST005" };
            List<Uri> filePaths = GenerateFakeFiles(GeneratorBaseUri, random.Next(20) + 1).Select(a => new Uri(a)).ToList();

            run.Tool = new Tool() { Name = "Test", Version = "1.0", };
            run.Resources.Rules = GenerateRules(ruleIds);
            run.Files = GenerateFiles(filePaths);

            int results = resultCount == null ? random.Next(100) : (int)resultCount;
            run.Results = GenerateFakeResults(random, ruleIds, filePaths, results);

            return run;
        }

        public static Run GenerateRandomRunWithoutDuplicateIssues(Random random, IEqualityComparer<Result> comparer, int? resultCount = null)
        {
            Run run = GenerateRandomRun(random, resultCount);
            IList<Result> resultList = run.Results;
            List<Result> uniqueResults = new List<Result>();
            foreach (var result in resultList)
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

        public static IList<Result> GenerateFakeResults(Random random, List<string> ruleIds, List<Uri> filePaths, int resultCount)
        {
            List<Result> results = new List<Result>();
            for (int i = 0; i < resultCount; i++)
            { 
                results.Add(new Result()
                {
                    RuleId = ruleIds[random.Next(ruleIds.Count)],
                    Locations = new Location[] 
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation()
                            {
                                FileLocation = new FileLocation
                                {
                                    Uri = filePaths[random.Next(filePaths.Count)]
                                },
                            }
                        }
                    }
                });
            }
            return results;
        }

        public static IDictionary<string, FileData> GenerateFiles(List<Uri> filePaths)
        {
            Dictionary<string, FileData> dictionary = new Dictionary<string, FileData>();
            foreach (var path in filePaths)
            {
                dictionary.Add(
                    path.ToString(), 
                    new FileData()
                    {
                        FileLocation = new FileLocation
                        {
                            Uri = path
                        }
                    });
            }
            return dictionary;
        }

        public static IDictionary<string, IRule> GenerateRules(List<string> ruleIds)
        {
            Dictionary<string, IRule> dictionary = new Dictionary<string, IRule>();

            foreach (var ruleId in ruleIds)
            {
                dictionary.Add(ruleId, 
                    new Rule()
                    {
                        Id = ruleId,
                        FullDescription = new Message
                        {
                            Text = "TestRule"
                        }
                    });
            }
            return dictionary;
        }
    }
}
