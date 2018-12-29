// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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
        
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
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

            var visitor = new AddFileReferencesVisitor();
            visitor.VisitRun(run);

            foreach (Result result in results)
            {
                visitor.VisitResult(result);
            }

            if (run.Files != null && run.Files.Count > 0)
            {
                output.WriteFiles(run.Files);
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

                CharOffset = entry.StartPosition.Position
            };

            int length = entry.EndPosition.Position - entry.StartPosition.Position;
            region.CharLength = length > 0 ? length : 0;

            Uri analysisTargetUri = new Uri(entry.Name, UriKind.Relative);

            var physicalLocation = new PhysicalLocation
            {
                FileLocation = new FileLocation
                {
                    Uri = analysisTargetUri
                },
                Region = region
            };

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
                        CharLength = fix.InnerLength,
                        CharOffset = fix.InnerStart
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

                var sarifFileChange = new FileChange
                {
                    FileLocation = new FileLocation
                    {
                        Uri = analysisTargetUri
                    },
                    Replacements = replacements
                };

                Fix sarifFix = new Fix(description: null, fileChanges: new List<FileChange>() { sarifFileChange }, properties: null);
                result.Fixes = new List<Fix> { sarifFix };
            } 

            return result;
        }        
    }
}
