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

            LogicalLocationsDictionary.Clear();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                NameTable = _nameTable,
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
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

            var fileInfoFactory = new FileInfoFactory(null, loggingOptions);
            Dictionary<string, FileData> fileDictionary = fileInfoFactory.Create(results);

            var run = new Run()
            {
                Tool = tool
            };

            output.Initialize(run);

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
                result.Message = new Message { Text = description };
            }
            else
            {
                result.Message = new Message { Text = GenerateFullMessage(description, problem.Hints) };
            }

            SetSarifResultPropertiesForProblem(result, problem);
            var location = new Location();
            location.FullyQualifiedLogicalName = CreateSignature(problem);

            string logicalLocationKey = CreateLogicalLocation(problem);

            if (logicalLocationKey != location.FullyQualifiedLogicalName)
            {
                location.LogicalLocationKey = logicalLocationKey;
            }

            Uri uri;
            string file = problem.File;
            if (!String.IsNullOrEmpty(file))
            {
                location.PhysicalLocation = new PhysicalLocation
                {
                    FileLocation = new FileLocation(),
                    Region = problem.Line <= 0 ? null : Extensions.CreateRegion(problem.Line)
                };

                if (RemoveBadRoot(file, out uri))
                {
                    location.PhysicalLocation.FileLocation.UriBaseId = PROJECT_DIR;
                }
                location.PhysicalLocation.FileLocation.Uri = uri;
            }

            result.Locations = new List<Location> { location };

            return result;
        }

        private static string CreateSignature(AndroidStudioProblem problem)
        {
            string entryPointName = problem.EntryPointName;
            if ("file".Equals(problem.EntryPointType))
            {
                entryPointName = null;
            }

            string[] parts = new string[] { problem.Module, problem.Package, entryPointName };
            var updated = parts
                    .Where(part => !String.IsNullOrEmpty(part));

            string joinedParts = String.Join(@"\", updated);

            if (String.IsNullOrEmpty(joinedParts))
            {
                return problem.Module;
            }
            else
            {
                return joinedParts;
            }
        }

        private string CreateLogicalLocation(AndroidStudioProblem problem)
        {
            string parentLogicalLocationKey = null;

            parentLogicalLocationKey = TryAddLogicalLocation(parentLogicalLocationKey, problem.Module, LogicalLocationKind.Module);
            parentLogicalLocationKey = TryAddLogicalLocation(parentLogicalLocationKey, problem.Package, LogicalLocationKind.Package);

            if (problem.EntryPointName != null)
            {
                if ("class".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
                {
                    parentLogicalLocationKey = TryAddLogicalLocation(parentLogicalLocationKey, problem.EntryPointName, LogicalLocationKind.Type);
                }
                else if ("method".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
                {
                    parentLogicalLocationKey = TryAddLogicalLocation(parentLogicalLocationKey, problem.EntryPointName, LogicalLocationKind.Member);
                }
            }

            return parentLogicalLocationKey;
        }

        private string TryAddLogicalLocation(string parentKey, string value, string kind, string delimiter = @"\")
        {
            if (!String.IsNullOrEmpty(value))
            {
                var logicalLocation = new LogicalLocation
                {
                    ParentKey = parentKey,
                    Kind = kind,
                    Name = value
                };

                return AddLogicalLocation(logicalLocation, delimiter);
            }
            return parentKey;
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
                return String.Format(CultureInfo.InvariantCulture, ConverterResources.AndroidStudioDescriptionUnknown, problem.ProblemClass);
            }

            return desc;
        }

        private static void SetSarifResultPropertiesForProblem(Result result, AndroidStudioProblem problem)
        {
            if (problem.Severity != null)
            {
                result.SetProperty("severity", problem.Severity);
            }

            if (problem.AttributeKey != null)
            {
                result.SetProperty("attributeKey", problem.AttributeKey);
            }
        }

        private static string GenerateFullMessage(string description, ImmutableArray<string> hints)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(description);
            foreach (string hint in hints)
            {
                sb.AppendLine();
                sb.AppendFormat(CultureInfo.InvariantCulture, ConverterResources.AndroidStudioHintStaple, hint);
            }

            return sb.ToString();
        }

        private const string PROJECT_DIR = "$PROJECT_DIR$";

        private static bool RemoveBadRoot(string path, out Uri uri)
        {
            const string badRoot = "file://" + PROJECT_DIR +"/";
            bool removedBadRoot;
            string removed;
            if (path.StartsWith(badRoot, StringComparison.Ordinal))
            {
                removed = path.Substring(badRoot.Length);
                removedBadRoot = true;
            }
            else
            {
                removed = path;
                removedBadRoot = false;
            }

            uri = new Uri(removed, UriKind.RelativeOrAbsolute);
            return removedBadRoot;
        }
    }
}