using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace SarifBaseline.Extensions
{
    public static class ResultEnumeration
    {
        public static IEnumerable<Result> EnumerateResults(this SarifLog log)
        {
            if (log != null)
            {
                foreach (Run run in log.EnumerateRuns())
                {
                    foreach (Result result in run.EnumerateResults())
                    {
                        yield return result;
                    }
                }
            }
        }

        public static IEnumerable<Result> EnumerateResults(this Run run)
        {
            if (run?.Results != null)
            {
                foreach (Result result in run.Results)
                {
                    result.Run = run;
                    yield return result;
                }
            }
        }
    }
}
