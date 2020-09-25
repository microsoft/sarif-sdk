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

            IList<Result> results = ExtractResults(flawFinderResults);
            IList<ReportingDescriptor> rules = ExtractRules(flawFinderResults);
            IList<Artifact> artifacts = ExtractArtifacts(results);

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
                Results = results,
                Artifacts = artifacts
            };

            PersistResults(output, results, run);
        }

        private IList<Result> ExtractResults(IList<FlawFinderCsvResult> flawFinderCsvResults) =>
            flawFinderCsvResults.Select(FlawFinderCsvResultToSarif).ToList();

        private static Result FlawFinderCsvResultToSarif(FlawFinderCsvResult flawFinderCsvResult) =>
            new Result
            {
                RuleId = flawFinderCsvResult.CWEs,
                Message = new Message
                {
                    Text = flawFinderCsvResult.Warning
                },
                Level = flawFinderCsvResult.Level < 4 ? FailureLevel.Warning : FailureLevel.Error
            };

        private IList<ReportingDescriptor> ExtractRules(IList<FlawFinderCsvResult> _)
        {
            return new List<ReportingDescriptor>();
        }

        private IList<Artifact> ExtractArtifacts(IList<Result> _)
        {
            return new List<Artifact>();
        }
    }
}
