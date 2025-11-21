// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.Diagnostics.Tracing.Session;

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
                AutomationGuidProperty,
                AutomationIdProperty,
                BaselineFilePathProperty,
                BinaryFileExtensionsProperty,
                ChannelSizeProperty,
                OutputConfigurationFilePathProperty,
                DataToInsertProperty,
                DataToRemoveProperty,
                EventsFilePathProperty,
                FailureLevelsProperty,
                GlobalFilePathDenyRegexProperty,
                MaxFileSizeInKilobytesProperty,
                MaxArchiveRecursionDepthProperty,
                EventsBufferSizeInMegabytesProperty,
                OpcFileExtensionsProperty,
                OutputFileOptionsProperty,
                OutputFilePathProperty,
                PluginFilePathsProperty,
                PostUriProperty,
                RecurseProperty,
                ResultKindsProperty,
                RuleKindsProperty,
                RichReturnCodeProperty,
                TargetFileSpecifiersProperty,
                ThreadsProperty,
                TracesProperty,
                TimeoutInMillisecondsProperty,
            };
        }

        /// <summary>
        ///  TBD these all need good comments.
        /// </summary>
        public virtual IList<VersionControlDetails> VersionControlProvenance { get; set; }
        public virtual ISet<string> InvocationPropertiesToLog { get; set; }
        public virtual ISet<string> PropertiesToLog { get; set; }
        public virtual ISet<string> InsertProperties { get; set; }
        public virtual bool Quiet { get; set; }
        public virtual IFileSystem FileSystem { get; set; }
        public virtual CancellationToken CancellationToken { get; set; }
        public virtual IArtifactProvider TargetsProvider { get; set; }
        public virtual IEnumeratedArtifact CurrentTarget { get; set; }
        public virtual string MimeType { get; set; }
        public virtual HashData Hashes { get; set; }
        public virtual IList<Exception> RuntimeExceptions { get; set; }
        public virtual bool IsValidAnalysisTarget { get; set; }
        public virtual ReportingDescriptor Rule { get; set; }
        public PropertiesDictionary Policy { get; set; }
        public virtual IAnalysisLogger Logger { get; set; }
        public virtual RuntimeConditions RuntimeErrors { get; set; }
        public virtual bool AnalysisComplete { get; set; }
        public bool Inline => OutputFileOptions.HasFlag(FilePersistenceOptions.Inline);
        public bool Minify => OutputFileOptions.HasFlag(FilePersistenceOptions.Minify);
        public bool Optimize => OutputFileOptions.HasFlag(FilePersistenceOptions.Optimize);
        public bool PrettyPrint => OutputFileOptions.HasFlag(FilePersistenceOptions.PrettyPrint);
        public bool ForceOverwrite => OutputFileOptions.HasFlag(FilePersistenceOptions.ForceOverwrite);

        public Regex CompiledGlobalFileDenyRegex { get; set; }

        public TraceEventSession TraceEventSession { get; set; }

        public string GlobalFilePathDenyRegex
        {
            get => this.Policy.GetProperty(GlobalFilePathDenyRegexProperty);
            set
            {
                CompiledGlobalFileDenyRegex = string.IsNullOrEmpty(value)
                    ? null
                    : new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

                this.Policy.SetProperty(GlobalFilePathDenyRegexProperty, value);
            }
        }

        public virtual ISet<string> PluginFilePaths
        {
            get => this.Policy.GetProperty(PluginFilePathsProperty);
            set => this.Policy.SetProperty(PluginFilePathsProperty, new StringSet(value));
        }

        public virtual string PostUri
        {
            get => this.Policy.GetProperty(PostUriProperty);
            set => this.Policy.SetProperty(PostUriProperty, value);
        }

        public virtual int ChannelSize
        {
            get => this.Policy.GetProperty(ChannelSizeProperty);
            set => this.Policy.SetProperty(ChannelSizeProperty, value);
        }

        public virtual Guid AutomationGuid
        {
            get => this.Policy.GetProperty(AutomationGuidProperty);
            set => this.Policy.SetProperty(AutomationGuidProperty, value);
        }

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

        public string ConfigurationFilePath { get; set; }

        public string OutputConfigurationFilePath
        {
            get => this.Policy.GetProperty(OutputConfigurationFilePathProperty);
            set => this.Policy.SetProperty(OutputConfigurationFilePathProperty, value);
        }

        public string EventsFilePath
        {
            get => this.Policy.GetProperty(EventsFilePathProperty);
            set => this.Policy.SetProperty(EventsFilePathProperty, value);
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
            set => this.Policy.SetProperty(TracesProperty, new StringSet(value));
        }

        public ISet<string> TargetFileSpecifiers
        {
            get => this.Policy.GetProperty(TargetFileSpecifiersProperty);
            set => this.Policy.SetProperty(TargetFileSpecifiersProperty, new StringSet(value));
        }

        public ISet<string> BinaryFileExtensions
        {
            get => this.Policy.GetProperty(BinaryFileExtensionsProperty);
            set => this.Policy.SetProperty(BinaryFileExtensionsProperty, new StringSet(value));
        }

        public ISet<string> OpcFileExtensions
        {
            get => this.Policy.GetProperty(OpcFileExtensionsProperty);
            set => this.Policy.SetProperty(OpcFileExtensionsProperty, new StringSet(value));
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

        public RuleKindSet RuleKinds
        {
            get => this.Policy.GetProperty(RuleKindsProperty);
            set => this.Policy.SetProperty(RuleKindsProperty, value);
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

        public int MaxArchiveRecursionDepth
        {
            get => this.Policy.GetProperty(MaxArchiveRecursionDepthProperty);
            set => this.Policy.SetProperty(MaxArchiveRecursionDepthProperty, value >= 0 ? value : MaxArchiveRecursionDepthProperty.DefaultValue());
        }

        public int EventsBufferSizeInMegabytes
        {
            get => this.Policy.GetProperty(EventsBufferSizeInMegabytesProperty);
            set => this.Policy.SetProperty(EventsBufferSizeInMegabytesProperty, value >= 0 ? value : EventsBufferSizeInMegabytesProperty.DefaultValue());
        }

        public virtual void Dispose()
        {
            var disposableLogger = this.Logger as IDisposable;
            disposableLogger?.Dispose();
            this.Logger = null;

            if (TraceEventSession != null)
            {
                if (EventsFilePath.Equals("console", StringComparison.OrdinalIgnoreCase))
                {
                    TraceEventSession.Source.Process();

                }
                else
                {
                    TraceEventSession.Flush();
                    if (TraceEventSession.EventsLost > 0)
                    {
                        Console.WriteLine(($"{TraceEventSession.EventsLost} events were lost. ETL log is incomplete."));
                    }
                    else
                    {
                        Console.WriteLine(($"No trace events were lost. ETL log is complete."));
                    }

                    TraceEventSession.Dispose();
                }
                TraceEventSession = null;
            }

            GC.SuppressFinalize(this);
        }

        public static PerLanguageOption<int> ChannelSizeProperty { get; } =
            new PerLanguageOption<int>(
                "CoreSettings", nameof(ChannelSize), defaultValue: () => 50000,
                "The capacity of the channels for analyzing scan targets and logging results.");

        public static PerLanguageOption<Guid> AutomationGuidProperty { get; } =
            new PerLanguageOption<Guid>(
                "CoreSettings", nameof(AutomationGuid), defaultValue: () => default,
                "A guid that will be persisted to the 'Run.AutomationDetails.Guid' property. " +
                "See section '3.17.4' of the SARIF specification for more information.");

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

        public static PerLanguageOption<string> EventsFilePathProperty { get; } =
                    new PerLanguageOption<string>(
                        "CoreSettings", nameof(EventsFilePath), defaultValue: () => string.Empty,
                        "The path to which ETW events raised by analysis should be saved.");

        public static PerLanguageOption<string> PostUriProperty { get; } =
                            new PerLanguageOption<string>(
                                "CoreSettings", nameof(PostUri), defaultValue: () => string.Empty,
                                "A SARIF-accepting endpoint to publish the output log to.");

        public static PerLanguageOption<string> OutputConfigurationFilePathProperty { get; } =
                            new PerLanguageOption<string>(
                                "CoreSettings", nameof(OutputConfigurationFilePath), defaultValue: () => string.Empty,
                                "The path to write all resolved configuration (by current command-line) to.");

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

        public static PerLanguageOption<StringSet> PluginFilePathsProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(PluginFilePaths), defaultValue: () => new StringSet(),
                        "Path to plugin(s) that should drive analysis for all configured scan targets.");

        public static PerLanguageOption<StringSet> TargetFileSpecifiersProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(TargetFileSpecifiers), defaultValue: () => new StringSet(),
                        "One or more file specifiers for locating scan targets.");

        public static PerLanguageOption<StringSet> BinaryFileExtensionsProperty { get; } =
                    new PerLanguageOption<StringSet>(
                        "CoreSettings", nameof(BinaryFileExtensions),
                        defaultValue: () => new StringSet([".bmp", ".cab", ".cer", ".der", ".dll", ".exe", ".gif", ".gz", ".iso", ".jpe",
                                                           ".jpeg", ".lock", ".p12", ".pack", ".pfx", ".pkcs12", ".png", ".psd", ".rar",
                                                           ".tar", ".tif", ".tiff", ".xcf", ".zip"]),
                        "One or more file extensions that should be forcibly treated as binary not textual data.");

        public static PerLanguageOption<StringSet> OpcFileExtensionsProperty { get; } =
                     new PerLanguageOption<StringSet>(
                         "CoreSettings", nameof(OpcFileExtensions),
                        defaultValue: () => new StringSet([".apk", ".appx", ".appxbundle", ".docx", ".epub", ".jar", ".msix", ".msixbundle",
                                                           ".odp", ".ods", ".odt", ".onepkg", ".oxps", ".pkg", ".pptx", ".unitypackage", "vsix",
                                                           ".vsdx", ".xps", ".xlsx", ".zip"]),
                        "One or more file extensions that should be expanded as Open Packaging Convention (OPC) files.");

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

        public static PerLanguageOption<RuleKindSet> RuleKindsProperty { get; } =
                    new PerLanguageOption<RuleKindSet>(
                        "CoreSettings", nameof(RuleKinds), defaultValue: () => new RuleKindSet(new[] { RuleKind.Sarif }),
                        "One or more rule kinds that should be run. Valid values include Sarif, Ado, Ghas. " +
                        "Defaults to 'Sarif'.");

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

        private const int DefaultMaxFileSizeInKilobytes = 10 * 1000; // 10 MB
        public static PerLanguageOption<long> MaxFileSizeInKilobytesProperty { get; } =
            new PerLanguageOption<long>(
                $"CoreSettings", nameof(MaxFileSizeInKilobytes), defaultValue: () => DefaultMaxFileSizeInKilobytes,
                $"{Environment.NewLine}" +
                $"    Scan targets that fall below this size threshold (in kilobytes) will not be analyzed.{Environment.NewLine}" +
                $"    It is legal to set this value to 0 (in order to potentially complete an analysis that{Environment.NewLine}" +
                $"    records what scan targets would have been analyzed, given current configuration.{Environment.NewLine}" +
                $"    Negative values will be discarded in favor of the default of {MaxFileSizeInKilobytesProperty?.DefaultValue() ?? DefaultMaxFileSizeInKilobytes} KB.");

        private const int DefaultMaxArchiveRecursionDepth = 10;
        public static PerLanguageOption<int> MaxArchiveRecursionDepthProperty { get; } =
            new PerLanguageOption<int>(
                $"CoreSettings", nameof(MaxArchiveRecursionDepth), defaultValue: () => DefaultMaxArchiveRecursionDepth,
                $"{Environment.NewLine}" +
                $"    Maximum depth for recursively analyzing nested archives (ZIP files, OPC packages, etc.).{Environment.NewLine}" +
                $"    This prevents stack overflow when processing deeply nested or circular archive structures.{Environment.NewLine}" +
                $"    Negative values will be discarded in favor of the default of {DefaultMaxArchiveRecursionDepth}.");


        public static PerLanguageOption<int> EventsBufferSizeInMegabytesProperty { get; } =
            new PerLanguageOption<int>(
                $"CoreSettings", nameof(EventsBufferSizeInMegabytes), defaultValue: () => 512,
                $"{Environment.NewLine}" +
                $"    A buffer size, in megabytes, passed to the events trace session instance when '--etw is enabled.");

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

        public static PerLanguageOption<string> GlobalFilePathDenyRegexProperty { get; } =
                    new PerLanguageOption<string>(
                        "CoreSettings", nameof(GlobalFilePathDenyRegex), defaultValue: () => string.Empty,
                        "An optional regex that can be used to filter unwanted files or directories from analysis, " +
                        "e.g.: (?i)\\.(?:bmp|dll|exe|gif|jpe?g|lock|pack|png|psd|tar\\.gz|tiff?|ttf|xcf|zip)$");
    }
}
