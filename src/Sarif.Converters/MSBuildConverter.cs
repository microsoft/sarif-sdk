// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class MSBuildConverter : ToolFileConverterBase
    {
        private readonly HashSet<string> fullMessageHashes = new HashSet<string>();

        public override string ToolName => ToolFormat.MSBuild;

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

            fullMessageHashes.Clear();
            IList<Result> results = GetResults(input);

            PersistResults(output, results);
        }

        private IList<Result> GetResults(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                return GetResults(reader);
            }
        }

        private IList<Result> GetResults(TextReader reader)
        {
            IList<Result> results = new List<Result>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Result result = GetResultFrom(line);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private const string ErrorLinePattern = @"
            ^
            \s*
            (                                  # EITHER
              (                                # a file name and a region,
                (?<fileName>[^(]+)             # for example, 'MyFile.c(14,9)'...
                \(
                (?<region>[^)]+)
                \)
              )
              |                                # OR
              (                                # the name of a build tool,
                (?<buildTool>[^\s:]+)          # for example, 'LINK'.
              )
            )
            \s*:\s*
            (?<qualifiedLevel>
              (?<levelQualification>.*)        # For example, 'fatal'.
              (?<level>error|warning|info)
            )
            \s+
            (?<ruleId>[^\s:]+)
            \s*:\s*
            (?<message>.*)
            $";
        private static readonly Regex s_errorLineRegex = SarifUtilities.RegexFromPattern(ErrorLinePattern);

        private Result GetResultFrom(string fullMessage)
        {
            Match match = s_errorLineRegex.Match(fullMessage);
            if (!match.Success) { return null; }

            // MSBuild logs can contain duplicate error report lines. Take only one of them.
            if (fullMessageHashes.Contains(fullMessage)) { return null; }
            fullMessageHashes.Add(fullMessage);

            string fileName = match.Groups["fileName"].Value;
            string region = match.Groups["region"].Value;
            string buildTool = match.Groups["buildTool"].Value;
            string levelQualification = match.Groups["levelQualification"].Value;
            string level = match.Groups["level"].Value;
            string ruleId = match.Groups["ruleId"].Value;
            string message = match.Groups["message"].Value;

            var result = new Result
            {
                RuleId = ruleId,
                Level = GetFailureLevelFrom(level),
                Message = new Message
                {
                    Text = message
                }
            };

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var physicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri(fileName, UriKind.RelativeOrAbsolute)
                    }
                };

                if (!string.IsNullOrWhiteSpace(region))
                {
                    physicalLocation.Region = GetRegionFrom(region);
                }

                result.Locations = new Location[]
                {
                    new Location
                    {
                        PhysicalLocation = physicalLocation
                    }
                };
            }

            SetProperty(result, fullMessage.Trim(), nameof(fullMessage));
            SetPropertyIfPresent(result, buildTool, nameof(buildTool));
            SetPropertyIfPresent(result, levelQualification, nameof(levelQualification));

            return result;
        }

        private static void SetPropertyIfPresent(IPropertyBagHolder containingObject, string propertyValue, string propertyName)
        {
            if (!string.IsNullOrWhiteSpace(propertyValue))
            {
                SetProperty(containingObject, propertyValue, propertyName);
            }
        }

        private static void SetProperty(IPropertyBagHolder containingObject, string propertyValue, string propertyName)
        {
            containingObject.SetProperty("microsoft/msBuildConverter/" + propertyName, propertyValue.Trim());
        }

        private static FailureLevel GetFailureLevelFrom(string level)
        {
            switch (level)
            {
                case "info": return FailureLevel.Note;
                case "warning": return FailureLevel.Warning;
                case "error": return FailureLevel.Error;
                default: return FailureLevel.Warning;
            }
        }

        // VS supports the following region formatting options:

        // (startLine)
        private const string StartLineStartOnlyPattern = @"^(?<startLine>\d+)$";
        private static readonly Regex s_startLineOnlyRegex = SarifUtilities.RegexFromPattern(StartLineStartOnlyPattern);

        // (startLine-endLine)
        private const string StartLineEndLinePattern = @"^(?<startLine>\d+)-(?<endLine>\d+)$";
        private static readonly Regex s_startLineEndLineRegex = SarifUtilities.RegexFromPattern(StartLineEndLinePattern);

        // (startLine,startColumn)
        private const string StartLineStartColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnRegex = SarifUtilities.RegexFromPattern(StartLineStartColumnPattern);

        // (startLine,startColumn-endColumn)
        private const string StartLineStartColumEndColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)-(?<endColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnEndColumnRegex = SarifUtilities.RegexFromPattern(StartLineStartColumEndColumnPattern);

        // (startLine,startColumn,endLine,endColumn)
        private const string StartLineStartColumEndLineEndColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+),(?<endLine>\d+),(?<endColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnEndLineEndColumnRegex = SarifUtilities.RegexFromPattern(StartLineStartColumEndLineEndColumnPattern);

        private static Region GetRegionFrom(string regionString)
        {
            int startLine, startColumn, endLine, endColumn;
            Match match;
            Region region = null;

            // Try the startLine,startColumn pattern first because it's the most common.
            match = s_startLineStartColumnRegex.Match(regionString);
            if (match.Success)
            {
                startLine = Int32.Parse(match.Groups["startLine"].Value);
                startColumn = Int32.Parse(match.Groups["startColumn"].Value);

                region = new Region
                {
                    StartLine = startLine,
                    StartColumn = startColumn
                };
            }

            if (region == null)
            {
                match = s_startLineOnlyRegex.Match(regionString);
                if (match.Success)
                {
                    startLine = Int32.Parse(match.Groups["startLine"].Value);

                    region = new Region
                    {
                        StartLine = startLine
                    };
                }
            }

            if (region == null)
            {
                match = s_startLineEndLineRegex.Match(regionString);
                if (match.Success)
                {
                    startLine = Int32.Parse(match.Groups["startLine"].Value);
                    endLine = Int32.Parse(match.Groups["endLine"].Value);

                    region = new Region
                    {
                        StartLine = startLine,
                        EndLine = endLine
                    };
                }
            }

            if (region == null)
            {
                match = s_startLineStartColumnEndColumnRegex.Match(regionString);
                if (match.Success)
                {
                    startLine = Int32.Parse(match.Groups["startLine"].Value);
                    startColumn = Int32.Parse(match.Groups["startColumn"].Value);
                    endColumn = Int32.Parse(match.Groups["endColumn"].Value);

                    region = new Region
                    {
                        StartLine = startLine,
                        StartColumn = startColumn,
                        EndColumn = endColumn
                    };
                }
            }

            if (region == null)
            {
                match = s_startLineStartColumnEndLineEndColumnRegex.Match(regionString);
                if (match.Success)
                {
                    startLine = Int32.Parse(match.Groups["startLine"].Value);
                    startColumn = Int32.Parse(match.Groups["startColumn"].Value);
                    endLine = Int32.Parse(match.Groups["endLine"].Value);
                    endColumn = Int32.Parse(match.Groups["endColumn"].Value);

                    region = new Region
                    {
                        StartLine = startLine,
                        StartColumn = startColumn,
                        EndLine = endLine,
                        EndColumn = endColumn
                    };
                }
            }

            return region;
        }
    }
}