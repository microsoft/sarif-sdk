// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintConverter : ToolFileConverterBase
    {
        private readonly LogReader<TSLintLog> logReader;

        public TSLintConverter()
        {
            logReader = new TSLintLogReader();
        }

        public override string ToolName => ToolFormat.TSLint;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            TSLintLog tsLintLog = logReader.ReadLog(input);

            var results = new List<Result>();
            foreach (TSLintLogEntry entry in tsLintLog)
            {
                results.Add(CreateResult(entry));
            }

            PersistResults(output, results);
        }

        internal Result CreateResult(TSLintLogEntry entry)
        {
            entry = entry ?? throw new ArgumentNullException(nameof(entry));

            Result result = new Result()
            {
                RuleId = entry.RuleName,
                Message = new Message { Text = entry.Failure },
                Kind = ResultKind.Fail
            };

            switch (entry.RuleSeverity)
            {
                case "ERROR":
                    result.Level = FailureLevel.Error;
                    break;
                case "WARN":
                case "WARNING":
                    result.Level = FailureLevel.Warning;
                    break;
                case "DEFAULT":
                default:
                    result.Level = FailureLevel.Note;
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
                ArtifactLocation = new ArtifactLocation
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
                        replacement.InsertedContent = new ArtifactContent
                        {
                            Text = fix.InnerText
                        };
                    }

                    replacements.Add(replacement);
                }

                var sarifFileChange = new ArtifactChange
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = analysisTargetUri
                    },
                    Replacements = replacements
                };

                Fix sarifFix = new Fix(description: null, artifactChanges: new List<ArtifactChange>() { sarifFileChange }, properties: null);
                result.Fixes = new List<Fix> { sarifFix };
            }

            return result;
        }
    }
}
