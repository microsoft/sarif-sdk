// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Converters.CisCatObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CisCatConverter : ToolFileConverterBase
    {
        private readonly LogReader<CisCatReport> logReader;

        public CisCatConverter()
        {
            logReader = new CisCatReportReader();
        }

        public override string ToolName => ToolFormat.CisCat;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            //Read CIS CAT data
            CisCatReport log = logReader.ReadLog(input);

            //Top level run object for the scan data
            var run = new Run();

            //Set the tool details
            run.Tool = new Tool();
            run.Tool.Driver = CreateDriver(log);

            //Set the list of tool rules
            run.Tool.Driver.Rules = new List<ReportingDescriptor>();
            foreach (CisCatRule rule in log.Rules)
            {
                run.Tool.Driver.Rules.Add(CreateReportDescriptor(rule));
            }

            var results = new List<Result>();
            foreach (CisCatRule rule in log.Rules.Where(i => !i.IsPass()))
            {
                results.Add(CreateResult(rule));
            }

            PersistResults(output, results, run);
        }

        internal ToolComponent CreateDriver(CisCatReport report)
        {

            var driver = new ToolComponent();

            driver.Name = this.ToolName;
            driver.FullName = report.BenchmarkTitle;
            driver.Version = report.BenchmarkVersion;
            driver.SemanticVersion = report.BenchmarkVersion;
            driver.InformationUri = new Uri("https://www.cisecurity.org/cybersecurity-tools/cis-cat-pro_pre");

            driver.SetProperty("benchmarkId", report.BenchmarkId);
            driver.SetProperty("profileId", report.ProfileId);
            driver.SetProperty("profileTitle", report.ProfileTitle);
            driver.SetProperty("score", report.Score);

            return driver;
        }

        internal ReportingDescriptor CreateReportDescriptor(CisCatRule rule)
        {
            ReportingDescriptor descriptor = new ReportingDescriptor();

            descriptor.Id = rule.RuleId;
            descriptor.Name = rule.RuleTitle;
            descriptor.ShortDescription = new MultiformatMessageString()
            {
                Text = rule.RuleTitle,
                Markdown = rule.RuleTitle,
            };
            descriptor.FullDescription = new MultiformatMessageString()
            {
                Text = rule.RuleTitle,
                Markdown = rule.RuleTitle,
            };
            descriptor.Help = new MultiformatMessageString()
            {
                Text = rule.RuleTitle,
                Markdown = rule.RuleTitle,
            };

            //Use for GH Security Advisories
            //set result level and rank (Critical - Low risk rating)
            FailureLevel level = FailureLevel.None;
            ResultKind kind = ResultKind.None;
            double rank = RankConstants.None;
            getResultSeverity(rule.Result, out level, out kind, out rank);

            //Create only if a valid is assigned
            if (rank != RankConstants.None)
            {
                descriptor.SetProperty("security-severity", rank.ToString("F1"));
            }

            //Tags for GH filtering
            var tags = new List<string>()
            {
                "security",
            };

            descriptor.SetProperty("tags", tags);

            return descriptor;
        }

        internal Result CreateResult(CisCatRule rule)
        {
            //set the result metadata
            Result result = new Result
            {
                RuleId = rule.RuleId,
                Message = new Message { Text = rule.RuleTitle },
            };

            //set result kind, level and rank (Critical - Low risk rating)
            FailureLevel level = FailureLevel.None;
            ResultKind kind = ResultKind.None;
            double rank = RankConstants.None;
            getResultSeverity(rule.Result, out level, out kind, out rank);

            //Set result object data
            result.Level = level;
            result.Kind = kind;
            result.Rank = rank;

            //Set the unique fingerprint
            result.Fingerprints = new Dictionary<string, string>();
            result.Fingerprints.Add("0", HashUtilities.ComputeSha256HashValue(rule.RuleId).ToLower());

            return result;
        }

        private void getResultSeverity(string result, out FailureLevel level, out ResultKind kind, out double rank)
        {
            // Default values
            level = FailureLevel.None;
            kind = ResultKind.None;
            rank = RankConstants.None;

            //Kind & Level determine the status
            //Result: "fail": Level = Error, Kind = Fail
            //Result: "info|notchecked|pass|unknown": Level = None, Kind = Informational|NotApplicable|Pass|Review
            switch (result)
            {
                case "pass":
                    level = FailureLevel.None;
                    kind = ResultKind.Pass;
                    rank = RankConstants.None;
                    break;
                case "fail":
                    level = FailureLevel.Error;
                    kind = ResultKind.Fail;
                    rank = RankConstants.High;
                    break;
                case "notchecked":
                    level = FailureLevel.None;
                    kind = ResultKind.NotApplicable;
                    rank = RankConstants.None;
                    break;
                case "informational":
                    level = FailureLevel.None;
                    kind = ResultKind.Informational;
                    rank = RankConstants.None;
                    break;
                case "unknown":
                default:
                    level = FailureLevel.Warning;
                    kind = ResultKind.Fail;
                    rank = RankConstants.Medium;
                    break;
            };
        }
    }
}
