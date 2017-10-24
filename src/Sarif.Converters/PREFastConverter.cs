using System.IO;
using Microsoft.CodeAnalysis.Sarif.Writers;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class PREfastConverter : ToolFileConverterBase
    {
        private Dictionary<string, string> categories = new Dictionary<string, string>
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

            var serializer = new XmlSerializer(typeof(DefectList));
            var defectList = (DefectList)serializer.Deserialize(input);
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

        private Result CreateResult(Defect defect)
        {
            var region = new Region
            {
                StartColumn = defect.SFA.Column + 1,
                StartLine = defect.SFA.Line
            };

            Uri resultsFileUri = new Uri($"{defect.SFA.FilePath}{defect.SFA.FileName}", UriKind.Relative);
            PhysicalLocation physicalLocation = new PhysicalLocation(uri: resultsFileUri, uriBaseId: null, region: region);
            Location location = new Location()
            {
                ResultFile = physicalLocation,
                FullyQualifiedLogicalName = defect.Function,
                DecoratedName = defect.Decorated
            };

            location.SetProperty("funcline", defect.Funcline);

            var result = new Result
            {
                RuleId = defect.DefectCode,
                Message = RemoveLegacyNewLine(defect.Description),
                Locations = new List<Location>()
            };

            result.Locations.Add(location);
            SetProbability(defect, result);
            SetRank(defect, result);

            ExtractCategories(defect, result);
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
            if (defect.Path?.SFAs.Any() == false)
                return;

            int step = 0;
            var locations = new List<AnnotatedCodeLocation>();
            bool pathUsesKeyEvents = defect.Path.SFAs.Any(x => !string.IsNullOrWhiteSpace(x?.KeyEvent?.Id));

            foreach (var sfa in defect.Path.SFAs)
            {
                var reg = new Region()
                {
                    StartColumn = sfa.Column + 1,
                    StartLine = sfa.Line
                };

                Uri uri = new Uri($"{sfa.FilePath}{sfa.FileName}", UriKind.Relative);
                PhysicalLocation fileLocation = new PhysicalLocation(uri: uri, uriBaseId: null, region: reg);
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

            var codeFlow = new CodeFlow()
            {
                Locations = locations
            };

            result.CodeFlows = new List<CodeFlow>()
            {
                codeFlow
            };
        }

        private void ExtractCategories(Defect defect, Result result)
        {
            if (defect.Category != null)
            {
                foreach (var keyValuePair in defect.Category)
                {
                    string category = keyValuePair.Key;
                    if (categories.ContainsKey(category))
                    {
                        category = categories[category];
                    }

                    result.SetProperty(category, keyValuePair.Value);
                }
            }
        }

        private string RemoveLegacyNewLine(string content)
        {
            return content.Replace("PREFAST_NEWLINE\n", "");
        }
    }
}
