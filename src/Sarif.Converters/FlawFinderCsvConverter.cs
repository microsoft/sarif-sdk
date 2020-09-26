// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderCsvConverter : ToolFileConverterBase
    {
        private const string PartialFingerprintKey = "matchHash";

        public override string ToolName => "FlawFinder";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            using var textReader = new StreamReader(input);
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);

            IList<FlawFinderCsvResult> flawFinderCsvResults = csvReader.GetRecords<FlawFinderCsvResult>().ToList();

            IList <ReportingDescriptor> rules = ExtractRules(flawFinderCsvResults);
            IList<Result> results = ExtractResults(flawFinderCsvResults);

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        Rules = rules
                    }
                },
                Results = results
            };

            PersistResults(output, results, run);
        }

        private static IList<ReportingDescriptor> ExtractRules(IList<FlawFinderCsvResult> flawFinderCsvResults)
        {
            var rules = new List<ReportingDescriptor>();
            var ruleIds = new HashSet<string>();

            foreach (FlawFinderCsvResult flawFinderCsvResult in flawFinderCsvResults)
            {
                if (!ruleIds.Contains(flawFinderCsvResult.CWEs))
                {
                    rules.Add(SarifRuleFromFlawFinderCsvResult(flawFinderCsvResult));
                    ruleIds.Add(flawFinderCsvResult.CWEs);
                }
            }

            return rules;
        }

        private static IList<Result> ExtractResults(IList<FlawFinderCsvResult> flawFinderCsvResults) =>
            flawFinderCsvResults.Select(SarifResultFromFlawFinderCsvResult).ToList();

        private static ReportingDescriptor SarifRuleFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            new ReportingDescriptor
            {
                Id = flawFinderCsvResult.CWEs,
                ShortDescription = new MultiformatMessageString
                {
                    Text = flawFinderCsvResult.Warning
                },
                DefaultConfiguration = new ReportingConfiguration
                {
                    Level = SarifLevelFromFlawFinderLevel(flawFinderCsvResult.Level)
                }
            };

        private static Result SarifResultFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult)
        {
            var result = new Result
            {
                RuleId = flawFinderCsvResult.CWEs,
                Message = new Message
                {
                    Text = flawFinderCsvResult.Warning
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
                                Uri = new Uri(flawFinderCsvResult.File, UriKind.RelativeOrAbsolute)
                            },
                            Region = RegionFromFlawFinderCsvResult(flawFinderCsvResult)
                        }
                    }
                },
                PartialFingerprints = new Dictionary<string, string>
                {
                    [PartialFingerprintKey] = flawFinderCsvResult.Fingerprint
                }
            };


            result.SetProperty(
                nameof(flawFinderCsvResult.Level),
                flawFinderCsvResult.Level.ToString(CultureInfo.InvariantCulture));

            return result;
        }

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

        private static FailureLevel SarifLevelFromFlawFinderLevel(int flawFinderLevel) =>
                flawFinderLevel< 4 ? FailureLevel.Warning : FailureLevel.Error;
    }
}
