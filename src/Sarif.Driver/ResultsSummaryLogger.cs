// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal class ResultsSummaryLogger : IAnalysisLogger, IDisposable
    {
        public FileRegionsCache FileRegionsCache { get; set; }

        public void AnalysisStarted()
        {
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
        }

        private Dictionary<string, Tuple<int, int, int>> rulesSummary;

        public void Log(ReportingDescriptor rule, Result result, int? extensionIndex = null)
        {
            string key = $"{rule.Id}.{rule.Name}";
            rulesSummary ??= new Dictionary<string, Tuple<int, int, int>>();
            rulesSummary.TryGetValue(key, out Tuple<int, int, int> tuple);
            tuple ??= new Tuple<int, int, int>(0, 0, 0);

            switch (result.Level)
            {
                case FailureLevel.Error:
                {
                    tuple = new Tuple<int, int, int>(tuple.Item1 + 1, tuple.Item2, tuple.Item3);
                    break;
                }
                case FailureLevel.Warning:
                {
                    tuple = new Tuple<int, int, int>(tuple.Item1 + 1, tuple.Item2, tuple.Item3);
                    break;
                }
                case FailureLevel.Note:
                {
                    tuple = new Tuple<int, int, int>(tuple.Item1 + 1, tuple.Item2, tuple.Item3);
                    break;
                }
            }

            rulesSummary[key] = tuple;
        }

        public void LogConfigurationNotification(Notification notification)
        {
        }

        public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule = null)
        {
        }

        public void TargetAnalyzed(IAnalysisContext context)
        {
        }

        public void Dispose()
        {
            if (this.rulesSummary == null) { return; }

            var aggregatedCounts = new Dictionary<string, long>();
            foreach (string ruleIdAndName in this.rulesSummary.Keys)
            {
                Tuple<int, int, int> tuple = this.rulesSummary[ruleIdAndName];
                long count = tuple.Item1 + tuple.Item2 + tuple.Item3;
                string line = $"{ruleIdAndName} : {count}";
                aggregatedCounts[line] = count;
            }

            IOrderedEnumerable<KeyValuePair<string, long>> sortedResults = from entry in aggregatedCounts orderby entry.Value descending select entry;
            foreach (KeyValuePair<string, long> line in sortedResults)
            {
                Console.WriteLine(line.Key);
            }
        }
    }
}
