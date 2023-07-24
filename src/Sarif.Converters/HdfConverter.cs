// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Converters.HdfModel;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class HdfConverter : ToolFileConverterBase
    {
        private const string PeriodString = ".";

        private const string ToolInformationUri = "https://saf.cms.gov/#/normalize";

        public override string ToolName => "Heimdall Tools";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            using var textReader = new StreamReader(input);
            string jsonString = textReader.ReadToEnd();
            var hdfFile = HdfFile.FromJson(jsonString);

            (List<ReportingDescriptor>, List<Result>) rulesAndResults = ExtractRulesAndResults(hdfFile);

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = hdfFile.Platform.Name,
                        Version = hdfFile.Version,
                        InformationUri = new Uri(ToolInformationUri),
                        Rules = rulesAndResults.Item1,
                        SupportedTaxonomies = new List<ToolComponentReference>() { new ToolComponentReference() { Name = "NIST SP800-53 v5", Guid = Guid.Parse("AAFBAB93-5201-419E-8443-D4925C542398") } }
                    }
                },
                OriginalUriBaseIds = new Dictionary<string, ArtifactLocation>()
                {
                    {
                        "ROOTPATH",  new ArtifactLocation {
                            Uri = new Uri("file:///")
                        }
                    }
                },
                ExternalPropertyFileReferences = new ExternalPropertyFileReferences()
                {
                    Taxonomies = new List<ExternalPropertyFileReference>()
                    {
                        new ExternalPropertyFileReference()
                        {
                            Location = new ArtifactLocation()
                            {
                                Uri = new Uri("https://raw.githubusercontent.com/sarif-standard/taxonomies/main/NIST_SP800-53_v5.sarif"),
                            },
                            Guid = Guid.Parse("AAFBAB93-5201-419E-8443-D4925C542398")
                        }
                    }
                },
                Results = rulesAndResults.Item2,
            };

            PersistResults(output, rulesAndResults.Item2, run);
        }

        private static (List<ReportingDescriptor>, List<Result>) ExtractRulesAndResults(HdfFile hdfFile)
        {
            var rules = new List<ReportingDescriptor>();
            var ruleIds = new HashSet<string>();
            var results = new List<Result>();

            foreach (ExecJsonControl execJsonControl in hdfFile.Profiles.SelectMany(p => p.Controls))
            {
                string ruleId = execJsonControl.Id;
                if (!ruleIds.Contains(ruleId))
                {
                    (ReportingDescriptor, IList<Result>) ruleAndResult = SarifRuleAndResultFromHdfControl(execJsonControl);
                    rules.Add(ruleAndResult.Item1);
                    ruleIds.Add(ruleId);
                    results.AddRange(ruleAndResult.Item2);
                }
            }

            return (rules, results);
        }

        private static (ReportingDescriptor, IList<Result>) SarifRuleAndResultFromHdfControl(ExecJsonControl execJsonControl)
        {
            var reportingDescriptor = new ReportingDescriptor
            {
                Id = execJsonControl.Id,
                Name = execJsonControl.Title,
                ShortDescription = new MultiformatMessageString
                {
                    Text = AppendPeriod(execJsonControl.Title),
                },
                FullDescription = new MultiformatMessageString
                {
                    Text = AppendPeriod(execJsonControl.Desc),
                },
                DefaultConfiguration = new ReportingConfiguration
                {
                    Level = SarifLevelFromHdfImpact(execJsonControl.Impact),
                },
                Help = execJsonControl.Descriptions.Any() ? new MultiformatMessageString
                {
                    Text = string.Join("\n", execJsonControl.Descriptions.Select(d => d.Label + ":\n" + d.Data))
                } : null,
                HelpUri = null,
                Relationships = new List<ReportingDescriptorRelationship>(
                    ((JArray)execJsonControl.Tags["nist"])
                    .Select(p => p.ToString().Trim()).Where(p => !string.IsNullOrWhiteSpace(p)).ToList().OrderBy(o => o)
                    .Select(s => new ReportingDescriptorRelationship()
                    {
                        Target = new ReportingDescriptorReference()
                        {
                            Id = s,
                            ToolComponent = new ToolComponentReference()
                            {
                                Name = "NIST",
                                Guid = Guid.Parse("AAFBAB93-5201-419E-8443-D4925C542398")
                            }
                        },
                        Kinds = new List<string>() { "relevant" },
                    }))
            };

            var results = new List<Result>(execJsonControl.Results.Count);
            foreach (ControlResult controlResult in execJsonControl.Results)
            {
                ResultKind kind = controlResult.Status switch
                {
                    ControlResultStatus.Passed => ResultKind.Pass,
                    ControlResultStatus.Failed => ResultKind.Fail,
                    ControlResultStatus.Error => ResultKind.Review,
                    ControlResultStatus.Skipped => ResultKind.NotApplicable,
                    _ => ResultKind.Fail,
                };
                FailureLevel level = (kind == ResultKind.Fail) ? SarifLevelFromHdfImpact(execJsonControl.Impact) : FailureLevel.None;
                double rank = (kind == ResultKind.Fail) ? SarifRankFromHdfImpact(execJsonControl.Impact) : -1.0;
                var result = new Result
                {
                    RuleId = execJsonControl.Id,
                    Message = new Message
                    {
                        Text = AppendPeriod(string.IsNullOrWhiteSpace(controlResult.CodeDesc) ? execJsonControl.Desc : controlResult.CodeDesc),
                    },
                    Kind = kind,
                    Level = level,
                    Rank = rank,
                    Locations = new List<Location>
                    {
                        new Location {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                   Uri =  new Uri(".", UriKind.Relative),
                                   UriBaseId = "ROOTPATH",
                                },
                                Region = new Region
                                {
                                    StartLine = 1,
                                    StartColumn = 1,
                                    EndLine = 1,
                                    EndColumn = 1,
                                }
                            }
                        }
                    }
                };
                results.Add(result);
            }
            return (reportingDescriptor, results);
        }

        private static FailureLevel SarifLevelFromHdfImpact(double impact)
        {
            // level 4 & 5
            // Hdf: Critical, High
            if (impact >= 0.7)
            {
                return FailureLevel.Error;
            }
            // level 3
            // Hdf: Medium
            else if (impact >= 0.5)
            {
                return FailureLevel.Warning;
            }
            // level 2
            // Hdf: Low
            else if (impact >= 0.3)
            {
                return FailureLevel.Warning;
            }
            // level 0, 1
            // Hdf: Best_Practice, Information
            else
            {
                return FailureLevel.Note;
            }
        }

        private static double SarifRankFromHdfImpact(double impact) =>
            /*
            SARIF rank  Hdf Level   SARIF level Default Viewer Action
            0.0         0           note        Does not display by default
            0.3         0.3         note        Does not display by default
            0.5         0.5         warning     Displays by default, does not break build / other processes
            0.7         0.7         error       Displays by default, breaks build/ other processes
            1.0         1.0         error       Displays by default, breaks build/ other processes
            */
            // both range from 0 to 1, return as it is
            impact;

        private static string AppendPeriod(string text) =>
            text.EndsWith(PeriodString, StringComparison.OrdinalIgnoreCase) ? text : text + PeriodString;
    }
}
