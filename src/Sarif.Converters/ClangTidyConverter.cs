// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel;

using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class ClangTidyConverter : ToolFileConverterBase
    {
        private const string ToolInformationUri = "https://clang.llvm.org/extra/clang-tidy/";

        public override string ToolName => "Clang-Tidy";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            var yaml = new YamlStream();
            IDeserializer deserializer = new DeserializerBuilder().Build();
            using var textReader = new StreamReader(input);
            ClangTidyLog order = deserializer.Deserialize<ClangTidyLog>(textReader);

            var results = new List<Result>();
            foreach (ClangTidyDiagnostic entry in order.Diagnostics)
            {
                results.Add(CreateResult(entry));
            }

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        InformationUri = new Uri(ToolInformationUri),
                    }
                },
                Results = results,
            };

            PersistResults(output, results, run);
        }

        internal Result CreateResult(ClangTidyDiagnostic entry)
        {
            entry = entry ?? throw new ArgumentNullException(nameof(entry));

            Result result = new Result()
            {
                RuleId = entry.DiagnosticName,
                Message = new Message { Text = entry.DiagnosticMessage.Message },
                Kind = ResultKind.Fail
            };

            // no level infomation in Clang-Tidy report
            result.Level = FailureLevel.Warning;

            Region region = new Region()
            {
                CharOffset = entry.DiagnosticMessage.FileOffset,
            };

            Uri analysisTargetUri = new Uri(entry.DiagnosticMessage.FilePath, UriKind.RelativeOrAbsolute);

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

            if (entry.DiagnosticMessage.Replacements.Count > 0)
            {
                IList<Replacement> replacements = new List<Replacement>();

                foreach (ClangTidyReplacement fix in entry.DiagnosticMessage.Replacements)
                {
                    Replacement replacement = new Replacement();

                    replacement.DeletedRegion = new Region
                    {
                        CharLength = fix.Length,
                        CharOffset = fix.Offset,
                    };

                    if (!string.IsNullOrEmpty(fix.ReplacementText))
                    {
                        replacement.InsertedContent = new ArtifactContent
                        {
                            Text = fix.ReplacementText
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
