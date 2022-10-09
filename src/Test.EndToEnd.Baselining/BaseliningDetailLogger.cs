// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;

using SarifBaseline.Extensions;

namespace Test.EndToEnd.Baselining
{
    /// <summary>
    ///  Write a detail log for a single log series, with one section per SarifLog
    ///  and one line per Result indicating whether the Result matched and to which
    ///  other Result.
    ///
    ///  The output contains Result GUIDs (replaced by RIDs) only.
    ///  This log is used to compare a current and previous baselining run in detail
    ///  and pass or fail the test run.
    /// </summary>
    public class BaseliningDetailLogger : IDisposable
    {
        private StreamWriter Writer { get; set; }

        public BaseliningDetailLogger(string seriesPath, Stream outputStream)
        {
            Writer = new StreamWriter(outputStream);
            Writer.WriteLine(RelativeSeriesPath(seriesPath));
        }

        private static string RelativeSeriesPath(string seriesPath)
        {
            string inputFolderSnippet = $"\\{BaseliningTester.InputFolderName}\\";
            int indexOfInputFolder = seriesPath.IndexOf(inputFolderSnippet);

            if (indexOfInputFolder > 0)
            {
                return seriesPath.Substring(indexOfInputFolder + inputFolderSnippet.Length);
            }
            else
            {
                return seriesPath;
            }
        }

        public void Write(SarifLog newBaselineLog, SarifLog baselineLog, BaseliningSummary summary)
        {
            Dictionary<Guid?, Result> baselineResultsByGuid = new Dictionary<Guid?, Result>();
            foreach (Result result in baselineLog.EnumerateResults())
            {
                baselineResultsByGuid[result.CorrelationGuid ?? result.Guid] = result;
            }

            Writer.WriteLine();
            Writer.WriteLine($"   {summary}");

            foreach (Result result in newBaselineLog.EnumerateResults())
            {
                switch (result.BaselineState)
                {
                    case BaselineState.Absent:
                        Write('-', result.Guid?.ToString());
                        break;
                    case BaselineState.New:
                        Write('+', result.Guid?.ToString());
                        break;
                    case BaselineState.Unchanged:
                    case BaselineState.Updated:
                        // Find and write old Result from previous Baseline (to get pre-merged properties)
                        Result previousResult = null;
                        baselineResultsByGuid.TryGetValue(result.CorrelationGuid, out previousResult);

                        if (previousResult == null)
                        {
                            // Write '?' for the Previous if we couldn't look it up
                            Write('=', result.Guid?.ToString());
                            Write('?', result.CorrelationGuid?.ToString());
                        }
                        else if (result.Guid != previousResult.Guid)
                        {
                            // Only Log Unchanged results from the latest log (with a new Guid)
                            Write('=', result.Guid?.ToString());
                            Write(' ', previousResult.Guid?.ToString());
                        }

                        break;
                }
            }
        }

        private void Write(char marker, string guid)
        {
            Writer.WriteLine($"      {marker} {guid}");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true && Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
        }
    }
}
