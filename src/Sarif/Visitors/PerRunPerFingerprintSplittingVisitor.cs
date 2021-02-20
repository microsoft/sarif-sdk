// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class PerRunPerFingerprintSplittingVisitor : SplittingVisitor
    {
        private Dictionary<string, Result> _fingerprintToResultMap;

        public PerRunPerFingerprintSplittingVisitor(Func<Result, bool> filteringStrategy = null) : base(filteringStrategy)
        {
        }

        public override Run VisitRun(Run node)
        {
            _fingerprintToResultMap = new Dictionary<string, Result>();
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
                sarifLog = new SarifLog()
                {
                    Runs = new[]
                    {
                        new Run
                        {
                            Tool = CurrentRun.Tool,
                            Invocations = CurrentRun.Invocations,
                            Results = new List<Result>()
                        },
                    }
                };
                SplitSarifLogs.Add(sarifLog);
            }
            else
            {
                sarifLog = SplitSarifLogs[0];
            }

            if (!_fingerprintToResultMap.TryGetValue(fingerprint, out Result result))
            {
                result = node;
                result.Locations ??= new List<Location>();
                _fingerprintToResultMap[fingerprint] = result;

                sarifLog.Runs[0].Results.Add(result);
            }
            else
            {
                if (node.Locations != null)
                {
                    var locations = result.Locations.ToList();
                    foreach (Location location in node.Locations)
                    {
                        locations.Add(location);
                    }
                    result.Locations = locations;
                }
            }

            return node;
        }
    }
}
