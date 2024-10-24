// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Sarif.Converters.ClangTidyObjectModel;

using YamlDotNet.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class ClangTidyConverter : ToolFileConverterBase
    {
        private const string ToolInformationUri = "https://clang.llvm.org/extra/clang-tidy/";
        private static readonly Regex ClangTidyLogRegex = new Regex("(.*):(\\d*):(\\d*): (warning|error): (.*) \\[(.*)\\]", RegexOptions.Compiled);

        public override string ToolName => "Clang-Tidy";

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            IDeserializer deserializer = new DeserializerBuilder().Build();
            using var textReader = new StreamReader(input);
            ClangTidyReport report = deserializer.Deserialize<ClangTidyReport>(textReader);

            var logs = new List<ClangTidyConsoleDiagnostic>();
            if (report != null)
            {
                string reportPath = (input as FileStream)?.Name;
                if (reportPath != null)
                {
                    string logPath = reportPath + ".log";
                    if (File.Exists(logPath))
                    {
                        logs = LoadLogFile(logPath);
                    }
                }
            }

            AddLineNumberAndColumnNumber(report, logs);

            (List<ReportingDescriptor>, List<Result>) rulesAndResults = ExtractRulesAndResults(report);

            var run = new Run
            {
                Tool = new Tool
                {
                    Driver = new ToolComponent
                    {
                        Name = ToolName,
                        InformationUri = new Uri(ToolInformationUri),
                        Rules = rulesAndResults.Item1,
                    }
                },
                Results = rulesAndResults.Item2,
            };

            PersistResults(output, rulesAndResults.Item2, run);
        }

        private void AddLineNumberAndColumnNumber(ClangTidyReport report, List<ClangTidyConsoleDiagnostic> logs)
        {
            if (report.Diagnostics.Count == logs.Count)
            {
                for (int i = 0; i < logs.Count; i++)
                {
                    report.Diagnostics[i].DiagnosticMessage.LineNumber = logs[i].LineNumber;
                    report.Diagnostics[i].DiagnosticMessage.ColumnNumber = logs[i].ColumnNumber;
                }
            }
        }

        private List<ClangTidyConsoleDiagnostic> LoadLogFile(string logFilePath)
        {
            var returnValue = new List<ClangTidyConsoleDiagnostic>();

            var logLines = File.ReadAllLines(logFilePath).ToList();
            foreach (string line in logLines)
            {
                Match match = ClangTidyLogRegex.Match(line);

                if (match.Success)
                {
                    int lineNumber;
                    int columnNumber;
                    if (int.TryParse(match.Groups[2].Value, out lineNumber) && int.TryParse(match.Groups[3].Value, out columnNumber))
                    {
                        var consoleDiagnostic = new ClangTidyConsoleDiagnostic()
                        {
                            LineNumber = lineNumber,
                            ColumnNumber = columnNumber
                        };
                        returnValue.Add(consoleDiagnostic);
                    }
                }
            }

            return returnValue;
        }

        internal static Result CreateResult(ClangTidyDiagnostic entry)
        {
            entry = entry ?? throw new ArgumentNullException(nameof(entry));

            var result = new Result()
            {
                RuleId = entry.DiagnosticName,
                Message = new Message { Text = entry.DiagnosticMessage.Message },
                Kind = ResultKind.Fail
            };

            // no level infomation in Clang-Tidy report
            result.Level = FailureLevel.Warning;

            var region = new Region()
            {
                CharOffset = entry.DiagnosticMessage.FileOffset,
                StartLine = entry.DiagnosticMessage.LineNumber,
                StartColumn = entry.DiagnosticMessage.ColumnNumber,
            };

            var analysisTargetUri = new Uri(entry.DiagnosticMessage.FilePath, UriKind.RelativeOrAbsolute);

            var physicalLocation = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation
                {
                    Uri = analysisTargetUri
                },
                Region = region
            };

            var location = new Location()
            {
                PhysicalLocation = physicalLocation
            };

            result.Locations = new List<Location>()
            {
                location
            };

            if (entry.DiagnosticMessage.Replacements.Count > 0)
            {
                IList<Replacement> replacements = new List<Replacement>();

                foreach (ClangTidyReplacement fix in entry.DiagnosticMessage.Replacements)
                {
                    var replacement = new Replacement();

                    replacement.DeletedRegion = new Region
                    {
                        CharLength = fix.Length,
                        CharOffset = fix.Offset,
                    };

                    if (!string.IsNullOrEmpty(fix.ReplacementText))
                    {
                        replacement.InsertedContent = new ArtifactContent
                        {
                            Text = fix.ReplacementText
                        };
                    }

                    replacements.Add(replacement);
                }

                var sarifFileChange = new ArtifactChange
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = analysisTargetUri
                    },
                    Replacements = replacements
                };

                var sarifFix = new Fix(description: null, artifactChanges: new List<ArtifactChange>() { sarifFileChange }, properties: null);
                result.Fixes = new List<Fix> { sarifFix };
            }

            return result;
        }

        private static (List<ReportingDescriptor>, List<Result>) ExtractRulesAndResults(ClangTidyReport report)
        {
            var rules = new Dictionary<string, ReportingDescriptor>(StringComparer.OrdinalIgnoreCase);
            var results = new List<Result>();

            foreach (ClangTidyDiagnostic diagnostic in report.Diagnostics)
            {
                string ruleId = diagnostic.DiagnosticName;
                (ReportingDescriptor, Result) ruleAndResult = SarifRuleAndResultFromClangTidyDiagnostic(diagnostic);
                if (!rules.ContainsKey(ruleId))
                {
                    rules.Add(ruleId, ruleAndResult.Item1);
                }

                results.Add(ruleAndResult.Item2);
            }

            return (rules.Values.ToList(), results);
        }

        private static (ReportingDescriptor, Result) SarifRuleAndResultFromClangTidyDiagnostic(ClangTidyDiagnostic diagnostic)
        {
            var reportingDescriptor = new ReportingDescriptor
            {
                Id = diagnostic.DiagnosticName,
                HelpUri = diagnostic.DiagnosticName.Equals("clang-diagnostic-error", StringComparison.OrdinalIgnoreCase)
                ? null
                : new Uri($"https://clang.llvm.org/extra/clang-tidy/checks/{diagnostic.DiagnosticName}.html")
            };

            Result result = CreateResult(diagnostic);

            return (reportingDescriptor, result);
        }
    }
}
