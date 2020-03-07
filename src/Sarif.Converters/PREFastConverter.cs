// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class PREfastConverter : ToolFileConverterBase
    {
        private readonly Dictionary<string, string> knownCategories = new Dictionary<string, string>
        {
            { "RULECATEGORY", "ruleCategory" }
        };

        public override string ToolName => ToolFormat.PREfast;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            LogicalLocations.Clear();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            var serializer = new XmlSerializer(typeof(DefectList));

            using (var reader = XmlReader.Create(input, settings))
            {
                var defectList = (DefectList)serializer.Deserialize(reader);
                var results = new List<Result>();
                foreach (Defect entry in defectList.Defects)
                {
                    results.Add(CreateResult(entry));
                }

                var run = new Run()
                {
                    Tool = new Tool
                    {
                        Driver = new ToolComponent
                        {
                            Name = ToolName,
                            FullName = "PREfast Code Analysis"
                        }
                    },
                    ColumnKind = ColumnKind.Utf16CodeUnits,
                    LogicalLocations = LogicalLocations
                };

                PersistResults(output, results, run);
            }
        }

        private Result CreateResult(Defect defect)
        {
            var region = new Region
            {
                StartColumn = defect.SFA.Column + 1,
                StartLine = defect.SFA.Line
            };

            string filePath = defect.SFA.FilePath.Trim('\\') + @"\";
            var resultsFileUri = new Uri($"{filePath}{defect.SFA.FileName}", UriKind.Relative);

            var physicalLocation = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = resultsFileUri
                },
                Region = region
            };

            var location = new Location()
            {
                PhysicalLocation = physicalLocation,
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = defect.Function
                }
            };

            if (!string.IsNullOrEmpty(defect.Funcline))
            {
                location.SetProperty("funcline", defect.Funcline);
            }
            int logicalLocationIndex = AddLogicalLocation(defect.Function, defect.Decorated);

            location.LogicalLocation.Index = logicalLocationIndex;

            var result = new Result
            {
                RuleId = defect.DefectCode,
                Message = new Message { Text = RemovePREfastNewLine(defect.Description) },
                Locations = new List<Location>()
            };

            result.Locations.Add(location);

            SetProbability(defect, result);
            SetRank(defect, result);
            SetCategories(defect, result);

            GenerateCodeFlows(defect, result);
            GenerateSuppressions(defect.AdditionalInfo, result);

            return result;
        }

        private void GenerateSuppressions(AdditionalInfo additionalInfo, Result result)
        {
            if (additionalInfo == null) { return; }

            Suppression suppression = GenerateSuppression(additionalInfo);
            if (suppression == null) { return; }

            result.Suppressions = new List<Suppression> { suppression };
        }

        internal static Suppression GenerateSuppression(AdditionalInfo additionalInfo)
        {
            // If this sentinel key does not exist, we have no suppression data.
            if (!additionalInfo.ContainsKey("SuppressedMatch")) return null;

            additionalInfo.TryGetValue("HashKey", out string hashKey);
            additionalInfo.TryGetValue("MatchingScore", out string matchingScore);
            additionalInfo.TryGetValue("SuppressJustification", out string justification);

            var suppression = new Suppression
            {
                Kind = SuppressionKind.External,
                Justification = justification
            };

            if (!string.IsNullOrEmpty(matchingScore))
            {
                suppression.SetProperty("MatchingScore", matchingScore);
            }

            if (!string.IsNullOrEmpty(hashKey))
            {
                suppression.SetProperty("HashKey", hashKey);
            }

            return suppression;
        }

        private int AddLogicalLocation(string name, string decoratedName)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            const string ScopeOperator = "::";
            string fullyQualifiedName = name;

            name = name.Contains(ScopeOperator) ? name.Substring(name.LastIndexOf(ScopeOperator) + ScopeOperator.Length) : null;

            var logicalLocation = new LogicalLocation
            {
                Name = name,
                FullyQualifiedName = fullyQualifiedName,
                DecoratedName = decoratedName,
                ParentIndex = -1
            };

            return AddLogicalLocation(logicalLocation);
        }

        private void SetRank(Defect defect, Result result)
        {
            if (string.IsNullOrWhiteSpace(defect.Rank))
                return;

            result.SetProperty("rank", defect.Rank);
        }

        private void SetProbability(Defect defect, Result result)
        {
            if (string.IsNullOrWhiteSpace(defect.Probability))
                return;

            result.SetProperty("probability", defect.Probability);
        }

        private void GenerateCodeFlows(Defect defect, Result result)
        {
            List<SFA> sfas = defect?.Path?.SFAs;
            if (sfas == null || sfas.Count == 0)
            {
                return;
            }

            var locations = new List<ThreadFlowLocation>();
            bool pathUsesKeyEvents = defect.Path.SFAs.Any(x => !string.IsNullOrWhiteSpace(x?.KeyEvent?.Id));

            foreach (SFA sfa in defect.Path.SFAs)
            {
                var region = new Region()
                {
                    StartColumn = sfa.Column + 1,
                    StartLine = sfa.Line
                };

                string filePath = sfa.FilePath + ((sfa.FilePath.EndsWith(@"\") ? "" : @"\"));
                var uri = new Uri($"{filePath}{sfa.FileName}", UriKind.Relative);
                var fileLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = uri
                    },
                    Region = region
                };

                var threadFlowLocation = new ThreadFlowLocation
                {
                    Location = new Location
                    {
                        PhysicalLocation = fileLocation
                    },
                };

                if (pathUsesKeyEvents)
                {
                    if (string.IsNullOrWhiteSpace(sfa.KeyEvent?.Id))
                    {
                        threadFlowLocation.Importance = ThreadFlowLocationImportance.Unimportant;
                    }
                    else
                    {
                        threadFlowLocation.SetProperty("keyEventId", sfa.KeyEvent.Id);

                        if (Enum.TryParse(sfa.KeyEvent.Importance, true, out ThreadFlowLocationImportance importance))
                        {
                            threadFlowLocation.Importance = importance;
                        }

                        if (!string.IsNullOrWhiteSpace(sfa.KeyEvent.Message) &&
                            threadFlowLocation.Location?.Message != null)
                        {
                            threadFlowLocation.Location.Message.Text = sfa.KeyEvent.Message;
                        }
                    }
                }

                locations.Add(threadFlowLocation);
            }

            result.CodeFlows = new List<CodeFlow>()
            {
                SarifUtilities.CreateSingleThreadedCodeFlow(locations)
            };
        }

        private void SetCategories(Defect defect, Result result)
        {
            if (defect.Category != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in defect.Category)
                {
                    string category = keyValuePair.Key;
                    if (knownCategories.ContainsKey(category))
                    {
                        category = knownCategories[category];
                    }

                    result.SetProperty(category, keyValuePair.Value);
                }
            }
        }

        private string RemovePREfastNewLine(string content)
        {
            // TODO: need to accept this change soon
            // https://github.com/Microsoft/sarif-sdk/issues/1169
            //return content.Replace("PREFAST_NEWLINE\n", string.Empty).Trim();
            return content.Replace("PREFAST_NEWLINE\n", string.Empty);
        }
    }
}
