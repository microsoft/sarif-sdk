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
    public partial class Program
    {
        private static int Main(string[] args)
        {
            // Mock up of end-to-end API use.
            
            // 1. Retrieve the current SARIF and its baseline equivalent
            SarifLog baselineSarif = RetrieveBaselineSarif(args[0]);
            SarifLog currentSarif = RetrieveCurrentSarif(args[1]);
            string outputFilePath = args[2];

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

            // Save out our updated baselined file.
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(matchedSarif);

            // End mock up.
            return 0;
        }

        private static void LogFailure(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
