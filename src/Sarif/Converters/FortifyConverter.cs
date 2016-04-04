// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class FortifyConverter : IToolFileConverter
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
        public void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }


            // We can't infer/produce a runInfo object
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                NameTable = _nameTable
            };

            output.OpenResults();

            var results = new List<Result>();
            using (XmlReader reader = XmlReader.Create(input, settings))
            {
                while (reader.Read())
                {
                    while (Ref.Equal(reader.LocalName, _strings.Issue))
                    {
                        FortifyIssue fortify = FortifyIssue.Parse(reader, _strings);
                        results.Add(ConvertFortifyIssueToSarifIssue(fortify));
                    }
                }
            }

            var toolInfo = new ToolInfo
            {
                Name = "Fortify"
            };

            var fileInfoFactory = new FileInfoFactory(MimeType.DetermineFromFileExtension);
            Dictionary<string, IList<FileReference>> fileInfoDictionary = fileInfoFactory.Create(results);

            var runInfo = fileInfoDictionary != null && fileInfoDictionary.Count > 0
                ? new RunInfo { FileInfo = fileInfoDictionary }
                : null;

            output.WriteToolAndRunInfo(toolInfo, runInfo);
            output.WriteResults(results);
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
            result.ToolFingerprint = fortify.InstanceId;
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
                result.FullMessage = String.Format(CultureInfo.InvariantCulture, SarifResources.FortifyFallbackMessage, result.RuleId);
            }
            else
            {
                result.FullMessage = String.Join(Environment.NewLine, messageComponents);
            }

            var extraProperties = new Dictionary<string, string>();
            extraProperties.Add("kingdom", fortify.Kingdom);
            if (fortify.Priority != null)
            {
                extraProperties.Add("priority", fortify.Priority);
            }

            if (!fortify.CweIds.IsDefaultOrEmpty)
            {
                extraProperties.Add("cwe", String.Join(", ",
                    fortify.CweIds.Select(id => id.ToString(CultureInfo.InvariantCulture))));
            }

            if (fortify.RuleId != null)
            {
                extraProperties.Add("fortifyRuleId", fortify.RuleId);
            }

            result.Properties = extraProperties;

            PhysicalLocation primaryOrSink = ConvertFortifyLocationToPhysicalLocation(fortify.PrimaryOrSink);
            result.Locations = new[]
            {
                new Location
                {
                    ResultFile = primaryOrSink
                }
            };

            if (fortify.Source != null)
            {
                PhysicalLocation source = ConvertFortifyLocationToPhysicalLocation(fortify.Source);
                result.ExecutionFlows = new[]
                {
                    new[]
                    {
                        new AnnotatedCodeLocation { PhysicalLocation = source },
                        new AnnotatedCodeLocation { PhysicalLocation = primaryOrSink }
                    }
                };
            }

            return result;
        }

        private static PhysicalLocation ConvertFortifyLocationToPhysicalLocation(FortifyPathElement element)
        {
            return new PhysicalLocation
            {
                Uri = new Uri(element.FilePath, UriKind.RelativeOrAbsolute),
                Region = Extensions.CreateRegion(element.LineStart)
            };
        }
    }
}
