// THESE ARE THROWAWAY STUBS backing the mock-up example.
// No need to review this file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sarif.WorkItems.Sample
{
    public partial class Program
    {

#if PREVIOUS
        private static void NonFunctioningEndToEndApiUsageExample()
        {
            // 1. Retrieve the current SARIF and its baseline equivalent
            SarifLog baselineSarif = RetrieveBaselineSarif();
            SarifLog currentSarif = RetrieveCurrentSarif();

            // 2. Match the SARIF files
            ISarifLogBaseliner resultMatcher = SarifLogBaselinerFactory.CreateSarifLogBaseliner(SarifBaselineType.Standard);

            SarifLog matchedLog = new SarifLog
            {
                Runs = new[]
                {
                    resultMatcher.CreateBaselinedRun(baselineSarif.Runs[0], currentSarif.Runs[0])
                }
            };

            // 3. Make sure everything has a GUID and that we have a map back to each result from it
            var guidToMatchedResultMap = new Dictionary<string, Result>();
            foreach (Result result in matchedLog.Runs[0].Results)
            {
                if (result.Guid == null)
                {
                    result.Guid = Guid.NewGuid().ToString();
                }
                guidToMatchedResultMap[result.Guid] = result;
            }

            var adoClient = new AzureDevOpsClient();
            var filingTarget = new AzureDevOpsFilingTarget(adoClient);

            var workItemFiler = new WorkItemFiler(
                filingTarget,
                filteringStrategy: new NewResultsFilteringStrategy(),
                groupingStrategy: new OneResultPerWorkItemGroupingStrategy());

            string logFilePath = Path.GetTempFileName();
            File.WriteAllText(logFilePath, JsonConvert.SerializeObject(matchedLog));

            // IMPORTANT! There's no facility defined yet for creating/configuring titles, area paths, etc.

            try
            {
                IEnumerable<ResultGroup> resultGroups = workItemFiler.FileWorkItems(logFilePath).Result;
                foreach (ResultGroup resultGroup in resultGroups)
                {
                    foreach (Result result in resultGroup.Results)
                    {
                        bool matched = guidToMatchedResultMap.TryGetValue(result.Guid, out Result originalResult);
                        Debug.Assert(matched);
                        originalResult.WorkItemUris = result.WorkItemUris;
                    }
                }
            }
            finally
            {
                if (File.Exists(logFilePath)) File.Delete(logFilePath);
            }
        }
#endif

        #region Stubs
        private static SarifLog RetrieveCurrentSarif(string v)
        {
            string sarifText = GetResourceText("Sarif.Sdk.Sample.SampleTestFiles.Current.sarif");
            return JsonConvert.DeserializeObject<SarifLog>(sarifText);
        }

        private static SarifLog RetrieveBaselineSarif(string v)
        {
            string[] resourceNames = typeof(Program).Assembly.GetManifestResourceNames();
            Console.WriteLine(String.Join("\r\n", resourceNames));

            string sarifText = GetResourceText("Sarif.Sdk.Sample.SampleTestFiles.Baseline.sarif");
            return JsonConvert.DeserializeObject<SarifLog>(sarifText);
        }

        public static string GetResourceText(string resourceName)
        {
            string text = null;

            using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) { return null; }

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            return text;
        }

        private interface ISarifLogBaseliner
        {
            SarifLog CreateBaselinedSarifLog(SarifLog baselineSarif, SarifLog currentSarif, bool assignGuidsToNewResults);
            SarifLog CreateBaselinedSarifLog(SarifLog baselineSarif, SarifLog currentSarif);
        }

        private class SarifLog
        {
            public IList<Run> Runs { get; internal set; }

            internal static IEnumerable<WorkItemFilingMetadata> CreateWorkItemFilingMetadataFromLogs(IEnumerable<SarifLog> sarifLogsToFile)
            {
                throw new NotImplementedException();
            }

            public class Run
            {
                public IList<Result> Results { get; internal set; }
            }
        }

        private class SarifLogBaselinerFactory
        {
            internal static ISarifLogBaseliner CreateSarifLogBaseliner(object standard)
            {
                throw new NotImplementedException();
            }
        }

        private class SarifBaselineType
        {
            public static object Standard { get; internal set; }
        }

        private class SarifSplitter
        {
            internal static IEnumerable<SarifLog> Split(SarifLog sarifLog, Func<Result, bool> filterPredicate, SplittingStrategy splittingStrategy)
            {
                throw new NotImplementedException();
            }

            internal class Result
            {
                public BaselineState BaselineState;
            }

            internal class SarifSplittingStrategy
            {
            }
        }

        private enum BaselineState
        {
            New
        }

        private enum SplittingStrategy
        {
            OneFilePerRunPerRule
        }

        private class WorkItemFilingMetadata
        {
            public string Title { get; internal set; }

            public string AreaPath { get; internal set; }

            public string Body { get; internal set; }

            public HashSet<string> Tags { get; internal set; }

            public Uri Uri { get; internal set; }

            public List<object> Attachments { get; internal set; }
        }


        private static void LogFailure(Exception exception)
        {
            throw new NotImplementedException();
        }

        private class AdoWorkItemFiler
        {
            public AdoWorkItemFiler()
            {
            }

            public bool FileAttachments { get; internal set; }

            public event EventHandler<WorkItemFiledOrUpdatedArgs> WorkItemFiledOrUpdated;

            internal async Task FileWorkItems(IEnumerable<WorkItemFilingMetadata> workItemsToFile)
            {
                throw new NotImplementedException();
            }

            public class WorkItemFiledOrUpdatedArgs
            {
                public WorkItemFilingMetadata Metadata;
            }
        }

        private class Result
        {
            public string Guid { get; internal set; }
            public IList<Uri> WorkItemUris { get; internal set; }
        }

        private class GuidMappingVisitor
        {
            public GuidMappingVisitor()
            {
            }

            public Dictionary<string, Result> GuidToResultMap { get; internal set; }

            internal void Visit(SarifLog matchedSarif)
            {
                throw new NotImplementedException();
            }
        }
        #endregion Stubs
    }
}
