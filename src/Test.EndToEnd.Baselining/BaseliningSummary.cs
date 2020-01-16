// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif;

using SarifBaseline.Extensions;

namespace Test.EndToEnd.Baselining
{
    /// <summary>
    ///  Write a summary of how baselining ran overall, with one line per series.
    ///  This log is to quickly compare how two implementations performed and to
    ///  direct further investigation.
    /// </summary>
    public struct BaseliningSummary
    {
        /// <summary>
        ///  A name for the set of things in this summary
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Any components within this baseline
        /// </summary>
        public List<BaseliningSummary> Components { get; set; }

        /// <summary>
        ///  ChurnCount is Min(NewResults, RemovedResult) per ArtifactUri.
        ///  It is the maximum number of undermatched Results there could be,
        ///  because there must be a New and Absent result which could have been matched.
        /// </summary>
        public int ChurnCount { get; set; }

        /// <summary>
        ///  ResultCount is the total number of Results in logs after the original Baseline.
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        ///  NewResultTotal is the total number of 'New' results after baselining,
        ///  and the number of unique issues that baselining a series detected.
        /// </summary>
        public int NewResultTotal { get; set; }

        public BaseliningSummary(string name)
        {
            this.Name = name;
            this.Components = null;
            this.ChurnCount = 0;
            this.ResultCount = 0;
            this.NewResultTotal = 0;
        }

        public void AddComponent(BaseliningSummary inner)
        {
            AddCounts(inner);

            Components = Components ?? new List<BaseliningSummary>();
            Components.Add(inner);
        }

        public void AddCounts(BaseliningSummary inner)
        {
            ChurnCount += inner.ChurnCount;
            ResultCount += inner.ResultCount;
            NewResultTotal += inner.NewResultTotal;
        }

        public void Add(SarifLog newBaselineLog, SarifLog baselineLog, SarifLog currentLog)
        {
            Dictionary<string, ResultCounts> newAndRemovedPerUri = new Dictionary<string, ResultCounts>();

            foreach (Result result in newBaselineLog.EnumerateResults())
            {
                // Track New+Absent results per distinct first Result Uri
                Uri first = result.AllArtifactUris().FirstOrDefault();
                ResultCounts counts = null;
                if (!newAndRemovedPerUri.TryGetValue(first?.OriginalString ?? "", out counts))
                {
                    counts = new ResultCounts();
                    newAndRemovedPerUri[first.OriginalString] = counts;
                }

                counts.Add(result);
            }

            ChurnCount += newAndRemovedPerUri.Values.Sum(counts => counts.ChurnCount);
            NewResultTotal += newAndRemovedPerUri.Values.Sum(counts => counts.NewCount);
            ResultCount += currentLog.EnumerateResults().Count();
        }

        private class ResultCounts
        {
            public int NewCount { get; set; }
            public int AbsentCount { get; set; }
            public int ChurnCount => Math.Min(NewCount, AbsentCount);

            public void Add(Result result)
            {
                if (result.BaselineState == BaselineState.New)
                {
                    NewCount++;
                }
                else if (result.BaselineState == BaselineState.Absent)
                {
                    AbsentCount++;
                }
            }
        }

        public override string ToString()
        {
            return ToString(Name);
        }

        public string ToString(string nameToUse)
        {
            return $"{nameToUse}   [{ChurnCount:n0} / {ResultCount:n0} churn, {NewResultTotal:n0} new]";
        }

        public void Write(StreamWriter writer, int indent = 0)
        {
            writer.Write(new string('\t', indent));
            writer.WriteLine(this.ToString());

            if (this.Components != null)
            {
                foreach (BaseliningSummary component in this.Components)
                {
                    component.Write(writer, indent + 1);
                }
            }
        }
    }
}
