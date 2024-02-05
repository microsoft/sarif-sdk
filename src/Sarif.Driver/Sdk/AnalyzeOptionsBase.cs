// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("analyze", HelpText = "Analyze one or more binary files for security and correctness issues.")]
    public class AnalyzeOptionsBase : CommonOptionsBase
    {
        [Value(0,
               HelpText = "One or more specifiers to a file, directory, or filter pattern that resolves to one or more binaries to analyze.")]
        public IList<string> TargetFileSpecifiers { get; set; }

        [Option(
            'o',
            "output",
            HelpText = "File path to which analysis output will be written.")]
        public string OutputFilePath { get; set; }

        [Option(
            'r',
            "recurse",
            HelpText = "Recurse into subdirectories when evaluating file specifier arguments.")]
        public bool? Recurse { get; set; }

        [Option(
            'c',
            "config",
            HelpText = "Path to policy file that will be used to configure analysis. This defaults to 'default.configuration.xml' beside the main tool; passing value of 'default' or removing that file will configure the tool to use its built-in settings.")]
        public string ConfigurationFilePath { get; set; }

        [Option(
            "output-config",
            HelpText = "Path to a policy file to which all analysis settings from the current run will be saved.")]
        public string OutputConfigurationFilePath { get; set; }

        [Option(
            'q',
            "quiet",
            HelpText = "Suppress all console output (except for catastrophic tool runtime or configuration errors).")]
        public bool? Quiet { get; set; }

        [Option(
            'e',
            "environment",
            HelpText = "Log machine environment details of run to output file. WARNING: This option records potentially sensitive information (such as all environment variable values) to any emitted log.")]
        public bool? LogEnvironment { get; set; }

        [Option(
            'p',
            "plugin",
            Separator = ';',
            HelpText = "Plugin paths, expressed as a semicolon-delimited list enclosed in double quotes, that " +
                       "points to plugin(s) that should drive analysis for all configured scan targets.")]
        public IEnumerable<string> PluginFilePaths { get; set; }

        [Option(
            "invocation-properties",
            Separator = ';',
            HelpText = "Properties of the Invocation object to log, expressed as a semicolon-delimited list enclosed in double quotes. " +
                       "NOTE: StartTime and EndTime are always logged.")]
        public IEnumerable<string> InvocationPropertiesToLog { get; set; }

        [Option(
            "rich-return-code",
            HelpText = "Emit a 'rich' return code consisting of a bitfield of conditions (as opposed to 0 or 1 indicating success or failure.")]
        public bool? RichReturnCode { get; set; }

        [Option(
            "trace",
            Separator = ';',
            Default = null,
            HelpText = "Execution traces, expressed as a semicolon-delimited list enclosed in double quotes, that " +
                       "should be emitted to the console and log file (if appropriate). " +
                       "Valid values: ScanTime.")]
        public IEnumerable<string> Trace { get; set; } = Array.Empty<string>();

        private IEnumerable<FailureLevel> level;
        [Option(
            "level",
            Separator = ';',
            Default = null,
            HelpText = "Failure levels, expressed as a semicolon-delimited list enclosed in double quotes, that " +
                       "is used to filter the scan results. Valid values: Error, Warning and Note.")]
        public IEnumerable<FailureLevel> Level
        {
            get => this.level;
            set => this.level = value?.Count() > 0 ? value : null;
        }

        public FailureLevelSet FailureLevels => Level != null ? new FailureLevelSet(Level) : BaseLogger.ErrorWarning;

        private IEnumerable<ResultKind> kind;
        [Option(
            "kind",
            Separator = ';',
            Default = null,
            HelpText = "Result kinds, expressed as a semicolon-delimited list enclosed in double quotes, that " +
                       "is used to filter the scan results. Valid values: Fail (for literal scan results), Pass, Review, Open, NotApplicable and Informational.")]
        public IEnumerable<ResultKind> Kind
        {
            get => this.kind;
            set => this.kind = value?.Count() > 0 ? value : null;
        }

        public ResultKindSet ResultKinds => Kind != null ? new ResultKindSet(Kind) : BaseLogger.Fail;

        [Option(
            "baseline",
            HelpText = "A SARIF file to be used as baseline.")]
        public string BaselineFilePath { get; set; }


        [Option(
            "etw",
            HelpText = "A file path to which all ETW events for the session will be saved.")]
        public string EventsFilePath { get; set; }

        [Option(
            "post-uri",
            HelpText = "A URI to which the SARIF log file will be posted.")]
        public string PostUri { get; set; }

        [Option(
            "max-file-size-in-kb",
            HelpText = "The maximum file size (in kilobytes) that will be analyzed.")]
        public long? MaxFileSizeInKilobytes { get; set; }

        [Option(
            "timeout-in-seconds",
            HelpText = "A timeout value expressed in seconds.")]
        public int? TimeoutInSeconds { get; set; }

        [Option(
            "deny-regex",
            HelpText = "A regular expression used to suppress scanning for any file or directory path that matches the regex.")]
        public string GlobalFilePathDenyRegex { get; set; }

        [Option(
            "rule-kinds",
            HelpText =
            @"Specify the kind(s) of rules that should be run.")]
        public HashSet<RuleKind> RuleKinds { get; set; } = [RuleKind.Sarif];
    }
}
