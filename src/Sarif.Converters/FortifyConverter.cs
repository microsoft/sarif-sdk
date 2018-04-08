// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Writers;

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

        /// <summary>
        /// Interface implementation for converting a stream of Fortify report in XML format to a
        /// SARIF json format stream.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="input">Stream of the Fortify report.</param>
        /// <param name="output">Stream of SARIF json.</param>
        /// <param name="loggingOptions">Logging options that configure output.</param>
        public override void Convert(Stream input, IResultLogWriter output, LoggingOptions loggingOptions)
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

            var results = new List<Result>();
            using (XmlReader reader = XmlReader.Create(input, settings))
            {
                while (reader.Read())
                {
                    while (StringReference.AreEqual(reader.LocalName, _strings.Issue))
                    {
                        FortifyIssue fortify = FortifyIssue.Parse(reader, _strings);
                        results.Add(ConvertFortifyIssueToSarifIssue(fortify));
                    }
                }
            }

            var tool = new Tool
            {
                Name = "Fortify"
            };

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension, loggingOptions);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            output.Initialize(id: null, automationId: null);

            output.WriteTool(tool);
            if (fileDictionary != null && fileDictionary.Count > 0) { output.WriteFiles(fileDictionary); }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
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
                if (result.ToolFingerprintContributions == null)
                {
                    result.ToolFingerprintContributions = new Dictionary<string, string>();
                }

                SarifUtilities.AddOrUpdateDictionaryEntry(result.ToolFingerprintContributions, "InstanceId", fortify.InstanceId);
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
                result.Message = String.Format(CultureInfo.InvariantCulture, ConverterResources.FortifyFallbackMessage, result.RuleId);
            }
            else
            {
                result.Message = String.Join(Environment.NewLine, messageComponents);
            }

            result.SetProperty("kingdom", fortify.Kingdom);
            if (fortify.Priority != null)
            {
                result.SetProperty("priority", fortify.Priority);
            }

            if (!fortify.CweIds.IsDefaultOrEmpty)
            {
                result.SetProperty("cwe", String.Join(", ",
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
                result.CodeFlows = new List<CodeFlow>
                {
                    new CodeFlow
                    {
                        Locations = new List<CodeFlowLocation>
                        {
                            new CodeFlowLocation
                            {
                                Location = new Location
                                {
                                    PhysicalLocation = source
                                }
                            },

                            new CodeFlowLocation
                            {
                                Location = new Location
                                {
                                    PhysicalLocation = primaryOrSink
                                }
                            }
                        }
                    }
                };
            }

            return result;
        }

        private static PhysicalLocation ConvertFortifyLocationToPhysicalLocation(FortifyPathElement element)
        {
            return new PhysicalLocation
            {
                FileLocation = new FileLocation
                {
                    Uri = new Uri(element.FilePath, UriKind.RelativeOrAbsolute)
                },
                Region = Extensions.CreateRegion(element.LineStart)
            };
        }
    }
}
