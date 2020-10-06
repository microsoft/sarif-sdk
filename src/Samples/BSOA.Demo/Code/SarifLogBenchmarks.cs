// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BSOA.Benchmarks.Attributes;

using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Demo
{
    public static class SarifLogBenchmarks
    {
        [Benchmark]
        public static void IntegerGet(SarifLog log)
        {
            LineTotal(log);
        }

        [Benchmark]
        public static void StringGet(SarifLog log)
        {
            MessageLengthTotal(log);
        }

        public static long LineTotal(SarifLog log)
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

        public static long MessageLengthTotal(SarifLog log)
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
