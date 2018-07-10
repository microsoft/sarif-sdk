// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverter : ToolFileConverterBase
    {
        private readonly LogReader<TSLintLog> logReader;
        
        public TSLintConverter()
        {
            logReader = new TSLintLogReader();
        }
        
        public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));

            output = output ?? throw new ArgumentNullException(nameof(output));

            TSLintLog tsLintLog = logReader.ReadLog(input);

            Tool tool = new Tool
            {
                Name = "TSLint"
            };

            var run = new Run()
            {
                Tool = tool
            };

            output.Initialize(run);

            var results = new List<Result>();
            foreach(TSLintLogEntry entry in tsLintLog)
            {
                results.Add(CreateResult(entry));
            }

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, loggingOptions);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);
            
            if (fileDictionary?.Any() == true)
            {
                output.WriteFiles(fileDictionary);
            }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        internal Result CreateResult(TSLintLogEntry entry)
        {
            entry = entry ?? throw new ArgumentNullException(nameof(entry));

            Result result = new Result()
            {
                RuleId = entry.RuleName,
                Message = new Message { Text = entry.Failure }
            };

            switch (entry.RuleSeverity)
            {
                case "ERROR":
                    result.Level = ResultLevel.Error;
                    break;
                case "WARN":
                case "WARNING":
                    result.Level = ResultLevel.Warning;
                    break;
                case "DEFAULT":
                default:
                    result.Level = ResultLevel.Note;
                    break;
            }

            Region region = new Region()
            {
                // The TSLint logs have line and column start at 0, Sarif has them starting at 1, so add 1 to each
                StartLine = entry.StartPosition.Line + 1,
                StartColumn = entry.StartPosition.Character + 1,
                EndLine = entry.EndPosition.Line + 1,
                EndColumn = entry.EndPosition.Character + 1,

                ByteOffset = entry.StartPosition.Position
            };

            int length = entry.EndPosition.Position - entry.StartPosition.Position + 1;
            region.ByteLength = length > 0 ? length : 0;

            string analysisTargetPath = UriHelper.MakeValidUri(entry.Name);

            var physicalLocation = new PhysicalLocation(id: 0, fileLocation: new FileLocation(uri: analysisTargetPath, uriBaseId: null), region: region, contextRegion: null);
            Location location = new Location()
            {
                PhysicalLocation = physicalLocation
            };

            result.Locations = new List<Location>()
            {
                location
            };

            if (entry.Fixes?.Any() == true)
            {
                IList<Replacement> replacements = new List<Replacement>();

                foreach (TSLintLogFix fix in entry.Fixes)
                {
                    Replacement replacement = new Replacement();

                    replacement.DeletedRegion = new Region
                    {
                        ByteLength = fix.InnerLength,
                        ByteOffset = fix.InnerStart
                    };

                    if (!string.IsNullOrEmpty(fix.InnerText))
                    {
                        replacement.InsertedContent = new FileContent
                        {
                            Text = fix.InnerText
                        };
                    }

                    replacements.Add(replacement);
                }

                FileChange sarifFileChange = new FileChange(fileLocation: new FileLocation(uri: analysisTargetPath, uriBaseId: null), replacements: replacements);

                Fix sarifFix = new Fix(description: null, fileChanges: new List<FileChange>() { sarifFileChange });
                result.Fixes = new List<Fix> { sarifFix };
            } 

            return result;
        }        
    }
}
