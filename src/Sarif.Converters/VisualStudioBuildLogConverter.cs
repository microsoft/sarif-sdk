// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class VisualStudioBuildLogConverter : ToolFileConverterBase
    {
        public override string ToolName => ToolFormat.VisualStudioBuildLog;

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

            IList<Result> results = GetResults(input);

            PersistResults(output, results);
        }

        private static IList<Result> GetResults(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                return GetResults(reader);
            }
        }

        private static IList<Result> GetResults(TextReader reader)
        {
            IList<Result> results = new List<Result>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Result result = GetResultFromLine(line);
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
        private static readonly Regex s_errorLineRegex = RegexFromPattern(ErrorLinePattern);

        private static readonly HashSet<string> s_lineHashes = new HashSet<string>();

        private static Result GetResultFromLine(string line)
        {
            Result result = null;
            string fileName, region, buildTool, levelQualification = null, level, ruleId, message;

            Match match = s_errorLineRegex.Match(line);
            if (match.Success)
            {
                // MSBuild logs can contain duplicate error report lines. Take only one of them.
                if (s_lineHashes.Contains(line)) { return null; }
                s_lineHashes.Add(line);

                fileName = match.Groups["fileName"].Value;
                region = match.Groups["region"].Value;
                buildTool = match.Groups["buildTool"].Value;
                levelQualification = match.Groups["levelQualification"].Value;
                level = match.Groups["level"].Value;
                ruleId = match.Groups["ruleId"].Value;
                message = match.Groups["message"].Value;

                result = new Result
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

                if (!string.IsNullOrWhiteSpace(buildTool))
                {
                    result.SetProperty("microsoft/visualStudioBuildLogConverter/buildTool", buildTool);
                }

                if (!string.IsNullOrWhiteSpace(levelQualification))
                {
                    result.SetProperty("microsoft/visualStudioBuildLogConverter/levelQualification", levelQualification);
                }
            }

            return result;
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
        private static readonly Regex s_startLineOnlyRegex = RegexFromPattern(StartLineStartOnlyPattern);

        // (startLine-endLine)
        private const string StartLineEndLinePattern = @"^(?<startLine>\d+)-(?<endLine>\d+)$";
        private static readonly Regex s_startLineEndLineRegex = RegexFromPattern(StartLineEndLinePattern);

        // (startLine,startColumn)
        private const string StartLineStartColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnRegex = RegexFromPattern(StartLineStartColumnPattern);

        // (startLine,startColumn-endColumn)
        private const string StartLineStartColumEndColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)-(?<endColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnEndColumnRegex = RegexFromPattern(StartLineStartColumEndColumnPattern);

        // (startLine,startColumn,endLine,endColumn)
        private const string StartLineStartColumEndLineEndColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+),(?<endLine>\d+),(?<endColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnEndLineEndColumnRegex = RegexFromPattern(StartLineStartColumEndLineEndColumnPattern);

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

        private static Regex RegexFromPattern(string pattern) =>
            new Regex(
                pattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
    }
}