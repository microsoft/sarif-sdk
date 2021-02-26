// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using CsvHelper;

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderConverter : ToolFileConverterBase
    {
        private const string FingerprintKey = "contextHash/v1";

        private const string PeriodString = ".";

        private const string UriBaseIdString = "REPO_ROOT";

        private const string ToolInformationUri = "https://dwheeler.com/flawfinder/";

        public override string ToolName => "Flawfinder";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            using var textReader = new StreamReader(input);
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);

            IList<FlawFinderCsvResult> flawFinderCsvResults = csvReader.GetRecords<FlawFinderCsvResult>().ToList();

            IList<ReportingDescriptor> rules = ExtractRules(flawFinderCsvResults);
            IList<Result> results = ExtractResults(flawFinderCsvResults);

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        Version = flawFinderCsvResults?.FirstOrDefault()?.ToolVersion,
                        InformationUri = new Uri(ToolInformationUri),
                        Rules = rules,
                    }
                },
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>() { { UriBaseIdString, new ArtifactLocation { Uri = null } } },
                Results = results,
            };

            PersistResults(output, results, run);
        }

        internal new void PersistResults(IResultLogWriter output, IList<Result> results, Run run)
        {
            output.Initialize(run);

            run.Results = results;

            if (run.Results != null)
            {
                output.OpenResults();
                output.WriteResults(run.Results);
                output.CloseResults();
            }
        }

        private static IList<ReportingDescriptor> ExtractRules(IList<FlawFinderCsvResult> flawFinderCsvResults)
        {
            var rules = new List<ReportingDescriptor>();
            var ruleIds = new HashSet<string>();

            foreach (FlawFinderCsvResult flawFinderCsvResult in flawFinderCsvResults)
            {
                string ruleId = RuleIdFromFlawFinderCsvResult(flawFinderCsvResult);
                if (!ruleIds.Contains(ruleId))
                {
                    rules.Add(SarifRuleFromFlawFinderCsvResult(flawFinderCsvResult));
                    ruleIds.Add(ruleId);
                }
            }

            return rules;
        }

        private static IList<Result> ExtractResults(IList<FlawFinderCsvResult> flawFinderCsvResults) =>
            flawFinderCsvResults.Select(SarifResultFromFlawFinderCsvResult).ToList();

        private static ReportingDescriptor SarifRuleFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            new ReportingDescriptor
            {
                Id = RuleIdFromFlawFinderCsvResult(flawFinderCsvResult),
                ShortDescription = new MultiformatMessageString
                {
                    Text = AppendPeriod(flawFinderCsvResult.Warning),
                },
                DefaultConfiguration = new ReportingConfiguration
                {
                    Level = SarifLevelFromFlawFinderLevel(flawFinderCsvResult.Level),
                },
                HelpUri = new Uri(flawFinderCsvResult.HelpUri),
            };

        private static Result SarifResultFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult)
        {
            var result = new Result
            {
                RuleId = RuleIdFromFlawFinderCsvResult(flawFinderCsvResult),
                Message = new Message
                {
                    Text = AppendPeriod(MessageFromFlawFinderCsvResult(flawFinderCsvResult)),
                },
                Level = SarifLevelFromFlawFinderLevel(flawFinderCsvResult.Level),
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(UriHelper.MakeValidUri(flawFinderCsvResult.File), UriKind.RelativeOrAbsolute),
                                UriBaseId = UriBaseIdString,
                            },
                            Region = RegionFromFlawFinderCsvResult(flawFinderCsvResult),
                        },
                    },
                },
                Fingerprints = new Dictionary<string, string>
                {
                    [FingerprintKey] = flawFinderCsvResult.Fingerprint,
                },
            };

            result.SetProperty(
                nameof(flawFinderCsvResult.Level),
                flawFinderCsvResult.Level.ToString(CultureInfo.InvariantCulture));

            return result;
        }

        private static string RuleIdFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) => flawFinderCsvResult.RuleId;

        // keep same format as html report
        private static string MessageFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            $"({flawFinderCsvResult.Category}) {flawFinderCsvResult.Name}: {flawFinderCsvResult.Warning}{flawFinderCsvResult.Note}";

        private static Region RegionFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            new Region
            {
                StartLine = flawFinderCsvResult.Line,
                StartColumn = flawFinderCsvResult.Column,
                EndColumn = flawFinderCsvResult.Column + flawFinderCsvResult.Context.Length,
                Snippet = new ArtifactContent
                {
                    Text = flawFinderCsvResult.Context
                }
            };

        private static FailureLevel SarifLevelFromFlawFinderLevel(int flawFinderLevel)
        {
            // level 4 & 5
            if (flawFinderLevel >= 4)
            {
                return FailureLevel.Error;
            }

            // level 3
            if (flawFinderLevel == 3)
            {
                return FailureLevel.Warning;
            }

            // level 0 1 2
            return FailureLevel.Note;
        }

        private static string AppendPeriod(string text) =>
            text.EndsWith(PeriodString, StringComparison.OrdinalIgnoreCase) ? text : text + PeriodString;
    }
}
