// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Newtonsoft.Json;

namespace Sarif.WorkItems.Sample
{
    public class Program
    {
        private static int Main(string[] args)
        {
            NonFunctioningEndToEndApiUsageExample();
            return 0;
        }

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
        #endregion Stubs
    }
}
