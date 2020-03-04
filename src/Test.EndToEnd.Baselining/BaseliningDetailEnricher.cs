// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Sarif;

using SarifBaseline.Extensions;

namespace Test.EndToEnd.Baselining
{
    /// <summary>
    ///  Convert a detail log for a single log series, adding details to help human
    ///  investigation to each Result line.
    /// 
    ///  This log is used for human investigation of baselining algorithm differences
    ///  across runs. 
    ///  
    ///  The "enriched" logs are converted from the "canonical" detail logs after
    ///  the fact so that a developer debugging can change which Result details are
    ///  available for investigation without breaking existing test baselines.
    /// </summary>
    public class BaseliningDetailEnricher
    {
        private Dictionary<string, string> DetailByGuid { get; set; }
        private readonly Regex RidRegex = new Regex(@"(\d{3} \d{3,4})$", RegexOptions.Compiled | RegexOptions.Singleline);

        public BaseliningDetailEnricher()
        {
            DetailByGuid = new Dictionary<string, string>();
        }

        public void AddLog(SarifLog currentLog)
        {
            foreach (Result result in currentLog.EnumerateResults())
            {
                DetailByGuid[result.Guid] = Details(result);
            }
        }

        public void Convert(string sourcePath, string targetPath)
        {
            if (!File.Exists(sourcePath)) { return; }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            using (Stream source = File.OpenRead(sourcePath))
            using (Stream target = File.Create(targetPath))
            {
                Convert(source, target);
            }
        }

        public void Convert(Stream source, Stream target)
        {
            using (StreamReader reader = new StreamReader(source))
            using (StreamWriter writer = new StreamWriter(target))
            {
                Convert(reader, writer);
            }
        }

        public void Convert(StreamReader reader, StreamWriter writer)
        {
            // Series Path
            writer.WriteLine(reader.ReadLine());

            // Details column headings
            writer.WriteLine(DetailsHeading());

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                Match m = RidRegex.Match(line);
                if (m.Success)
                {
                    // Add details for every line with a RID
                    string rid = m.Groups[1].Value;
                    if (!DetailByGuid.TryGetValue(rid, out string detail)) { detail = ""; }
                    writer.Write(line);
                    writer.WriteLine(detail);
                }
                else
                {
                    // Otherwise, just echo it
                    writer.WriteLine(line);
                }
            }
        }

        // Customize as needed when debugging to include columns relevant to Result Matching in your scenario
        // (There are too many to include all of them all the time)
        private string DetailsHeading()
        {
            return $"        RID     | RuleID    | Uri+Region          | Hash     | Snippet";
        }

        private string Details(Result result)
        {
            return $" | {result.ResolvedRuleId(result.Run)} | {result.FirstLocation()} | {result.PartialFingerprint("SecretHash/v1", 8)} | {result.Snippet(16)}";
        }
    }
}
