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
            (?<fileName>[^(]+)
            \(
            (?<region>[^)]+)
            \)
            \s*:\s*
            (?<level>[^\s]+)
            \s+
            (?<ruleId>[^\s:]+)
            \s*:\s*
            (?<message>.*)
            $";
        private static readonly Regex s_errorLineRegex = RegexFromPattern(ErrorLinePattern);

        private const string ToolErrorLinePattern = @"
            ^
            \s*
            (?<toolName>[^\s:]+)
            \s*:\s*
            (?<level>[^\s]+)
            \s+
            (?<ruleId>[^\s:]+)
            \s*:\s*
            (?<message>.*)
            $";
        private static readonly Regex s_toolErrorLineRegex = RegexFromPattern(ToolErrorLinePattern);

        private static readonly HashSet<string> s_lineHashes = new HashSet<string>();

        private static Result GetResultFromLine(string line)
        {
            Result result = null;

            Match match = s_errorLineRegex.Match(line);
            if (match.Success)
            {
                // MSBuild logs can contain duplicate error report lines. Take only one of them.
                if (s_lineHashes.Contains(line)) { return null; }
                s_lineHashes.Add(line);

                string fileName = match.Groups["fileName"].Value;
                string region = match.Groups["region"].Value;
                string level = match.Groups["level"].Value;
                string ruleId = match.Groups["ruleId"].Value;
                string message = match.Groups["message"].Value;

                result = new Result
                {
                    RuleId = ruleId,
                    Level = GetFailureLevelFrom(level),
                    Locations = new Location[]
                    {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(fileName, UriKind.RelativeOrAbsolute)
                            },
                            Region = GetRegionFrom(region)
                        }
                    }
                    },
                    Message = new Message
                    {
                        Text = message
                    }
                };
            }

            if (result == null)
            {
                match = s_toolErrorLineRegex.Match(line);
                if (match.Success)
                {
                    // MSBuild logs can contain duplicate error report lines. Take only one of them.
                    if (s_lineHashes.Contains(line)) { return null; }
                    s_lineHashes.Add(line);

                    string toolName = match.Groups["toolName"].Value;
                    string level = match.Groups["level"].Value;
                    string ruleId = match.Groups["ruleId"].Value;
                    string message = match.Groups["message"].Value;

                    result = new Result
                    {
                        RuleId = ruleId,
                        Level = GetFailureLevelFrom(level),
                        Message = new Message
                        {
                            Text = message
                        }
                    };

                    result.SetProperty("microsoft/visualStudioBuildLogConverter/buildToolName", toolName);
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
        //    (startLine)
        //    (startLine-endLine)
        //    (startLine,startColumn)
        //    (startLine,startColumn-endColumn)
        //    (startLine,startColumn,endLine,endColumn)

        private const string StartLineStartOnlyPattern = @"^(?<startLine>\d+)$";
        private static readonly Regex s_startLineOnlyRegex = RegexFromPattern(StartLineStartOnlyPattern);

        private const string StartLineEndLinePattern = @"^(?<startLine>\d+)-(?<endLine>\d+)$";
        private static readonly Regex s_startLineEndLineRegex = RegexFromPattern(StartLineEndLinePattern);

        private const string StartLineStartColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnRegex = RegexFromPattern(StartLineStartColumnPattern);

        private const string StartLineStartColumEndColumnPattern = @"^(?<startLine>\d+),(?<startColumn>\d+)-(?<endColumn>\d+)$";
        private static readonly Regex s_startLineStartColumnEndColumnRegex = RegexFromPattern(StartLineStartColumEndColumnPattern);

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
