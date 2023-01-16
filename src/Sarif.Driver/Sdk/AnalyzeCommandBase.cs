﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Obsolete("AnalyzeCommandBase will be deprecated entirely soon. Use MultithreadedAnalyzeCommandBase instead.")]
    public abstract class AnalyzeCommandBase<TContext, TOptions> : PluginDriverCommand<TOptions>
        where TContext : IAnalysisContext, new()
        where TOptions : AnalyzeOptionsBase
    {
        public const string DefaultPolicyName = "default";

        // This is a unit test knob that configures the console output logger to capture its output
        // in a string builder. This is important to do because tooling behavior differs in non-trivial
        // ways depending on whether output is captured to a log file disk or not. In the latter case,
        // the captured output is useful to verify behavior.
        internal bool _captureConsoleOutput;

        internal ConsoleLogger _consoleLogger;

        private Run _run;
        private Tool _tool;
        internal TContext _rootContext;
        private CacheByFileHashLogger _cacheByFileHashLogger;
        private IDictionary<string, HashData> _pathToHashDataMap;

        public Exception ExecutionException { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        public virtual FileFormat ConfigurationFormat { get { return FileFormat.Json; } }

        protected AnalyzeCommandBase(IFileSystem fileSystem = null)
        {
            FileSystem = fileSystem ?? Sarif.FileSystem.Instance;
        }

        public string DefaultConfigurationPath
        {
            get
            {
                string currentDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                return Path.Combine(currentDirectory, "default.configuration.xml");
            }
        }

        public override int Run(TOptions options)
        {
            try
            {
                //  To correctly initialize the logger, we must first add Hashes to dataToInsert
#pragma warning disable CS0618 // Type or member is obsolete
                if (options.ComputeFileHashes)
#pragma warning restore CS0618
                {
                    OptionallyEmittedData dataToInsert = options.DataToInsert.ToFlags();
                    dataToInsert |= OptionallyEmittedData.Hashes;

                    options.DataToInsert = Enum.GetValues(typeof(OptionallyEmittedData)).Cast<OptionallyEmittedData>()
                        .Where(oed => dataToInsert.HasFlag(oed)).ToList();
                }

                // 0. Initialize an common logger that drives all outputs. This
                //    object drives logging for console, statistics, etc.
                using (AggregatingLogger logger = InitializeLogger(options))
                {
                    //  Once the logger has been correctly initialized, we can raise a warning
                    _rootContext = CreateContext(options, logger, RuntimeErrors);

#pragma warning disable CS0618 // Type or member is obsolete
                    if (options.ComputeFileHashes)
#pragma warning restore CS0618
                    {
                        Warnings.LogObsoleteOption(_rootContext, "--hashes", SdkResources.ComputeFileHashes_ReplaceInsertHashes);
                    }

                    try
                    {
                        Analyze(options, logger);
                    }
                    catch (ExitApplicationException<ExitReason> ex)
                    {
                        // These exceptions have already been logged
                        ExecutionException = ex;
                        return FAILURE;
                    }
                    catch (Exception ex)
                    {
                        // These exceptions escaped our net and must be logged here
                        Errors.LogUnhandledEngineException(_rootContext, ex);
                        ExecutionException = ex;
                        return FAILURE;
                    }
                    finally
                    {
                        RuntimeErrors |= _rootContext.RuntimeErrors;
                        logger.AnalysisStopped(RuntimeErrors);
                    }
                }

                bool succeeded = (RuntimeErrors & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None;

                if (succeeded)
                {
                    try
                    {
                        ProcessBaseline(_rootContext, options, FileSystem);
                    }
                    catch (Exception ex)
                    {
                        RuntimeErrors |= RuntimeConditions.ExceptionProcessingBaseline;
                        ExecutionException = ex;
                        return FAILURE;
                    }

                    try
                    {
                        PostLogFile(options.PostUri, options.OutputFilePath, FileSystem);
                    }
                    catch (Exception ex)
                    {
                        RuntimeErrors |= RuntimeConditions.ExceptionPostingLogFile;
                        ExecutionException = ex;
                        return FAILURE;
                    }
                }

                if (options.RichReturnCode)
                {
                    return (int)RuntimeErrors;
                }

                return succeeded ? SUCCESS : FAILURE;
            }
            finally
            {
                _rootContext?.Dispose();
            }
        }

        private void Analyze(TOptions options, AggregatingLogger logger)
        {
            // 0. Log analysis initiation
            logger.AnalysisStarted();

            // 1. Create context object to pass to skimmers. The logger
            //    and configuration objects are common to all context
            //    instances and will be passed on again for analysis.
            _rootContext = CreateContext(options, logger, RuntimeErrors);

            // 2. Perform any command line argument validation beyond what
            //    the command line parser library is capable of.
            ValidateOptions(_rootContext, options);

            // 3. Produce a comprehensive set of analysis targets
            ISet<string> targets = CreateTargetsSet(options);

            // 4. Proactively validate that we can locate and
            //    access all analysis targets. Helper will return
            //    a list that potentially filters out files which
            //    did not exist, could not be accessed, etc.
            targets = ValidateTargetsExist(_rootContext, targets);

            // 5. Initialize report file, if configured.
            InitializeOutputFile(options, _rootContext);

            // 6. Instantiate skimmers.
            ISet<Skimmer<TContext>> skimmers = CreateSkimmers(options, _rootContext);

            // 7. Initialize configuration. This step must be done after initializing
            //    the skimmers, as rules define their specific context objects and
            //    so those assemblies must be loaded.
            InitializeConfiguration(options, _rootContext);

            // 8. Initialize skimmers. Initialize occurs a single time only. This
            //    step needs to occurs after initializing configuration in order
            //    to allow command-line override of rule settings
            skimmers = InitializeSkimmers(skimmers, _rootContext);

            // 9. Run all analysis
            AnalyzeTargets(options, skimmers, _rootContext, targets);

            // 10. For test purposes, raise an unhandled exception if indicated
            if (RaiseUnhandledExceptionInDriverCode)
            {
                throw new InvalidOperationException(GetType().Name);
            }
        }

        protected virtual void ValidateOptions(TContext context, TOptions analyzeOptions)
        {
            bool succeeded = true;

            succeeded &= ValidateFile(context, analyzeOptions.ConfigurationFilePath, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateFile(context, analyzeOptions.BaselineSarifFile, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateOutputFileCanBeCreated(context, analyzeOptions.OutputFilePath, analyzeOptions.Force);
            succeeded &= ValidateFiles(context, analyzeOptions.PluginFilePaths, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateFile(context, analyzeOptions.OutputFilePath, DefaultPolicyName, shouldExist: null);
            succeeded &= ValidateInvocationPropertiesToLog(context, analyzeOptions.InvocationPropertiesToLog);
            succeeded &= analyzeOptions.ValidateOutputOptions(context);
            succeeded &= context.MaxFileSizeInKilobytes >= 0;

            if (!succeeded)
            {
                //  TODO: This seems like uninformative error output.  All these errors get squished into one generic message
                //  whenever something goes wrong. #2260 https://github.com/microsoft/sarif-sdk/issues/2260
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }
        }

        protected virtual bool ShouldEnqueue(string file, TContext context)
        {
            bool shouldEnqueue = IsTargetWithinFileSizeLimit(file, context.MaxFileSizeInKilobytes, out long fileSizeInKb);

            if (!shouldEnqueue)
            {
                Warnings.LogFileSkippedDueToSize(context, file, fileSizeInKb);
            }

            return shouldEnqueue;
        }

        internal AggregatingLogger InitializeLogger(AnalyzeOptionsBase analyzeOptions)
        {
            _tool = Tool.CreateFromAssemblyData();

            var logger = new AggregatingLogger();

            if (!analyzeOptions.Quiet)
            {
                _consoleLogger = new ConsoleLogger(analyzeOptions.Quiet, _tool.Driver.Name, analyzeOptions.Level, analyzeOptions.Kind) { CaptureOutput = _captureConsoleOutput };
                logger.Loggers.Add(_consoleLogger);
            }

            if ((analyzeOptions.DataToInsert.ToFlags() & OptionallyEmittedData.Hashes) != 0)
            {
                _cacheByFileHashLogger = new CacheByFileHashLogger(analyzeOptions.Level, analyzeOptions.Kind);
                logger.Loggers.Add(_cacheByFileHashLogger);
            }

            return logger;
        }

        private ISet<string> CreateTargetsSet(TOptions analyzeOptions)
        {
            SortedSet<string> targets = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string specifier in analyzeOptions.TargetFileSpecifiers)
            {
                string normalizedSpecifier = specifier;

                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    if (uri.IsAbsoluteUri && (uri.IsFile || uri.IsUnc))
                    {
                        normalizedSpecifier = uri.LocalPath;
                    }
                }
                // Currently, we do not filter on any extensions.
                var fileSpecifier = new FileSpecifier(normalizedSpecifier, recurse: analyzeOptions.Recurse, fileSystem: FileSystem);
                foreach (string file in fileSpecifier.Files)
                {
                    // Only include files that are below the max size limit.
                    if (ShouldEnqueue(file, _rootContext))
                    {
                        targets.Add(file);
                    }
                }
            }
            return targets;
        }

        private ISet<string> ValidateTargetsExist(TContext context, ISet<string> targets)
        {
            if (targets.Count == 0)
            {
                Errors.LogNoValidAnalysisTargets(context);
                ThrowExitApplicationException(context, ExitReason.NoValidAnalysisTargets);
            }

            return targets;
        }

        protected virtual TContext CreateContext(
            TOptions options,
            IAnalysisLogger logger,
            RuntimeConditions runtimeErrors,
            PropertiesDictionary policy = null,
            string filePath = null)
        {
            var context = new TContext
            {
                Logger = logger,
                RuntimeErrors = runtimeErrors,
                Policy = policy ?? new PropertiesDictionary()
            };

            context.Traces =
                options.Traces.Any() ?
                    (DefaultTraces)Enum.Parse(typeof(DefaultTraces), string.Join(",", options.Traces)) :
                    DefaultTraces.None;

            context.MaxFileSizeInKilobytes =
                options.MaxFileSizeInKilobytes >= 0
                ? options.MaxFileSizeInKilobytes
                : AnalyzeContextBase.MaxFileSizeInKilobytesDefaultValue;

            if (filePath != null)
            {
                context.TargetUri = new Uri(filePath);

                if ((options.DataToInsert.ToFlags() & OptionallyEmittedData.Hashes) != 0)
                {
                    if (_pathToHashDataMap != null && _pathToHashDataMap.TryGetValue(filePath, out HashData hashData))
                    {
                        context.Hashes = hashData;
                    }
                    else
                    {
                        context.Hashes = HashUtilities.ComputeHashes(filePath);
                        _pathToHashDataMap?.Add(filePath, context.Hashes);
                    }
                }
            }

            return context;
        }

        /// <summary>
        /// Calculate the file to load the configuration from.
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Configuration file path, or null if the built in configuration should be used.</returns>
        internal string GetConfigurationFileName(TOptions options)
        {
            if (options.ConfigurationFilePath == DefaultPolicyName)
            {
                return null;
            }

            if (string.IsNullOrEmpty(options.ConfigurationFilePath))
            {
                return !this.FileSystem.FileExists(this.DefaultConfigurationPath)
                    ? null
                    : DefaultConfigurationPath;
            }
            return options.ConfigurationFilePath;
        }

        protected virtual void InitializeConfiguration(TOptions options, TContext context)
        {
            context.Policy ??= new PropertiesDictionary();

            string configurationFileName = GetConfigurationFileName(options);
            if (string.IsNullOrEmpty(configurationFileName))
            {
                return;
            }

            string extension = Path.GetExtension(configurationFileName);

            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                context.Policy.LoadFromXml(configurationFileName);
            }
            else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                context.Policy.LoadFromJson(configurationFileName);
            }
            else if (ConfigurationFormat == FileFormat.Xml)
            {
                context.Policy.LoadFromXml(configurationFileName);
            }
            else
            {
                context.Policy.LoadFromJson(configurationFileName);
            }
        }

        private void InitializeOutputFile(TOptions analyzeOptions, TContext context)
        {
            string filePath = analyzeOptions.OutputFilePath;
            AggregatingLogger aggregatingLogger = (AggregatingLogger)context.Logger;

            if (!string.IsNullOrEmpty(filePath))
            {
                InvokeCatchingRelevantIOExceptions
                (
                    () =>
                    {
                        LogFilePersistenceOptions logFilePersistenceOptions = analyzeOptions.ConvertToLogFilePersistenceOptions();

                        OptionallyEmittedData dataToInsert = analyzeOptions.DataToInsert.ToFlags();
                        OptionallyEmittedData dataToRemove = analyzeOptions.DataToRemove.ToFlags();

                        SarifLogger sarifLogger;

                        _run = new Run()
                        {
                            AutomationDetails = new RunAutomationDetails
                            {
                                Id = analyzeOptions.AutomationId,
                                Guid = analyzeOptions.AutomationGuid
                            }
                        };

                        if (analyzeOptions.SarifOutputVersion != SarifVersion.OneZeroZero)
                        {
                            sarifLogger = new SarifLogger(
                                    analyzeOptions.OutputFilePath,
                                    logFilePersistenceOptions,
                                    dataToInsert,
                                    dataToRemove,
                                    tool: _tool,
                                    run: _run,
                                    analysisTargets: null,
                                    quiet: analyzeOptions.Quiet,
                                    invocationTokensToRedact: GenerateSensitiveTokensList(),
                                    invocationPropertiesToLog: analyzeOptions.InvocationPropertiesToLog,
                                    levels: analyzeOptions.Level,
                                    kinds: analyzeOptions.Kind);
                        }
                        else
                        {
                            sarifLogger = new SarifOneZeroZeroLogger(
                                    analyzeOptions.OutputFilePath,
                                    logFilePersistenceOptions,
                                    dataToInsert,
                                    dataToRemove,
                                    tool: _tool,
                                    run: _run,
                                    analysisTargets: null,
                                    invocationTokensToRedact: GenerateSensitiveTokensList(),
                                    invocationPropertiesToLog: analyzeOptions.InvocationPropertiesToLog,
                                    levels: analyzeOptions.Level,
                                    kinds: analyzeOptions.Kind);
                        }
                        _pathToHashDataMap = sarifLogger.AnalysisTargetToHashDataMap;
                        sarifLogger.AnalysisStarted();
                        aggregatingLogger.Loggers.Add(sarifLogger);
                    },
                    (ex) =>
                    {
                        Errors.LogExceptionCreatingLogFile(context, filePath, ex);
                        ThrowExitApplicationException(context, ExitReason.ExceptionCreatingLogFile, ex);
                    }
                );
            }
        }

        private IEnumerable<string> GenerateSensitiveTokensList()
        {
            var result = new List<string>
            {
                Environment.MachineName,
                Environment.UserName,
                Environment.UserDomainName
            };

            string userDnsDomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
            string logonServer = Environment.GetEnvironmentVariable("LOGONSERVER");

            if (!string.IsNullOrEmpty(userDnsDomain)) { result.Add(userDnsDomain); }
            if (!string.IsNullOrEmpty(logonServer)) { result.Add(logonServer); }

            return result;
        }

        public void InvokeCatchingRelevantIOExceptions(Action action, Action<Exception> exceptionHandler)
        {
            try
            {
                action();
            }
            catch (UnauthorizedAccessException ex)
            {
                exceptionHandler(ex);
            }
            catch (IOException ex)
            {
                exceptionHandler(ex);
            }
        }

        protected virtual ISet<Skimmer<TContext>> CreateSkimmers(TOptions analyzeOptions, TContext context)
        {
            IEnumerable<Skimmer<TContext>> skimmers;
            var result = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            try
            {
                skimmers = CompositionUtilities.GetExports<Skimmer<TContext>>(RetrievePluginAssemblies(DefaultPluginAssemblies, analyzeOptions.PluginFilePaths));

                SupportedPlatform currentOS = GetCurrentRunningOS();
                foreach (Skimmer<TContext> skimmer in skimmers)
                {
                    if (skimmer.SupportedPlatforms.HasFlag(currentOS))
                    {
                        result.Add(skimmer);
                    }
                    else
                    {
                        Warnings.LogUnsupportedPlatformForRule(context, skimmer.Name, skimmer.SupportedPlatforms, currentOS);
                    }
                }
            }
            catch (Exception ex)
            {
                Errors.LogExceptionInstantiatingSkimmers(context, DefaultPluginAssemblies, ex);
                ThrowExitApplicationException(context, ExitReason.UnhandledExceptionInstantiatingSkimmers, ex);
            }

            if (result.Count == 0)
            {
                Errors.LogNoRulesLoaded(context);
                ThrowExitApplicationException(context, ExitReason.NoRulesLoaded);
            }
            return result;
        }

        private SupportedPlatform GetCurrentRunningOS()
        {
            // RuntimeInformation is not present in NET452.
#if NET452
            return SupportedPlatform.Windows;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return SupportedPlatform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return SupportedPlatform.OSX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SupportedPlatform.Windows;
            }
            else
            {
                return SupportedPlatform.Unknown;
            }
#endif
        }

        protected virtual void AnalyzeTargets(
            TOptions options,
            IEnumerable<Skimmer<TContext>> skimmers,
            TContext rootContext,
            IEnumerable<string> targets)
        {
            var disabledSkimmers = new SortedSet<string>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                PerLanguageOption<RuleEnabledState> ruleEnabledProperty =
                    DefaultDriverOptions.CreateRuleSpecificOption(skimmer, DefaultDriverOptions.RuleEnabled);

                RuleEnabledState ruleEnabled = rootContext.Policy.GetProperty(ruleEnabledProperty);
                FailureLevel failureLevel = (ruleEnabled == RuleEnabledState.Default || ruleEnabled == RuleEnabledState.Disabled)
                    ? default
                    : (FailureLevel)Enum.Parse(typeof(FailureLevel), ruleEnabled.ToString());

                if (ruleEnabled == RuleEnabledState.Disabled)
                {
                    disabledSkimmers.Add(skimmer.Id);
                    Warnings.LogRuleExplicitlyDisabled(rootContext, skimmer.Id);
                    RuntimeErrors |= RuntimeConditions.RuleWasExplicitlyDisabled;
                }
                else if (!skimmer.DefaultConfiguration.Enabled && ruleEnabled == RuleEnabledState.Default)
                {
                    // This skimmer is disabled by default, and the configuration file didn't mention it.
                    // So disable it, but don't complain that the rule was explicitly disabled.
                    disabledSkimmers.Add(skimmer.Id);
                }
                else if (skimmer.DefaultConfiguration.Level != failureLevel
                    && ruleEnabled != RuleEnabledState.Default
                    && ruleEnabled != RuleEnabledState.Disabled)
                {
                    skimmer.DefaultConfiguration.Level = failureLevel;
                }
            }

            this.CheckIncompatibleRules(skimmers, rootContext, disabledSkimmers);

            if (disabledSkimmers.Count == skimmers.Count())
            {
                Errors.LogAllRulesExplicitlyDisabled(rootContext);
                ThrowExitApplicationException(rootContext, ExitReason.NoRulesLoaded);
            }

            if ((options.DataToInsert.ToFlags() & OptionallyEmittedData.Hashes) != 0)
            {
                // If analysis is persisted to a disk log file, we will have already
                // computed all file hashes and stored them to _pathToHashDataMap.
                _pathToHashDataMap = _pathToHashDataMap ?? HashUtilities.MultithreadedComputeTargetFileHashes(targets, options.Quiet);
            }

            foreach (string target in targets)
            {
                using (TContext context = DetermineApplicabilityAndAnalyze(options, skimmers, rootContext, target, disabledSkimmers))
                {
                    RuntimeErrors |= context.RuntimeErrors;
                }
            }
        }

        protected virtual TContext DetermineApplicabilityAndAnalyze(
            TOptions options,
            IEnumerable<Skimmer<TContext>> skimmers,
            TContext rootContext,
            string target,
            ISet<string> disabledSkimmers)
        {
            TContext context = CreateContext(
                options,
                rootContext.Logger,
                rootContext.RuntimeErrors,
                rootContext.Policy,
                target);

            if ((options.DataToInsert.ToFlags() & OptionallyEmittedData.Hashes) != 0)
            {
                _cacheByFileHashLogger.HashToResultsMap.TryGetValue(context.Hashes.Sha256, out List<Tuple<ReportingDescriptor, Result>> cachedResultTuples);
                _cacheByFileHashLogger.HashToNotificationsMap.TryGetValue(context.Hashes.Sha256, out List<Notification> cachedNotifications);

                bool replayCachedData = (cachedResultTuples != null || cachedNotifications != null);

                if (replayCachedData)
                {
                    context.Logger.AnalyzingTarget(context);

                    if (cachedResultTuples != null)
                    {
                        foreach (Tuple<ReportingDescriptor, Result> cachedResultTuple in cachedResultTuples)
                        {
                            Result clonedResult = cachedResultTuple.Item2.DeepClone();
                            ReportingDescriptor cachedReportingDescriptor = cachedResultTuple.Item1;

                            UpdateLocationsAndMessageWithCurrentUri(clonedResult.Locations, clonedResult.Message, context.TargetUri);
                            context.Logger.Log(cachedReportingDescriptor, clonedResult);
                        }
                    }

                    if (cachedNotifications != null)
                    {
                        foreach (Notification cachedNotification in cachedNotifications)
                        {
                            Notification clonedNotification = cachedNotification.DeepClone();
                            UpdateLocationsAndMessageWithCurrentUri(clonedNotification.Locations, cachedNotification.Message, context.TargetUri);
                            context.Logger.LogConfigurationNotification(clonedNotification);
                        }
                    }
                    return context;
                }
            }

            if (context.TargetLoadException != null)
            {
                Errors.LogExceptionLoadingTarget(context);
                context.Dispose();
                return context;
            }
            else if (!context.IsValidAnalysisTarget)
            {
                Warnings.LogExceptionInvalidTarget(context);
                context.Dispose();
                return context;
            }

            context.Logger.AnalyzingTarget(context);
            IEnumerable<Skimmer<TContext>> applicableSkimmers = DetermineApplicabilityForTarget(skimmers, context, disabledSkimmers);
            AnalyzeTarget(applicableSkimmers, context, disabledSkimmers);

            return context;
        }

        internal static void UpdateLocationsAndMessageWithCurrentUri(IList<Location> locations, Message message, Uri updatedUri)
        {
            if (locations == null) { return; }

            foreach (Location location in locations)
            {
                ArtifactLocation artifactLocation = location.PhysicalLocation?.ArtifactLocation;
                if (artifactLocation == null) { continue; }

                if (message == null)
                {
                    artifactLocation.Uri = updatedUri;
                    continue;
                }

                string oldFilePath = artifactLocation.Uri.OriginalString;
                string newFilePath = updatedUri.OriginalString;

                string oldFileName = GetFileNameFromUri(artifactLocation.Uri);
                string newFileName = GetFileNameFromUri(updatedUri);

                if (!string.IsNullOrEmpty(oldFileName) && !string.IsNullOrEmpty(newFileName))
                {
                    for (int i = 0; i < message?.Arguments?.Count; i++)
                    {
                        if (message.Arguments[i] == oldFileName)
                        {
                            message.Arguments[i] = newFileName;
                        }

                        if (message.Arguments[i] == oldFilePath)
                        {
                            message.Arguments[i] = newFilePath;
                        }
                    }

                    if (message.Text != null)
                    {
                        message.Text = message.Text.Replace(oldFilePath, newFilePath);
                        message.Text = message.Text.Replace(oldFileName, newFileName);
                    }
                }

                artifactLocation.Uri = updatedUri;
            }
        }

        internal static string GetFileNameFromUri(Uri uri)
        {
            if (uri == null) { return null; }

            return Path.GetFileName(uri.OriginalString);
        }

        protected virtual void AnalyzeTarget(IEnumerable<Skimmer<TContext>> skimmers, TContext context, ISet<string> disabledSkimmers)
        {
            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Contains(skimmer.Id)) { continue; }

                context.Rule = skimmer;

                try
                {
                    skimmer.Analyze(context);
                }
                catch (Exception ex)
                {
                    Errors.LogUnhandledRuleExceptionAnalyzingTarget(disabledSkimmers, context, ex);
                    RuntimeErrors |= context.RuntimeErrors;
                }
            }
        }

        protected virtual IEnumerable<Skimmer<TContext>> DetermineApplicabilityForTarget(
            IEnumerable<Skimmer<TContext>> skimmers,
            TContext context,
            ISet<string> disabledSkimmers)
        {
            var candidateSkimmers = new List<Skimmer<TContext>>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Contains(skimmer.Id)) { continue; }

                string reasonForNotAnalyzing = null;
                context.Rule = skimmer;

                AnalysisApplicability applicability = AnalysisApplicability.Unknown;

                try
                {
                    applicability = skimmer.CanAnalyze(context, out reasonForNotAnalyzing);
                }
                catch (Exception ex)
                {
                    Errors.LogUnhandledRuleExceptionAssessingTargetApplicability(disabledSkimmers, context, ex);
                    continue;
                }
                finally
                {
                    RuntimeErrors |= context.RuntimeErrors;
                }

                switch (applicability)
                {
                    case AnalysisApplicability.NotApplicableToSpecifiedTarget:
                    {
                        Notes.LogNotApplicableToSpecifiedTarget(context, reasonForNotAnalyzing);
                        break;
                    }

                    case AnalysisApplicability.ApplicableToSpecifiedTarget:
                    {
                        candidateSkimmers.Add(skimmer);
                        break;
                    }
                }
            }
            return candidateSkimmers;
        }

        protected void ThrowExitApplicationException(TContext context, ExitReason exitReason, Exception innerException = null)
        {
            RuntimeErrors |= context.RuntimeErrors;

            throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, innerException)
            {
                ExitReason = exitReason
            };
        }

        internal void CheckIncompatibleRules(IEnumerable<Skimmer<TContext>> skimmers, TContext context, ISet<string> disabledSkimmers)
        {
            var availableRules = new Dictionary<string, Skimmer<TContext>>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Contains(skimmer.Id))
                {
                    continue;
                }

                availableRules[skimmer.Id] = skimmer;
            }

            foreach (KeyValuePair<string, Skimmer<TContext>> entry in availableRules)
            {
                if (entry.Value.IncompatibleRuleIds?.Any() != true)
                {
                    continue;
                }

                foreach (string incompatibleRuleId in entry.Value.IncompatibleRuleIds)
                {
                    if (availableRules.ContainsKey(incompatibleRuleId))
                    {
                        Errors.LogIncompatibleRules(context, entry.Key, incompatibleRuleId);
                        ThrowExitApplicationException(context, ExitReason.IncompatibleRulesDetected);
                    }
                }
            }
        }

        protected virtual ISet<Skimmer<TContext>> InitializeSkimmers(ISet<Skimmer<TContext>> skimmers, TContext context)
        {
            SortedSet<Skimmer<TContext>> disabledSkimmers = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            // ONE-TIME initialization of skimmers. Do not
            // call more than once per skimmer instantiation.
            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                try
                {
                    context.Rule = skimmer;
                    skimmer.Initialize(context);
                }
                catch (Exception ex)
                {
                    RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
                    Errors.LogUnhandledExceptionInitializingRule(context, ex);
                    disabledSkimmers.Add(skimmer);
                }
            }

            foreach (Skimmer<TContext> disabledSkimmer in disabledSkimmers)
            {
                skimmers.Remove(disabledSkimmer);
            }

            return skimmers;
        }

        protected static void LogToolNotification(
            IAnalysisLogger logger,
            string message,
            FailureLevel level = FailureLevel.Note,
            Exception ex = null)
        {
            ExceptionData exceptionData = null;
            if (ex != null)
            {
                exceptionData = new ExceptionData
                {
                    Kind = ex.GetType().FullName,
                    Message = ex.Message,
                    Stack = Stack.CreateStacks(ex).FirstOrDefault()
                };
            }

            TextWriter writer = level == FailureLevel.Error ? Console.Error : Console.Out;
            writer.WriteLine(message);

            logger.LogToolNotification(new Notification
            {
                Level = level,
                Message = new Message { Text = message },
                Exception = exceptionData
            });
        }
    }
}
