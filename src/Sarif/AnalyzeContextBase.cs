// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class AnalyzeContextBase : IAnalysisContext, IOptionsProvider
    {
        public AnalyzeContextBase()
        {
            this.Policy = new PropertiesDictionary();
        }

        // All of these properties are persisted to configuration XML and can be
        // passed using that mechanism. All command-line arguments are 
        // candidates to follow this pattern.
        public virtual IEnumerable<IOption> GetOptions()
        {
            return new IOption[]
            {
                TracesProperty,
                ThreadsProperty,
                RecurseProperty,
                ResultKindsProperty,
                DataToInsertProperty,
                FailureLevelsProperty,
                TimeoutInMillisecondsProperty,
                TargetFileSpecifiersProperty,
                MaxFileSizeInKilobytesProperty
            };
        }
        public virtual IFileSystem FileSystem { get; set; }
        public virtual CancellationToken CancellationToken { get; set; }
        public virtual IArtifactProvider TargetsProvider { get; set; }
        public virtual IEnumeratedArtifact CurrentTarget { get; set; }
        public virtual string MimeType { get; set; }
        public virtual HashData Hashes { get; set; }
        public virtual Exception TargetLoadException { get; set; }
        public virtual bool IsValidAnalysisTarget { get; set; }
        public virtual ReportingDescriptor Rule { get; set; }
        public PropertiesDictionary Policy { get; set; }
        public virtual IAnalysisLogger Logger { get; set; }
        public virtual RuntimeConditions RuntimeErrors { get; set; }
        public virtual bool AnalysisComplete { get; set; }

        /// <summary>
        /// Gets or sets flags that specify how log SARIF should be enriched,
        /// e.g., by including file hashes or comprehensive regions properties.
        /// </summary>
        public OptionallyEmittedData DataToInsert
        {
            get => this.Policy.GetProperty(DataToInsertProperty);
            set => this.Policy.SetProperty(DataToInsertProperty, value);
        }

        public ISet<string> Traces
        {
            get => this.Policy.GetProperty(TracesProperty);
            set => this.Policy.SetProperty(TracesProperty, value);
        }

        public ISet<string> TargetFileSpecifiers
        {
            get => this.Policy.GetProperty(TargetFileSpecifiersProperty);
            set => this.Policy.SetProperty(TargetFileSpecifiersProperty, value);
        }

        public ISet<FailureLevel> FailureLevels
        {
            get => this.Policy.GetProperty(FailureLevelsProperty);
            set => this.Policy.SetProperty(FailureLevelsProperty, value);
        }

        public ISet<ResultKind> ResultKinds
        {
            get => this.Policy.GetProperty(ResultKindsProperty);
            set => this.Policy.SetProperty(ResultKindsProperty, value);
        }

        public bool Recurse
        {
            get => this.Policy.GetProperty(RecurseProperty);
            set => this.Policy.SetProperty(RecurseProperty, value);
        }

        public int TimeoutInMilliseconds
        {
            get => Math.Max(this.Policy.GetProperty(TimeoutInMillisecondsProperty), 0);
            set => this.Policy.SetProperty(TimeoutInMillisecondsProperty, value);
        }

        public int Threads
        {
            get => Math.Max(this.Policy.GetProperty(ThreadsProperty), 1);
            set => this.Policy.SetProperty(ThreadsProperty, value);
        }

        public long MaxFileSizeInKilobytes
        {
            get => this.Policy.GetProperty(MaxFileSizeInKilobytesProperty);
            set => this.Policy.SetProperty(MaxFileSizeInKilobytesProperty, value >= 0 ? value : MaxFileSizeInKilobytesProperty.DefaultValue());
        }

        abstract public void Dispose();

        public static PerLanguageOption<OptionallyEmittedData> DataToInsertProperty { get; } =
                    new PerLanguageOption<OptionallyEmittedData>(
                        "CoreSettings", nameof(DataToInsert), defaultValue: () => 0,
                        "Optionally present data that should be inserted into log output. Valid values include. " +
                        "Hashes, TextFiles, BinaryFiles, EnvironmentVariables, RegionSnippets, ContextRegionSnippets, " +
                        "Guids, VersionControlDetails, and NondeterministicProperties.");

        public static PerLanguageOption<StringSet> TracesProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(Traces), defaultValue: () => new StringSet(new[] { "None" }),
                        "A set of trace values. Zero, one or more of ScanTime, RuleScanTime.");

        public static PerLanguageOption<StringSet> TargetFileSpecifiersProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(TargetFileSpecifiers), defaultValue: () => new StringSet(),
                        "One or more file specifiers for locating scan targets.");

        public static PerLanguageOption<ISet<FailureLevel>> FailureLevelsProperty { get; } =
                    new PerLanguageOption<ISet<FailureLevel>>(
                        "CoreSettings", nameof(FailureLevels), defaultValue: () => new HashSet<FailureLevel>(new[] { FailureLevel.Error, FailureLevel.Warning }),
                        "One or more failure levels to persist to loggers. Valid values include Error, Warning, " +
                        "and Informational. Defaults to 'Error' and 'Warning'.");

        public static PerLanguageOption<ISet<ResultKind>> ResultKindsProperty { get; } =
                    new PerLanguageOption<ISet<ResultKind>>(
                        "CoreSettings", nameof(ResultKinds), defaultValue: () => new HashSet<ResultKind>(new[] { ResultKind.Fail }),
                        "One or more result kinds to persist to loggers. Valid values include None, NotApplicable, Pass, Fail, " +
                        "Review, Open, Informational. Defaults to 'Fail'.");

        public static PerLanguageOption<bool> RecurseProperty { get; } =
                    new PerLanguageOption<bool>(
                        "CoreSettings", nameof(Recurse), defaultValue: () => false,
                        "Specifies whether to recurse into child directories when enumerating scan targets. " +
                        "Defaults to 'False'.");

        public static PerLanguageOption<long> MaxFileSizeInKilobytesProperty { get; } =
            new PerLanguageOption<long>(
                $"CoreSettings", nameof(MaxFileSizeInKilobytes), defaultValue: () => 1024,
                $"{Environment.NewLine}" +
                $"    Scan targets that fall below this size threshold (in kilobytes) will not be analyzed.{Environment.NewLine}" +
                $"    It is legal to set this value to 0 (in order to potentially complete an analysis that{Environment.NewLine}" +
                $"    records what scan targets would have been analyzed, given current configuration.{Environment.NewLine}" +
                $"    Negative values will be discarded in favor of the default of {MaxFileSizeInKilobytesProperty?.DefaultValue() ?? 1024} KB.");

        public static PerLanguageOption<int> TimeoutInMillisecondsProperty { get; } =
            new PerLanguageOption<int>(
                "CoreSettings", nameof(TimeoutInMilliseconds), defaultValue: () => int.MaxValue,
                "A timeout value expressed in milliseconds. Default value is 2^31 ms (~25 days)." +
                "Any negative value set is interpreted as '0' (i.e., timeout immediately).");

        public static PerLanguageOption<int> ThreadsProperty { get; } =
            new PerLanguageOption<int>(
                "CoreSettings", nameof(Threads), defaultValue: () => { return Debugger.IsAttached ? 1 : Environment.ProcessorCount; },
                "Count of threads to use in any parallel execution context. Defaults to '1' when " +
                "the debugger is attached, otherwise is set to the environment processor count. " +
                "Negative values are interpreted as '1'.");
    }
}
