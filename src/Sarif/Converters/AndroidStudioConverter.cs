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
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts an xml log file of the Android Studio format into the SARIF format
    /// </summary>
    internal class AndroidStudioConverter : ToolFileConverterBase
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
        public override void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            LogicalLocationsDictionary.Clear();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                NameTable = _nameTable,
                DtdProcessing = DtdProcessing.Ignore
            };

            ISet<Result> results;
            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                results = ProcessAndroidStudioLog(xmlReader);
            }

            var tool = new Tool
            {
                Name = "AndroidStudio"
            };

            var fileInfoFactory = new FileInfoFactory(uri => MimeType.Java);
            Dictionary<string, IList<FileData>> fileDictionary = fileInfoFactory.Create(results);

            output.WriteTool(tool);

            if (fileDictionary != null && fileDictionary.Any())
            {
                output.WriteFiles(fileDictionary);
            }

            if (LogicalLocationsDictionary != null && LogicalLocationsDictionary.Any())
            {
                output.WriteLogicalLocations(LogicalLocationsDictionary);
            }

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();
        }

        /// <summary>Processes an Android Studio log and writes issues therein to an instance of
        /// <see cref="IResultLogWriter"/>.</summary>
        /// <param name="xmlReader">The XML reader from which AndroidStudio format shall be read.</param>
        /// <returns>
        /// A list of the <see cref="Result"/> objects translated from the AndroidStudio format.
        /// </returns>
        private ISet<Result> ProcessAndroidStudioLog(XmlReader xmlReader)
        {
            var results = new HashSet<Result>(Result.ValueComparer);

            int problemsDepth = xmlReader.Depth;
            xmlReader.ReadStartElement(_strings.Problems);

            while (xmlReader.Depth > problemsDepth)
            {
                var problem = AndroidStudioProblem.Parse(xmlReader, _strings);
                if (problem != null)
                {
                    results.Add(ConvertProblemToSarifResult(problem));
                }
            }

            xmlReader.ReadEndElement(); // </problems>

            return results;
        }

        public Result ConvertProblemToSarifResult(AndroidStudioProblem problem)
        {
            var result = new Result();
            result.RuleId = problem.ProblemClass;
            string description = AndroidStudioConverter.GetShortDescriptionForProblem(problem);
            if (problem.Hints.IsEmpty)
            {
                result.Message = description;
            }
            else
            {
                result.Message = GenerateFullMessage(description, problem.Hints);
            }

            result.Properties = GetSarifIssuePropertiesForProblem(problem);
            var location = new Location();
            var logicalLocationComponents = new List<LogicalLocationComponent>();

            if (!String.IsNullOrWhiteSpace(problem.Module))
            {
                logicalLocationComponents.Add(new LogicalLocationComponent
                {
                    Name = problem.Module,
                    Kind = LogicalLocationKind.Module
                });
            }

            if (!String.IsNullOrWhiteSpace(problem.Package))
            {
                logicalLocationComponents.Add(new LogicalLocationComponent
                {
                    Name = problem.Package,
                    Kind = LogicalLocationKind.Package
                });
            }

            if ("class".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
            {
                logicalLocationComponents.Add(new LogicalLocationComponent
                {
                    Name = problem.EntryPointName,
                    Kind = LogicalLocationKind.Type
                });
            }

            if ("method".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
            {
                logicalLocationComponents.Add(new LogicalLocationComponent
                {
                    Name = problem.EntryPointName,
                    Kind = LogicalLocationKind.Member
                });
            }

            if (logicalLocationComponents.Count != 0)
            {
                location.FullyQualifiedLogicalName = String.Join("\\", logicalLocationComponents.Select(x => x.Name));

                AddLogicalLocation(location, logicalLocationComponents);
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

            result.Locations = new List<Location> { location };

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
                return String.Format(CultureInfo.InvariantCulture, SdkResources.AndroidStudioDescriptionUnknown, problem.ProblemClass);
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
                sb.AppendFormat(CultureInfo.InvariantCulture, SdkResources.AndroidStudioHintStaple, hint);
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