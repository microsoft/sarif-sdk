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

            //Kind & Level determine the status
            //Result: "fail": Level = Error, Kind = Fail
            //Result: "info|notchecked|pass|unknown": Level = None, Kind = Informational|NotApplicable|Pass|Review
            switch (rule.Result)
            {
                //PASS CASES ARE NOT INCLUDED IN THE RESULTS, AS MATCH FORWARD DOES NOT PRODUCE
                //THE CORRECT ABSENT / NEW STATES WHEN THEY EXIST
                // case "pass":
                //     result.Level = FailureLevel.None;
                //     result.Kind = ResultKind.Pass;
                //     break;
                case "fail":
                    result.Level = FailureLevel.Error;
                    result.Kind = ResultKind.Fail;
                    result.Rank = RankConstants.High;
                    break;
                case "notchecked":
                    result.Level = FailureLevel.None;
                    result.Kind = ResultKind.NotApplicable;
                    result.Rank = RankConstants.None;
                    break;
                case "informational":
                    result.Level = FailureLevel.None;
                    result.Kind = ResultKind.Informational;
                    result.Rank = RankConstants.None;
                    break;
                case "unknown":
                default:
                    result.Level = FailureLevel.Warning;
                    result.Kind = ResultKind.Fail;
                    result.Rank = RankConstants.Medium;
                    break;
            };

            //Set the unique fingerprint
            result.Fingerprints = new Dictionary<string, string>();
            result.Fingerprints.Add("0", HashUtilities.ComputeSha256HashValue(rule.RuleId).ToLower());

            return result;
        }
    }
}
