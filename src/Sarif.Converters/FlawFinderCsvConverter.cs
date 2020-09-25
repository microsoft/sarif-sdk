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
        public override string ToolName => "FlawFinder";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            using var textReader = new StreamReader(input);
            using var csvReader = new CsvReader(textReader, CultureInfo.InvariantCulture);
            IList<FlawFinderCsvResult> flawFinderResults =
                csvReader.GetRecords<FlawFinderCsvResult>().ToList();

            (IList<Result> results, IList<ReportingDescriptor> rules) = ExtractResultsAndRules(flawFinderResults);

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

            // T
            PersistResults(output, results, run);
        }

        private static (IList<Result>, IList<ReportingDescriptor>) ExtractResultsAndRules(IList<FlawFinderCsvResult> flawFinderCsvResults)
        {
            var results = new List<Result>();
            var rules = new List<ReportingDescriptor>();

            foreach (FlawFinderCsvResult flawFinderCsvResult in flawFinderCsvResults)
            {
                Result result = SarifResultFromFlawFinderCsvResult(flawFinderCsvResult);

                result.SetProperty(
                    nameof(flawFinderCsvResult.Level),
                    flawFinderCsvResult.Level.ToString(CultureInfo.InvariantCulture));

                results.Add(result);
            }

            return (results, null);
        }

        private static Result SarifResultFromFlawFinderCsvResult(FlawFinderCsvResult flawFinderCsvResult) =>
            new Result
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
                            Region = new Region
                            {
                                StartLine = flawFinderCsvResult.Line,
                                StartColumn = flawFinderCsvResult.Column
                            }
                        }
                    }
                }
            };

        private static FailureLevel SarifLevelFromFlawFinderLevel(int flawFinderLevel) =>
            flawFinderLevel< 4 ? FailureLevel.Warning : FailureLevel.Error;
    }
}
