// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sarif.WorkItems.Sample
{
    public class Program
    {
        private static int Main(string[] args)
        {
            // Mock up of end-to-end API use.

            // 1. Retrieve the current SARIF and its baseline equivalent
            SarifLog baselineSarif = RetrieveBaselineSarif();
            SarifLog currentSarif = RetrieveCurrentSarif();

            // 2. Given a baseline and current sarif, produce a 'matched' log
            ISarifLogBaseliner resultMatcher = SarifLogBaselinerFactory.CreateSarifLogBaseliner(SarifBaselineType.Standard);

            SarifLog matchedSarif = resultMatcher.CreateBaselinedSarifLog(
                baselineSarif, 
                currentSarif);

            // 3. We need a map from each result guid to the matched SARIF result.
            //    As bugs are filed against refactored files, we use the guid to 
            //    link to the original file in order to populate the WIT URI.

            var guidMappingVisitor = new GuidMappingVisitor();
            guidMappingVisitor.Visit(matchedSarif);

            // 4. Now we need to refactor the log file into one or more log files, 
            // each of which will drive the creation of a single bug.

            IEnumerable<SarifLog> sarifLogsToFile = SarifSplitter.Split(
                sarifLog: matchedSarif,
                filterPredicate: (result) => { return result.BaselineState == BaselineState.New; },
                splittingStrategy: SplittingStrategy.OneFilePerRunPerRule);

            // 5. Now we have a set of SARIF files, each of which will drive creation of a single bug
            IEnumerable<WorkItemFilingMetadata> workItemsToFile = SarifLog.CreateWorkItemFilingMetadataFromLogs(sarifLogsToFile);

            AdoWorkItemFiler adoWorkItemFiler = new AdoWorkItemFiler()
            {
                FileAttachments = true
            };

            // 6. Wire up an event handler that gets called when the bug is filed
            //    This callback will retrieve the 
            adoWorkItemFiler.WorkItemFiledOrUpdated += (sender, eventArgs) =>
            {
                Dictionary<string, Result> guidToResultMap = guidMappingVisitor.GuidToResultMap;
                WorkItemFilingMetadata metadata = eventArgs.Metadata;

                Debug.Assert(eventArgs.Metadata.Attachments.Count == 1);
                var filedSarifLog = (SarifLog)eventArgs.Metadata.Attachments[0];

                Debug.Assert(filedSarifLog.Runs.Count == 1);
                foreach (Result result in filedSarifLog.Runs[0].Results)
                {
                    bool matched = guidToResultMap.TryGetValue(result.Guid, out Result originalResult);
                    Debug.Assert(matched);
                    originalResult.WorkItemUris = new Uri[] { metadata.Uri };
                }             
            };

            // 7. Do the work.
            try
            {
                adoWorkItemFiler.FileWorkItems(workItemsToFile).Wait();
            }
            catch (Exception exception) { LogFailure(exception); }

            // End mock up.
            return 0;
        }

        private static void LogFailure(Exception exception)
        {
            throw new NotImplementedException();
        }

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
        private static SarifLog RetrieveCurrentSarif()
        {
            string sarifText = GetResourceText("Sarif.Sdk.Sample.SampleTestFiles.Current.sarif");
            return JsonConvert.DeserializeObject<SarifLog>(sarifText);
        }

        private static SarifLog RetrieveBaselineSarif()
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
