// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Visitors;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    // Enrichment that synthesizes a contextRegion must not emit SARIF that the SDK's own SARIF1008
    // rule rejects. These tests run the real InsertOptionalDataVisitor enrichment path and then run
    // the validator over its output, closing the gap where enrichment was only ever checked against
    // serialized baselines that never invoked SARIF1008.
    public class ContextRegionSnippetValidationTests
    {
        private const string LineOf220 = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

        [Fact]
        public void InsertContextRegionSnippets_SmallRegion_EmitsValidContextRegion()
        {
            // Control: a small region in a multi-line file yields a real contextRegion (proving the
            // enrichment path actually executes in this harness) and validates cleanly.
            string fileContent = string.Join(Environment.NewLine, "alpha", "beta", "gamma", "delta", "epsilon");

            (Region contextRegion, IList<Result> sarif1008Results) =
                EnrichAndValidate(fileContent, new Region { StartLine = 3, EndLine = 3 });

            contextRegion.Should().NotBeNull();
            sarif1008Results.Should().BeEmpty();
        }

        [Fact]
        public void InsertContextRegionSnippets_LargeMultilineRegion_OmitsContextRegionAndValidates()
        {
            // Mode A: a multi-line region whose snippet exceeds the 512-character cap previously
            // produced a contextRegion identical to the region, failing SARIF1008. It is now omitted.
            string fileContent = string.Join(Environment.NewLine, LineOf220, LineOf220, LineOf220);

            (Region contextRegion, IList<Result> sarif1008Results) =
                EnrichAndValidate(fileContent, new Region { StartLine = 1, EndLine = 3 });

            contextRegion.Should().BeNull();
            sarif1008Results.Should().BeEmpty();
        }

        [Fact]
        public void InsertContextRegionSnippets_CharOffsetWindowCannotContainRegion_OmitsContextRegionAndValidates()
        {
            // Mode B: a long single-line region whose context falls back to a capped char-offset
            // window that runs off the end of the region previously emitted a contextRegion that did
            // not contain the region, failing SARIF1008. It is now omitted.
            string fileContent = new string('x', 1000);

            (Region contextRegion, IList<Result> sarif1008Results) =
                EnrichAndValidate(fileContent, new Region { CharOffset = 100, CharLength = 450 });

            contextRegion.Should().BeNull();
            sarif1008Results.Should().BeEmpty();
        }

        private static (Region contextRegion, IList<Result> sarif1008Results) EnrichAndValidate(string fileContent, Region region)
        {
            string sourcePath = Path.GetTempFileName();
            string inputPath = Path.GetTempFileName() + ".sarif";
            string outputPath = Path.GetTempFileName() + ".sarif";

            try
            {
                File.WriteAllText(sourcePath, fileContent);

                var run = new Run
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = "ContextRegionTest",
                            Rules = new[] { new ReportingDescriptor { Id = "TEST0001" } }
                        }
                    },
                    Results = new List<Result>
                    {
                        new Result
                        {
                            RuleId = "TEST0001",
                            Message = new Message { Text = "test" },
                            Locations = new List<Location>
                            {
                                new Location
                                {
                                    PhysicalLocation = new PhysicalLocation
                                    {
                                        ArtifactLocation = new ArtifactLocation { Uri = new Uri(sourcePath) },
                                        Region = region
                                    }
                                }
                            }
                        }
                    }
                };

                var visitor = new InsertOptionalDataVisitor(
                    OptionallyEmittedData.ContextRegionSnippets,
                    new FileRegionsCache(),
                    run,
                    insertProperties: null);

                visitor.VisitRun(run);

                Region contextRegion = run.Results[0].Locations[0].PhysicalLocation.ContextRegion;

                var log = new SarifLog { Runs = new[] { run } };
                log.Save(inputPath);

                var options = new ValidateOptions
                {
                    TargetFileSpecifiers = new[] { inputPath },
                    OutputFilePath = outputPath,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    RuleKindOption = new List<RuleKind> { RuleKind.Sarif },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Note, FailureLevel.Warning, FailureLevel.Error }
                };

                var context = new SarifValidationContext { FileSystem = FileSystem.Instance };
                new ValidateCommand().Run(options, ref context);

                SarifLog output = SarifLog.Load(outputPath);
                IList<Result> sarif1008Results = output.Runs[0].Results
                    .Where(r => r.RuleId == "SARIF1008")
                    .ToList();

                return (contextRegion, sarif1008Results);
            }
            finally
            {
                foreach (string path in new[] { sourcePath, inputPath, outputPath })
                {
                    if (File.Exists(path)) { File.Delete(path); }
                }
            }
        }
    }
}
