// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Converters.PylintObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class PylintConverter : ToolFileConverterBase
    {
        private readonly LogReader<PylintLog> logReader;

        public PylintConverter()
        {
            logReader = new PylintLogReader();
        }

        public override string ToolName => ToolFormat.Pylint;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            PylintLog log = logReader.ReadLog(input);

            var results = new List<Result>();

            foreach (PylintLogEntry entry in log)
            {
                results.Add(CreateResult(entry));
            }

            PersistResults(output, results);
        }

        internal Result CreateResult(PylintLogEntry defect)
        {
            defect = defect ?? throw new ArgumentNullException(nameof(defect));

            Result result = new Result
            {
                RuleId = $"{defect.MessageId}({defect.Symbol})",
                Message = new Message { Text = defect.Message },
            };

            switch (defect.Type)
            {
                case "error":
                case "fatal":
                    result.Level = FailureLevel.Error;
                    break;
                case "warning":
                case "convention":
                    result.Level = FailureLevel.Warning;
                    break;
                case "refactor":
                default:
                    result.Level = FailureLevel.Note;
                    break;
            };

            var region = new Region
            {
                StartColumn = int.Parse(defect.Column),
                StartLine = int.Parse(defect.Line)
            };

            var fileUri = new Uri($"{defect.FilePath}", UriKind.RelativeOrAbsolute);
            var physicalLocation = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = fileUri
                },
                Region = region
            };

            var location = new Location
            {
                PhysicalLocation = physicalLocation
            };

            result.Locations = new List<Location>
            {
                location
            };

            return result;
        }
    }
}
