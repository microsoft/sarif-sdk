// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public enum BoolType
    {
        True,
        False,
    }

    [Verb("analyze", HelpText = "Analyze one or more binary files for security and correctness issues.")]
    public abstract class AnalyzeOptionsBase : CommonOptionsBase
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
        public BoolType? Recurse { get; set; }

        [Option(
            'c',
            "config",
            HelpText = "Path to policy file that will be used to configure analysis. This defaults to 'default.configuration.xml' beside the main tool; passing value of 'default' or removing that file will configure the tool to use its built-in settings.")]
        public string ConfigurationFilePath { get; set; }

        [Option(
            'q',
            "quiet",
            HelpText = "Suppress all console output (except for catastrophic tool runtime or configuration errors).")]
        public BoolType? Quiet { get; set; }

        [Option(
            'e',
            "environment",
            HelpText = "Log machine environment details of run to output file. WARNING: This option records potentially sensitive information (such as all environment variable values) to any emitted log.")]
        public BoolType? LogEnvironment { get; set; }

        [Option(
            "plugin",
            Separator = ';',
            HelpText = "Path to plugin that will be invoked against all targets in the analysis set.")]
        public IEnumerable<string> PluginFilePaths { get; set; }

        [Option(
            "invocation-properties",
            Separator = ';',
            HelpText = "Properties of the Invocation object to log. NOTE: StartTime and EndTime are always logged.")]
        public IEnumerable<string> InvocationPropertiesToLog { get; set; }

        [Option(
            "rich-return-code",
            HelpText = "Emit a 'rich' return code consisting of a bitfield of conditions (as opposed to 0 or 1 indicating success or failure.")]
        public BoolType? RichReturnCode { get; set; }

        private IEnumerable<string> trace;
        [Option(
            "trace",
            Separator = ';',
            Default = null,
            HelpText = "Execution traces, expressed as a semicolon-delimited list, that " +
                       "should be emitted to the console and log file (if appropriate). " +
                       "Valid values: ScanTime.")]
        public IEnumerable<string> Trace
        {
            get => this.trace;
            set => this.trace = value?.Count() > 0 ? value : null;
        }

        private DefaultTraces? defaultTraces;
        public DefaultTraces Traces
        {
            get
            {
                defaultTraces ??=
                    this.Trace.Any()
                        ? (DefaultTraces)Enum.Parse(typeof(DefaultTraces), string.Join(",", this.Trace))
                        : DefaultTraces.None;

                return this.defaultTraces.Value;
            }
        }

        private IEnumerable<FailureLevel> level;
        [Option(
            "level",
            Separator = ';',
            Default = null,
            HelpText = "A semicolon delimited list to filter output of scan results to one or more failure levels. Valid values: Error, Warning and Note.")]
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
            HelpText = "A semicolon delimited list to filter output to one or more result kinds. Valid values: Fail (for literal scan results), Pass, Review, Open, NotApplicable and Informational.")]
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
            "post-uri",
            HelpText = "A URI to which the SARIF log file will be posted.")]
        public string PostUri { get; set; }

        [Option(
            "max-file-size-in-kb",
            HelpText = "The maximum file size (in kilobytes) that will be analyzed.")]
        public long? MaxFileSizeInKilobytes { get; set; }
    }
}
