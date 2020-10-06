// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BSOA.Benchmarks;

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Demo
{
    public class SarifLogBenchmarks
    {
        [Benchmark]
        public void IntegerGet(SarifLog log)
        {
            LineTotal(log);
        }

        [Benchmark]
        public void StringGet(SarifLog log)
        {
            MessageLengthTotal(log);
        }

        public long LineTotal(SarifLog log)
        {
            long lineTotal = 0;

            foreach (Run run in log.Runs)
            {
                foreach (Result result in run.Results)
                {
                    foreach (Location location in result.Locations)
                    {
                        lineTotal += location?.PhysicalLocation?.Region?.StartLine ?? 0;
                    }
                }
            }

            return lineTotal;
        }

        public long MessageLengthTotal(SarifLog log)
        {
            MessageVisitor v = new MessageVisitor();
            v.Visit(log);
            return v.TotalMessageLength;
        }
    }

    internal class MessageVisitor : SarifRewritingVisitor
    {
        public long TotalMessageLength { get; set; }

        public override Message VisitMessage(Message node)
        {
            TotalMessageLength += node?.Text?.Length ?? 0;
            return base.VisitMessage(node);
        }
    }
}
