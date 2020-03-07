// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyConverter : ToolFileConverterBase
    {
        private readonly NameTable _nameTable;
        private readonly FortifyStrings _strings;

        /// <summary>Initializes a new instance of the <see cref="FortifyConverter"/> class.</summary>
        public FortifyConverter()
        {
            _nameTable = new NameTable();
            _strings = new FortifyStrings(_nameTable);
        }

        public override string ToolName => ToolFormat.Fortify;

        /// <summary>
        /// Interface implementation for converting a stream of Fortify report in XML format to a
        /// SARIF json format stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">Stream of the Fortify report.</param>
        /// <param name="output">Stream of SARIF json.</param>
        /// <param name="dataToInsert">Optionally emitted properties that should be written to log.</param>
        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                NameTable = _nameTable,
                XmlResolver = null
            };

            string runDescription = null;
            var results = new List<Result>();

            using (XmlReader reader = XmlReader.Create(input, settings))
            {
                while (reader.Read())
                {
                    if (runDescription == null)
                    {
                        // Find the executive summary <ReportSection> element
                        if (StringReference.AreEqual(reader.LocalName, _strings.ReportSection) && reader.IsStartElement())
                        {
                            reader.Read(); // Move to Title element

                            if (reader.ReadElementContentAsString(_strings.Title, string.Empty) == "Executive Summary")
                            {
                                reader.Read(); // Move to SubSection element
                                reader.IgnoreElement(_strings.Title, IgnoreOptions.Required);
                                reader.IgnoreElement(_strings.Description, IgnoreOptions.Required);
                                runDescription = reader.ReadElementContentAsString(_strings.Text, string.Empty);
                            }
                        }
                    }
                    else
                    {
                        while (StringReference.AreEqual(reader.LocalName, _strings.Issue))
                        {
                            FortifyIssue fortify = FortifyIssue.Parse(reader, _strings);
                            results.Add(ConvertFortifyIssueToSarifIssue(fortify));
                        }
                    }
                }
            }

            var run = new Run()
            {
                AutomationDetails = new RunAutomationDetails
                {
                    Description = new Message
                    {
                        Text = runDescription
                    }
                },
                Tool = new Tool { Driver = new ToolComponent { Name = ToolName } }
            };

            PersistResults(output, results, run);
        }

        /// <summary>Converts a Fortify result to a static analysis results interchange format result.</summary>
        /// <param name="fortify">The Fortify result convert.</param>
        /// <returns>
        /// A SARIF result <see cref="Result"/> containing the same content as the supplied
        /// <see cref="FortifyIssue"/>.
        /// </returns>
        public static Result ConvertFortifyIssueToSarifIssue(FortifyIssue fortify)
        {
            var result = new Result();
            result.RuleId = fortify.Category;

            if (!string.IsNullOrWhiteSpace(fortify.InstanceId))
            {
                if (result.PartialFingerprints == null)
                {
                    result.PartialFingerprints = new Dictionary<string, string>();
                }

                SarifUtilities.AddOrUpdateDictionaryEntry(result.PartialFingerprints, "InstanceId", fortify.InstanceId);
            }

            List<string> messageComponents = new List<string>();
            if (fortify.Abstract != null)
            {
                messageComponents.Add(fortify.Abstract);
            }

            if (fortify.AbstractCustom != null)
            {
                messageComponents.Add(fortify.AbstractCustom);
            }

            if (messageComponents.Count == 0)
            {
                result.Message = new Message
                {
                    Text = string.Format(CultureInfo.InvariantCulture, ConverterResources.FortifyFallbackMessage, result.RuleId)
                };
            }
            else
            {
                result.Message = new Message
                {
                    Text = string.Join(Environment.NewLine, messageComponents)
                };
            }

            result.SetProperty("kingdom", fortify.Kingdom);
            if (fortify.Priority != null)
            {
                result.SetProperty("priority", fortify.Priority);
            }

            if (!fortify.CweIds.IsDefaultOrEmpty)
            {
                result.SetProperty("cwe", string.Join(", ",
                    fortify.CweIds.Select(id => id.ToString(CultureInfo.InvariantCulture))));
            }

            if (fortify.RuleId != null)
            {
                result.SetProperty("fortifyRuleId", fortify.RuleId);
            }

            PhysicalLocation primaryOrSink = ConvertFortifyLocationToPhysicalLocation(fortify.PrimaryOrSink);
            result.Locations = new List<Location>
            {
                new Location
                {
                    PhysicalLocation = primaryOrSink
                }
            };

            if (fortify.Source != null)
            {
                PhysicalLocation source = ConvertFortifyLocationToPhysicalLocation(fortify.Source);

                var locations = new List<ThreadFlowLocation>()
                {
                    new ThreadFlowLocation { Location = new Location { PhysicalLocation = source } },
                    new ThreadFlowLocation { Location = new Location { PhysicalLocation = primaryOrSink } }
                };
                result.CodeFlows = new List<CodeFlow>()
                {
                    SarifUtilities.CreateSingleThreadedCodeFlow(locations)
                };
            }

            return result;
        }

        private static PhysicalLocation ConvertFortifyLocationToPhysicalLocation(FortifyPathElement element)
        {
            return new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = new Uri(element.FilePath, UriKind.RelativeOrAbsolute)
                },
                Region = Extensions.CreateRegion(element.LineStart)
            };
        }
    }
}
