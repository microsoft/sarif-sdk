// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts an xml log file of the Android Studio format into the SARIF format
    /// </summary>
    internal class AndroidStudioConverter : IToolFileConverter
    {
        private readonly NameTable _nameTable;
        private readonly AndroidStudioStrings _strings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidStudioConverter"/> class.
        /// </summary>
        public AndroidStudioConverter()
        {
            _nameTable = new NameTable();
            _strings = new AndroidStudioStrings(_nameTable);
        }

        /// <summary>
        /// Converts an Android Studio formatted log as input into a SARIF SarifLog using the output.
        /// </summary>
        /// <param name="input">The Android Studio formatted log.</param>
        /// <param name="output">The SarifLog to write the output to.</param>
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

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                NameTable = _nameTable,
                DtdProcessing = DtdProcessing.Ignore
            };

            IList<Result> results;
            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                results = ProcessAndroidStudioLog(xmlReader);
            }

            var toolInfo = new ToolInfo
            {
                Name = "AndroidStudio"
            };

            var fileInfoFactory = new FileInfoFactory(uri => MimeType.Java);
            Dictionary<string, IList<FileReference>> fileInfoDictionary = fileInfoFactory.Create(results);

            var runInfo = fileInfoDictionary != null && fileInfoDictionary.Count > 0
                ? new RunInfo { FileInfo = fileInfoDictionary }
                : null;

            output.WriteToolAndRunInfo(toolInfo, runInfo);
            output.WriteResults(results);
        }

        /// <summary>Processes an Android Studio log and writes issues therein to an instance of
        /// <see cref="IResultLogWriter"/>.</summary>
        /// <param name="xmlReader">The XML reader from which AndroidStudio format shall be read.</param>
        /// <returns>
        /// A list of the <see cref="Result"/> objects translated from the AndroidStudio format.
        /// </returns>
        private IList<Result> ProcessAndroidStudioLog(XmlReader xmlReader)
        {
            var results = new List<Result>();

            int problemsDepth = xmlReader.Depth;
            xmlReader.ReadStartElement(_strings.Problems);

            output.OpenResults();
            while (xmlReader.Depth > problemsDepth)
            {
                var problem = AndroidStudioProblem.Parse(xmlReader, _strings);
                if (problem != null)
                {
                    results.Add(ConvertProblemToSarifIssue(problem));
                }
            }

            xmlReader.ReadEndElement(); // </problems>

            return results;
        }

        public static Result ConvertProblemToSarifIssue(AndroidStudioProblem problem)
        {
            var result = new Result();
            result.RuleId = problem.ProblemClass;
            string description = AndroidStudioConverter.GetShortDescriptionForProblem(problem);
            if (problem.Hints.IsEmpty)
            {
                result.FullMessage = description;
            }
            else
            {
                result.ShortMessage = description;
                result.FullMessage = GenerateFullMessage(description, problem.Hints);
            }

            result.Properties = GetSarifIssuePropertiesForProblem(problem);
            var location = new Location();
            result.Locations = new[] { location };
            var logicalLocation = new List<LogicalLocationComponent>();

            if (!String.IsNullOrWhiteSpace(problem.Module))
            {
                logicalLocation.Add(new LogicalLocationComponent
                {
                    Name = problem.Module,
                    Kind = LogicalLocationKind.AndroidModule
                });
            }

            if (!String.IsNullOrWhiteSpace(problem.Package))
            {
                logicalLocation.Add(new LogicalLocationComponent
                {
                    Name = problem.Package,
                    Kind = LogicalLocationKind.JvmPackage
                });
            }

            if ("class".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
            {
                logicalLocation.Add(new LogicalLocationComponent
                {
                    Name = problem.EntryPointName,
                    Kind = LogicalLocationKind.JvmType
                });
            }

            if ("method".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
            {
                logicalLocation.Add(new LogicalLocationComponent
                {
                    Name = problem.EntryPointName,
                    Kind = LogicalLocationKind.JvmFunction
                });
            }

            if (logicalLocation.Count != 0)
            {
                location.LogicalLocation = logicalLocation;
                location.FullyQualifiedLogicalName = String.Join("\\", location.LogicalLocation.Select(x => x.Name));
            }

            string file = problem.File;
            if (!String.IsNullOrEmpty(file))
            {
                location.ResultFile = new PhysicalLocation
                {
                    Uri = RemoveBadRoot(file),
                    Region = problem.Line <= 0 ? null : Extensions.CreateRegion(problem.Line)
                };
            }

            if ("file".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
            {
                if (location.AnalysisTarget != null)
                {
                    location.ResultFile = location.AnalysisTarget;
                }

                location.AnalysisTarget = new PhysicalLocation
                {
                    Uri = RemoveBadRoot(problem.EntryPointName)
                };
            }

            return result;
        }

        /// <summary>Generates a user-facing description for a problem, using the description supplied at
        /// construction time if it is present; otherwise, generates a description from the problem type.</summary>
        /// <param name="problem">The problem.</param>
        /// <returns>A user usable description message.</returns>
        public static string GetShortDescriptionForProblem(AndroidStudioProblem problem)
        {
            string desc = problem.Description;
            if (desc == null)
            {
                return String.Format(CultureInfo.InvariantCulture, SarifResources.AndroidStudioDescriptionUnknown, problem.ProblemClass);
            }

            return desc;
        }

        private static Dictionary<string, string> GetSarifIssuePropertiesForProblem(AndroidStudioProblem problem)
        {
            var props = new Dictionary<string, string>();
            if (problem.Severity != null)
            {
                props.Add("severity", problem.Severity);
            }

            if (problem.AttributeKey != null)
            {
                props.Add("attributeKey", problem.AttributeKey);
            }

            if (props.Count == 0)
            {
                return null;
            }

            return props;
        }

        private static string GenerateFullMessage(string description, ImmutableArray<string> hints)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(description);
            foreach (string hint in hints)
            {
                sb.AppendLine();
                sb.AppendFormat(CultureInfo.InvariantCulture, SarifResources.AndroidStudioHintStaple, hint);
            }

            return sb.ToString();
        }

        private static Uri RemoveBadRoot(string path)
        {
            const string badRoot = "file://$PROJECT_DIR$/";
            string removed;
            if (path.StartsWith(badRoot, StringComparison.Ordinal))
            {
                removed = path.Substring(badRoot.Length);
            }
            else
            {
                removed = path;
            }

            return new Uri(removed, UriKind.RelativeOrAbsolute);
        }
    }
}