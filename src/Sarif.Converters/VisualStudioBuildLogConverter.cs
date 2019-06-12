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

        private static readonly Regex s_errorLineRegex =
            new Regex(ErrorLinePattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private static readonly HashSet<string> s_lineHashes = new HashSet<string>();

        private static Result GetResultFromLine(string line)
        {
            Match match = s_errorLineRegex.Match(line);
            if (!match.Success) { return null; }

            // MSBuild logs can contain duplicate error report lines. Take only one of them.
            if (s_lineHashes.Contains(line)) { return null; }
            s_lineHashes.Add(line);

            string fileName = match.Groups["fileName"].Value;
            string region = match.Groups["region"].Value;
            string level = match.Groups["level"].Value;
            string ruleId = match.Groups["ruleId"].Value;
            string message = match.Groups["message"].Value;

            return new Result
            {
                RuleId = ruleId,
                Level = GetFailureLevelFrom(level),
                Message = new Message
                {
                    Text = message
                }
            };
        }

        private static FailureLevel GetFailureLevelFrom(string level)
        {
            switch (level)
            {
                case "note": return FailureLevel.Note;
                case "warning": return FailureLevel.Warning;
                case "error": return FailureLevel.Error;
                default: return FailureLevel.Warning;
            }
        }
    }
}
