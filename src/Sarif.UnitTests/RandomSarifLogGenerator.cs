using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    /// <summary>
    /// Generate random sarif logs for testing.
    /// </summary>
    internal static class RandomSarifLogGenerator
    {
        public static SarifLog GenerateSarifLogWithRuns(Random randomGen, int runCount)
        {
            SarifLog log = new SarifLog();

            if(runCount > 0)
            {
                log.Runs = new List<Run>();
            }

            for (int i = 0; i < runCount; i++)
            {
                log.Runs.Add(GenerateRandomRun(randomGen));
            }

            return log;
        }

        public static Run GenerateRandomRun(Random random)
        {
            Run run = new Run();
            List<string> ruleIds = new List<string>() { "TEST001", "TEST002", "TEST003", "TEST004", "TEST005" };
            List<Uri> filePaths = GenerateFakeFiles(@"C:\src", random.Next(20)+1).Select(a => new Uri(a)).ToList();

            run.Tool = new Tool() { Name = "Test", Version = "1.0", };
            run.Rules = GenerateRules(ruleIds);
            run.Files = GenerateFiles(filePaths);
            run.Results = GenerateFakeResults(random, ruleIds, filePaths, random.Next(100));

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
            for (int i=0; i<resultCount; i++)
            {
                results.Add(new Result()
                {
                    RuleId = ruleIds[random.Next(ruleIds.Count)],
                    Locations = new Location[] 
                    {
                        new Location
                        {
                            AnalysisTarget = new PhysicalLocation()
                            {
                                Uri = filePaths[random.Next(filePaths.Count)],
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
                        Uri = path
                    });
            }
            return dictionary;
        }

        public static IDictionary<string, Rule> GenerateRules(List<string> ruleIds)
        {
            Dictionary<string, Rule> dictionary = new Dictionary<string, Rule>();

            foreach (var ruleId in ruleIds)
            {
                dictionary.Add(ruleId, 
                    new Rule()
                    {
                        Id = ruleId,
                        FullDescription = "TestRule",
                        DefaultLevel = ResultLevel.Pass
                    });
            }
            return dictionary;
        }
    }
}
