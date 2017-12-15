﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel;
using Microsoft.CodeAnalysis.Sarif.Writers;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class PREfastConverter : ToolFileConverterBase
    {
        private Dictionary<string, string> knownCategories = new Dictionary<string, string>
        {
            { "RULECATEGORY", "ruleCategory" }
        };

        public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));
            output.Initialize(id: null, automationId: null);

            var tool = new Tool
            {
                Name = ToolFormat.PREfast,
                FullName = "PREfast Code Analysis"
            };

            output.WriteTool(tool);

            XmlReaderSettings settings = new XmlReaderSettings
            {
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
        }

        private Result CreateResult(Defect defect)
        {
            var region = new Region
            {
                StartColumn = defect.SFA.Column + 1,
                StartLine = defect.SFA.Line
            };

            var resultsFileUri = new Uri($"{defect.SFA.FilePath}{defect.SFA.FileName}", UriKind.Relative);
            var physicalLocation = new PhysicalLocation(uri: resultsFileUri, uriBaseId: null, region: region);
            var location = new Location()
            {
                ResultFile = physicalLocation,
                FullyQualifiedLogicalName = defect.Function,
                DecoratedName = defect.Decorated
            };

            location.SetProperty("funcline", defect.Funcline);

            var result = new Result
            {
                RuleId = defect.DefectCode,
                Message = RemovePREfastNewLine(defect.Description),
                Locations = new List<Location>()
            };

            result.Locations.Add(location);
            SetProbability(defect, result);
            SetRank(defect, result);

            SetCategories(defect, result);
            GenerateCodeFlows(defect, result);

            return result;
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

            int step = 0;
            var locations = new List<AnnotatedCodeLocation>();
            bool pathUsesKeyEvents = defect.Path.SFAs.Any(x => !string.IsNullOrWhiteSpace(x?.KeyEvent?.Id));

            foreach (var sfa in defect.Path.SFAs)
            {
                var region = new Region()
                {
                    StartColumn = sfa.Column + 1,
                    StartLine = sfa.Line
                };

                var uri = new Uri($"{sfa.FilePath}{sfa.FileName}", UriKind.Relative);
                var fileLocation = new PhysicalLocation(uri: uri, uriBaseId: null, region: region);
                var annotatedCodeLocation = new AnnotatedCodeLocation
                {
                    PhysicalLocation = fileLocation,
                    Step = ++step
                };

                if (pathUsesKeyEvents)
                {
                    if (string.IsNullOrWhiteSpace(sfa.KeyEvent?.Id))
                    {
                        annotatedCodeLocation.Importance = AnnotatedCodeLocationImportance.Unimportant;
                    }
                    else
                    {
                        annotatedCodeLocation.SetProperty("keyEventId", sfa.KeyEvent.Id);
                        if (Enum.TryParse(sfa.KeyEvent.Kind, true, out AnnotatedCodeLocationKind kind))
                        {
                            annotatedCodeLocation.Kind = kind;
                        }

                        if (Enum.TryParse(sfa.KeyEvent.Importance, true, out AnnotatedCodeLocationImportance importance))
                        {
                            annotatedCodeLocation.Importance = importance;
                        }

                        if (!string.IsNullOrWhiteSpace(sfa.KeyEvent.Message))
                        {
                            annotatedCodeLocation.Message = sfa.KeyEvent.Message;
                        }
                    }
                }

                locations.Add(annotatedCodeLocation);
            }

            result.CodeFlows = new List<CodeFlow>()
            {
                new CodeFlow
                {
                    Locations = locations
                }
            };
        }

        private void SetCategories(Defect defect, Result result)
        {
            if (defect.Category != null)
            {
                foreach (var keyValuePair in defect.Category)
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
            return content.Replace("PREFAST_NEWLINE\n", string.Empty);
        }
    }
}
