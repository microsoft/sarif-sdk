// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerFingerprintSplittingVisitor : SplittingVisitor
    {
        private Dictionary<string, Run> _fingerprintToRunMap;

        public PerRunPerFingerprintSplittingVisitor(Func<Result, bool> filteringStrategy = null) : base(filteringStrategy)
        {
        }

        public override Run VisitRun(Run node)
        {
            _fingerprintToRunMap = new Dictionary<string, Run>();
            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            if (!FilteringStrategy(node)) { return node; }

            string fingerprint = node.Fingerprints.Any()
                ? node.Fingerprints.First().Value
                : string.Empty;

            SarifLog sarifLog;
            if (SplitSarifLogs.Count == 0)
            {
                sarifLog = new SarifLog() { Runs = new List<Run>() };
                SplitSarifLogs.Add(sarifLog);
            }
            else
            {
                sarifLog = SplitSarifLogs[0];
            }

            if (!_fingerprintToRunMap.TryGetValue(fingerprint, out Run run))
            {
                run = new Run
                {
                    Tool = CurrentRun.Tool,
                    Invocations = CurrentRun.Invocations,
                    Results = new List<Result>()
                };
                sarifLog.Runs.Add(run);
                _fingerprintToRunMap[fingerprint] = run;
            }

            run.Results.Add(node);

            return node;
        }
    }
}
