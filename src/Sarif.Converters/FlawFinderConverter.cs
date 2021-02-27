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

        private const string UriBaseIdString = "SRCROOT";

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
                    Level = SarifLevelFromFlawFinderLevel(flawFinderCsvResult.DefaultLevel),
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
                Rank = SarifRankFromFlawFinderLevel(flawFinderCsvResult.Level),
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

            return result;
        }

        private static string RuleIdFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            $"{flawFinderCsvResult.RuleId}/{flawFinderCsvResult.Name}";

        // keep same format as html report
        private static string MessageFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            $"{flawFinderCsvResult.Category}/{flawFinderCsvResult.Name}: {flawFinderCsvResult.Warning}";

        private static Region RegionFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            new Region
            {
                StartLine = flawFinderCsvResult.Line,
                StartColumn = flawFinderCsvResult.Column,
                EndColumn = flawFinderCsvResult.Context.Length + 1,
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

        private static double SarifRankFromFlawFinderLevel(int flawFinderLevel) =>
            /*
            SARIF rank  FF Level    SARIF level Default Viewer Action
            0.0         0           note        Does not display by default
            0.2         1           note        Does not display by default
            0.4         2           note        Does not display by default
            0.6         3           warning     Displays by default, does not break build / other processes
            0.8         4           error       Displays by default, breaks build/ other processes
            1.0         5           error       Displays by default, breaks build/ other processes
            */
            // round to 1 decimal to avoid value like 0.60000000000000009
            Math.Round(flawFinderLevel * 0.2, 1);

        private static string AppendPeriod(string text) =>
            text.EndsWith(PeriodString, StringComparison.OrdinalIgnoreCase) ? text : text + PeriodString;
    }
}
