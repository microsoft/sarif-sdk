// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Writers;
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

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            PylintLog log = logReader.ReadLog(input);

            Tool tool = new Tool
            {
                Name = "Pylint"
            };

            var run = new Run()
            {
                Tool = tool
            };

            output.Initialize(run);

            var results = new List<Result>();

            foreach (PylintLogEntry entry in log)
            {
                results.Add(CreateResult(entry));
            }

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, dataToInsert);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            if (fileDictionary?.Any() == true)
            {
                output.WriteFiles(fileDictionary);
            }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        internal Result CreateResult(PylintLogEntry defect)
        {
            defect = defect ?? throw new ArgumentNullException(nameof(defect));

            Result result = new Result
            {
                RuleId = $"{defect.MessageId}({defect.Symbol})",
                Message = new Message { Text = defect.Message }
            };

            switch (defect.Type)
            {
                case "error":
                case "fatal":
                    result.Level = ResultLevel.Error;
                    break;
                case "warning":
                case "convention":
                    result.Level = ResultLevel.Warning;
                    break;
                case "refactor":
                default:
                    result.Level = ResultLevel.Note;
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
                FileLocation = new FileLocation
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
