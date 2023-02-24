// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public virtual string PostUri { get; set; }
        public virtual ISet<string> InvocationPropertiesToLog { get; set; }
        public virtual ISet<string> PluginFilePaths { get; set; }
        public virtual ISet<string> PropertiesToLog { get; set; }
        public virtual ISet<string> InsertProperties { get; set; }
        public virtual Guid? AutomationGuid { get; set; }
        public virtual bool Quiet { get; set; }
        public virtual IFileSystem FileSystem { get; set; }
        public virtual CancellationToken CancellationToken { get; set; }
        public virtual IArtifactProvider TargetsProvider { get; set; }
        public virtual IEnumeratedArtifact CurrentTarget { get; set; }
        public virtual string MimeType { get; set; }
        public virtual HashData Hashes { get; set; }
        public virtual Exception RuntimeException { get; set; }
        public virtual bool IsValidAnalysisTarget { get; set; }
        public virtual ReportingDescriptor Rule { get; set; }
        public PropertiesDictionary Policy { get; set; }
        public virtual IAnalysisLogger Logger { get; set; }
        public virtual RuntimeConditions RuntimeErrors { get; set; }
        public virtual bool AnalysisComplete { get; set; }

        public bool Minify => OutputFileOptions.HasFlag(FilePersistenceOptions.Minify);
        public bool Optimize => OutputFileOptions.HasFlag(FilePersistenceOptions.Optimize);
        public bool PrettyPrint => OutputFileOptions.HasFlag(FilePersistenceOptions.PrettyPrint);
        public bool ForceOverwrite => OutputFileOptions.HasFlag(FilePersistenceOptions.ForceOverwrite);

        public virtual string AutomationId
        {
            get => this.Policy.GetProperty(AutomationIdProperty);
            set => this.Policy.SetProperty(AutomationIdProperty, value);
        }

        public FilePersistenceOptions OutputFileOptions
        {
            get => this.Policy.GetProperty(OutputFileOptionsProperty);
            set => this.Policy.SetProperty(OutputFileOptionsProperty, value);
        }

        public string BaselineFilePath
        {
            get => this.Policy.GetProperty(BaselineFilePathProperty);
            set => this.Policy.SetProperty(BaselineFilePathProperty, value);
        }

        public string OutputFilePath
        {
            get => this.Policy.GetProperty(OutputFilePathProperty);
            set => this.Policy.SetProperty(OutputFilePathProperty, value);
        }

        public string ConfigurationFilePath
        {
            get => this.Policy.GetProperty(ConfigurationFilePathProperty);
            set => this.Policy.SetProperty(ConfigurationFilePathProperty, value);
        }

        /// <summary>
        /// Gets or sets flags that specify how log SARIF should be enriched,
        /// e.g., by including file hashes or comprehensive regions properties.
        /// </summary>
        public OptionallyEmittedData DataToInsert
        {
            get => this.Policy.GetProperty(DataToInsertProperty);
            set => this.Policy.SetProperty(DataToInsertProperty, value);
        }

        /// <summary>
        /// Gets or sets flags that specify how log SARIF should be optimized,
        /// e.g., by including file hashes or comprehensive regions properties.
        /// </summary>
        public OptionallyEmittedData DataToRemove
        {
            get => this.Policy.GetProperty(DataToRemoveProperty);
            set => this.Policy.SetProperty(DataToRemoveProperty, value);
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

        public FailureLevelSet FailureLevels
        {
            get => this.Policy.GetProperty(FailureLevelsProperty);
            set => this.Policy.SetProperty(FailureLevelsProperty, value);
        }

        public ResultKindSet ResultKinds
        {
            get => this.Policy.GetProperty(ResultKindsProperty);
            set => this.Policy.SetProperty(ResultKindsProperty, value);
        }

        public virtual bool RichReturnCode
        {
            get => this.Policy.GetProperty(RichReturnCodeProperty);
            set => this.Policy.SetProperty(RichReturnCodeProperty, value);
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

        public virtual void Dispose()
        {
            var disposableLogger = this.Logger as IDisposable;
            disposableLogger?.Dispose();
            GC.SuppressFinalize(this);
            this.Logger = null;
        }

        public static PerLanguageOption<string> AutomationIdProperty { get; } =
                    new PerLanguageOption<string>(
                        "CoreSettings", nameof(AutomationId), defaultValue: () => string.Empty,
                        "An id that will be persisted to the 'Run.AutomationDetails.Id' property. " +
                        "See section '3.17.3' of the SARIF specification for more information.");

        public static PerLanguageOption<string> BaselineFilePathProperty { get; } =
                    new PerLanguageOption<string>(
                        "CoreSettings", nameof(BaselineFilePath), defaultValue: () => string.Empty,
                        "The path to a SARIF baseline file.");

        public static PerLanguageOption<string> OutputFilePathProperty { get; } =
                            new PerLanguageOption<string>(
                                "CoreSettings", nameof(OutputFilePath), defaultValue: () => string.Empty,
                                "The path to write all SARIF log file results to.");

        public static PerLanguageOption<string> ConfigurationFilePathProperty { get; } =
                            new PerLanguageOption<string>(
                                "CoreSettings", nameof(ConfigurationFilePath), defaultValue: () => string.Empty,
                                "The path to write all SARIF log file results to.");

        public static PerLanguageOption<OptionallyEmittedData> DataToInsertProperty { get; } =
                    new PerLanguageOption<OptionallyEmittedData>(
                        "CoreSettings", nameof(DataToInsert), defaultValue: () => 0,
                        "Optionally present data that should be inserted into log output. Valid values include. " +
                        "Hashes, TextFiles, BinaryFiles, EnvironmentVariables, RegionSnippets, ContextRegionSnippets, " +
                        "Guids, VersionControlDetails, and NondeterministicProperties.");

        public static PerLanguageOption<OptionallyEmittedData> DataToRemoveProperty { get; } =
                    new PerLanguageOption<OptionallyEmittedData>(
                        "CoreSettings", nameof(DataToRemove), defaultValue: () => 0,
                        "Optionally present data that should be removed from log output, e.g., NondeterminsticProperties.");

        public static PerLanguageOption<StringSet> TracesProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(Traces), defaultValue: () => new StringSet(new[] { "None" }),
                        "A set of trace values. Zero, one or more of ScanTime, RuleScanTime.");

        public static PerLanguageOption<StringSet> TargetFileSpecifiersProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(TargetFileSpecifiers), defaultValue: () => new StringSet(),
                        "One or more file specifiers for locating scan targets.");

        public static PerLanguageOption<FailureLevelSet> FailureLevelsProperty { get; } =
                    new PerLanguageOption<FailureLevelSet>(
                        "CoreSettings", nameof(FailureLevels), defaultValue: () => new FailureLevelSet(new[] { FailureLevel.Error, FailureLevel.Warning }),
                        "One or more failure levels to persist to loggers. Valid values include Error, Warning, " +
                        "and Informational. Defaults to 'Error' and 'Warning'.");

        public static PerLanguageOption<ResultKindSet> ResultKindsProperty { get; } =
                    new PerLanguageOption<ResultKindSet>(
                        "CoreSettings", nameof(ResultKinds), defaultValue: () => new ResultKindSet(new[] { ResultKind.Fail }),
                        "One or more result kinds to persist to loggers. Valid values include None, NotApplicable, Pass, Fail, " +
                        "Review, Open, Informational. Defaults to 'Fail'.");

        public static PerLanguageOption<FilePersistenceOptions> OutputFileOptionsProperty { get; } =
                    new PerLanguageOption<FilePersistenceOptions>(
                        "CoreSettings", nameof(OutputFileOptions), defaultValue: () => FilePersistenceOptions.PrettyPrint,
                        "Configures one or more file output options. Valid values include PrettyPrint, Minify, " +
                        "Inline, and Optimize. Pretty-printed output is the default. When Inline is specified, " +
                        "output will be written to relevant input files rather than generating a new log. Optimize " +
                        "indicates that all duplicative information in the log should be removed, minimizing size.");

        public static PerLanguageOption<bool> RichReturnCodeProperty { get; } =
                    new PerLanguageOption<bool>(
                        "CoreSettings", nameof(RichReturnCode), defaultValue: () => false,
                        "Emit a 'rich' return code consisting of a bitfield of conditions (as opposed to 0 or 1 indicating success or failure.");

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
                "CoreSettings", nameof(Threads), defaultValue: () => Debugger.IsAttached ? 1 : Environment.ProcessorCount,
                "Count of threads to use in any parallel execution context. Defaults to '1' when " +
                "the debugger is attached, otherwise is set to the environment processor count. " +
                "Negative values are interpreted as '1'.");
    }
}
