// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FlawFinderCsvConverter : ToolFileConverterBase
    {
        private const string ExpectedHeader = "File,Line,Column,Level,Category,Name,Warning,Suggestion,Note,CWEs,Context,Fingerprint";

        public override string ToolName => "FlawFinder";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            using var textReader = new StreamReader(input);
            var parser = new CsvParser(textReader, CultureInfo.InvariantCulture);

            EnsureHeader(parser);

            IList<FlawFinderCsvResult> flawFinderResults = GetFlawFinderCsvResults();

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

        private void EnsureHeader(CsvParser parser)
        {
            string[] headerFields = parser.Read();
            if (headerFields == null)
            {
                throw new InvalidDataException(ConverterResources.FlawFinderMissingCsvHeader);
            }

            string headerLine = string.Join(",", headerFields);
            if (!headerLine.Equals(ExpectedHeader, StringComparison.Ordinal))
            {
                throw new InvalidDataException(ConverterResources.FlawFinderInvalidCsvHeader);
            }
        }

        private IList<FlawFinderCsvResult> GetFlawFinderCsvResults()
        {
            return new List<FlawFinderCsvResult>();
        }

        private IList<Result> ExtractResults(IList<FlawFinderCsvResult> _)
        {
            return new List<Result>();
        }

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
