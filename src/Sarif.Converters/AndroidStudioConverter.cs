// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts an xml log file of the Android Studio format into the SARIF format
    /// </summary>
    public class AndroidStudioConverter : ToolFileConverterBase
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

        public override string ToolName => "AndroidStudio";

        /// <summary>
        /// Converts an Android Studio formatted log as input into a SARIF SarifLog using the output.
        /// </summary>
        /// <param name="input">The Android Studio formatted log.</param>
        /// <param name="output">The SarifLog to write the output to.</param>
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

            LogicalLocations.Clear();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                NameTable = _nameTable,
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            IList<Result> results;
            using (XmlReader xmlReader = XmlReader.Create(input, settings))
            {
                results = ProcessAndroidStudioLog(xmlReader);
            }

            var run = new Run()
            {
                Tool = new Tool { Driver = new ToolComponent { Name = ToolName } },
                ColumnKind = ColumnKind.Utf16CodeUnits,
                LogicalLocations = this.LogicalLocations,
            };

            PersistResults(output, results, run);
        }

        /// <summary>Processes an Android Studio log and writes issues therein to an instance of
        /// <see cref="IResultLogWriter"/>.</summary>
        /// <param name="xmlReader">The XML reader from which AndroidStudio format shall be read.</param>
        /// <returns>
        /// A list of the <see cref="Result"/> objects translated from the AndroidStudio format.
        /// </returns>
        private IList<Result> ProcessAndroidStudioLog(XmlReader xmlReader)
        {
            // The SARIF spec actually allows for non-unique results to be persisted to the 
            // results array. We currently enforce uniqueness in the Android converter. We could
            // consider loosening this moving forward. The current behavior is primarily
            // intended to minimize test breaks while SARIF v2 is finalized.
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

            return new List<Result>(results);
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
            var location = new Location
            {
                LogicalLocation = new LogicalLocation
                {
                    FullyQualifiedName = AddLogicalLocationEntriesForProblem(problem, out int logicalLocationIndex),
                    Index = logicalLocationIndex
                }
            };

            Uri uri;
            string file = problem.File;
            if (!string.IsNullOrEmpty(file))
            {
                location.PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation(),
                    Region = problem.Line <= 0 ? null : Extensions.CreateRegion(problem.Line)
                };

                bool foundUriBaseId = RemoveBadRoot(file, out uri);
                location.PhysicalLocation.ArtifactLocation.Uri = uri;

                // TODO enable this change after current work
                // https://github.com/Microsoft/sarif-sdk/issues/1168
                // 
                //if (foundUriBaseId)
                //{
                //    location.PhysicalLocation.FileLocation.UriBaseId = PROJECT_DIR;
                //}
            }

            result.Locations = new List<Location> { location };

            return result;
        }

        // This method adds entries to the LogicalLocations array for the logical
        // location where the problem occurs, and for each of its ancestor logical
        // locations. It returns the fully qualified name of the logical location,
        // and it fill an out parameter with the index within LogicalLocations
        // of the newly created entry for the logical location.
        private string AddLogicalLocationEntriesForProblem(AndroidStudioProblem problem, out int index)
        {
            index = -1;
            string fullyQualifiedName = null;
            string delimiter = string.Empty;

            if (!string.IsNullOrEmpty(problem.Module))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, problem.Module, LogicalLocationKind.Module, delimiter);
                delimiter = @"\";
            }

            if (!string.IsNullOrEmpty(problem.Package))
            {
                index = AddLogicalLocation(index, ref fullyQualifiedName, problem.Package, LogicalLocationKind.Package, delimiter);
                delimiter = @"\";
            }

            if (problem.EntryPointName != null)
            {
                if ("class".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
                {
                    index = AddLogicalLocation(index, ref fullyQualifiedName, problem.EntryPointName, LogicalLocationKind.Type, delimiter);

                }
                else if ("method".Equals(problem.EntryPointType, StringComparison.OrdinalIgnoreCase))
                {
                    index = AddLogicalLocation(index, ref fullyQualifiedName, problem.EntryPointName, LogicalLocationKind.Member, delimiter);
                }
                // TODO: 'file' is another entry point type. should we be doing something here?
            }

            return fullyQualifiedName;
        }

        private int AddLogicalLocation(int parentIndex, ref string fullyQualifiedName, string value, string kind, string delimiter = ".")
        {
            fullyQualifiedName = fullyQualifiedName + delimiter + value;

            // Need to decide which item gets preference, name vs. fully qualified name
            // if both match. The behaviors of various API in the SDK differs on this point.
            var logicalLocation = new LogicalLocation
            {
                FullyQualifiedName = fullyQualifiedName,
                Kind = kind,
                Name = value != fullyQualifiedName ? value : null,
                ParentIndex = parentIndex
            };

            return AddLogicalLocation(logicalLocation);
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
                return string.Format(CultureInfo.InvariantCulture, ConverterResources.AndroidStudioDescriptionUnknown, problem.ProblemClass);
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
            const string badRoot = "file://" + PROJECT_DIR + "/";
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