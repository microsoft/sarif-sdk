// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class MultithreadedAnalyzeCommandBase<TContext, TOptions> : PluginDriverCommand<TOptions>
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
        internal ConcurrentDictionary<string, IAnalysisLogger> _analysisLoggerCache;

        private Run _run;
        private Tool _tool;
        private bool _computeHashes;
        private TContext _rootContext;
        private int _fileContextsCount;
        private Channel<int> _hashChannel;
        private OptionallyEmittedData _dataToInsert;
        private Channel<int> _resultsWritingChannel;
        private Channel<int> _fileEnumerationChannel;
        private ConcurrentDictionary<int, TContext> _fileContexts;
        private ConcurrentDictionary<string, List<string>> _hashToFilesMap;

        public Exception ExecutionException { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        public virtual FileFormat ConfigurationFormat { get { return FileFormat.Json; } }

        protected MultithreadedAnalyzeCommandBase(IFileSystem fileSystem = null)
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
            // Initialize an common logger that drives all outputs. This
            // object drives logging for console, statistics, etc.
            using (AggregatingLogger logger = InitializeLogger(options))
            {
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
                    ex = ex.InnerException ?? ex;

                    if (!(ex is ExitApplicationException<ExitReason>))
                    {
                        // These exceptions escaped our net and must be logged here
                        RuntimeErrors |= Errors.LogUnhandledEngineException(_rootContext, ex);
                    }
                    ExecutionException = ex;
                    return FAILURE;
                }
                finally
                {
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
            }

            if (options.RichReturnCode)
            {
                return (int)RuntimeErrors;
            }

            return succeeded ? SUCCESS : FAILURE;
        }

        private void Analyze(TOptions analyzeOptions, AggregatingLogger logger)
        {
            // 0. Log analysis initiation
            logger.AnalysisStarted();

            // 1. Create context object to pass to skimmers. The logger
            //    and configuration objects are common to all context
            //    instances and will be passed on again for analysis.
            _rootContext = CreateContext(analyzeOptions, logger, RuntimeErrors);

            // 2. Perform any command line argument validation beyond what
            //    the command line parser library is capable of.
            ValidateOptions(analyzeOptions, _rootContext);

            // 5. Initialize report file, if configured.
            InitializeOutputFile(analyzeOptions, _rootContext);

            // 6. Instantiate skimmers.
            ISet<Skimmer<TContext>> skimmers = CreateSkimmers(analyzeOptions, _rootContext);

            // 7. Initialize configuration. This step must be done after initializing
            //    the skimmers, as rules define their specific context objects and
            //    so those assemblies must be loaded.
            InitializeConfiguration(analyzeOptions, _rootContext);

            // 8. Initialize skimmers. Initialize occurs a single time only. This
            //    step needs to occurs after initializing configuration in order
            //    to allow command-line override of rule settings
            skimmers = InitializeSkimmers(skimmers, _rootContext);

            // 9. Run all multi-threaded analysis operations.
            AnalyzeTargets(analyzeOptions, _rootContext, skimmers);

            // 10. For test purposes, raise an unhandled exception if indicated
            if (RaiseUnhandledExceptionInDriverCode)
            {
                throw new InvalidOperationException(GetType().Name);
            }
        }

        private void MultithreadedAnalyzeTargets(TOptions options,
                                                 TContext rootContext,
                                                 IEnumerable<Skimmer<TContext>> skimmers,
                                                 ISet<string> disabledSkimmers)
        {
            options.Threads = options.Threads > 0 ?
                options.Threads :
                (Debugger.IsAttached) ? 1 : Environment.ProcessorCount;

            var channelOptions = new BoundedChannelOptions(2000)
            {
                SingleWriter = true,
                SingleReader = false,
            };
            _fileEnumerationChannel = Channel.CreateBounded<int>(channelOptions);
            _hashChannel = Channel.CreateBounded<int>(channelOptions);

            channelOptions = new BoundedChannelOptions(2000)
            {
                SingleWriter = false,
                SingleReader = true,
            };
            _resultsWritingChannel = Channel.CreateBounded<int>(channelOptions);

            var sw = Stopwatch.StartNew();

            var workers = new Task<bool>[options.Threads];
            var hashWorkers = new Task<bool>[options.Threads];

            for (int i = 0; i < options.Threads; i++)
            {
                hashWorkers[i] = HashAsync();
                workers[i] = AnalyzeTargetAsync(skimmers, disabledSkimmers);
            }

            Task<bool> findFiles = FindFilesAsync(options, rootContext);
            Task<bool> writeResults = WriteResultsAsync(rootContext);

            // FindFiles is single-thread and will close its write channel
            findFiles.Wait();

            Task.WhenAll(hashWorkers)
                .ContinueWith(_ => _fileEnumerationChannel.Writer.Complete())
                .Wait();

            Task.WhenAll(workers)
                .ContinueWith(_ => _resultsWritingChannel.Writer.Complete())
                .Wait();

            writeResults.Wait();

            Console.WriteLine();

            if (rootContext.Traces.HasFlag(DefaultTraces.ScanTime))
            {
                Console.WriteLine($"Done. {_fileContextsCount:n0} files scanned in {sw.Elapsed}.");
            }
            else
            {
                Console.WriteLine($"Done. {_fileContextsCount:n0} files scanned.");
            }
        }

        private async Task<bool> WriteResultsAsync(TContext rootContext)
        {
            int currentIndex = 0;

            ChannelReader<int> reader = _resultsWritingChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out int item))
                {
                    // This condition can occur if currentIndex moves
                    // ahead in array processing due to operations
                    // against it by other threads. For this case,
                    // since the relevant file has already been
                    // processed, we just ignore this notification.
                    if (currentIndex > item) { break; }

                    TContext context = default;
                    try
                    {
                        context = _fileContexts[currentIndex];

                        while (context?.AnalysisComplete == true)
                        {
                            if (_computeHashes)
                            {
                                bool cache = _analysisLoggerCache.TryGetValue(context.Hashes.Sha256, out IAnalysisLogger logger);
                                LogCachingLogger(rootContext, logger ?? context.Logger, context, clone: cache);
                            }
                            else
                            {
                                LogCachingLogger(rootContext, context.Logger, context);
                            }

                            RuntimeErrors |= context.RuntimeErrors;

                            context.Dispose();
                            _fileContexts.TryRemove(currentIndex, out _);
                            _fileContexts.TryGetValue(currentIndex + 1, out context);

                            currentIndex++;
                        }
                    }
                    catch (Exception e)
                    {
                        context?.Dispose();
                        context = default;
                        RuntimeErrors |= Errors.LogUnhandledEngineException(rootContext, e);
                        ThrowExitApplicationException(context, ExitReason.ExceptionWritingToLogFile, e);
                    }
                }
            }

            Debug.Assert(_fileContexts.Count == 0);
            Debug.Assert(_fileContextsCount == currentIndex);

            return true;
        }

        private void LogCachingLogger(TContext rootContext, IAnalysisLogger logger, TContext context, bool clone = false)
        {
            var cachingLogger = (CachingLogger)logger;
            IDictionary<ReportingDescriptor, IList<Result>> results = cachingLogger.Results;

            if (results?.Count > 0)
            {
                foreach (KeyValuePair<ReportingDescriptor, IList<Result>> kv in results)
                {
                    foreach (Result result in kv.Value)
                    {
                        Result currentResult = result;
                        if (clone)
                        {
                            Result clonedResult = result.DeepClone();

                            UpdateLocationsAndMessageWithCurrentUri(clonedResult.Locations, clonedResult.Message, context.TargetUri);

                            currentResult = clonedResult;
                        }

                        rootContext.Logger.Log(kv.Key, currentResult);
                    }
                }
            }

            if (cachingLogger.ToolNotifications != null)
            {
                foreach (Notification notification in cachingLogger.ToolNotifications)
                {
                    rootContext.Logger.LogToolNotification(notification);
                }
            }

            if (cachingLogger.ConfigurationNotifications != null)
            {
                foreach (Notification notification in cachingLogger.ConfigurationNotifications)
                {
                    Notification currentNotification = notification;
                    if (clone)
                    {
                        Notification clonedNotification = notification.DeepClone();
                        UpdateLocationsAndMessageWithCurrentUri(clonedNotification.Locations, notification.Message, context.TargetUri);

                        currentNotification = clonedNotification;
                    }

                    rootContext.Logger.LogConfigurationNotification(currentNotification);
                }
            }
        }

        private async Task<bool> FindFilesAsync(TOptions options, TContext rootContext)
        {
            this._fileContextsCount = 0;
            this._fileContexts = new ConcurrentDictionary<int, TContext>();
            this._hashToFilesMap = new ConcurrentDictionary<string, List<string>>();

            foreach (string specifier in options.TargetFileSpecifiers)
            {
                string normalizedSpecifier = Environment.ExpandEnvironmentVariables(specifier);

                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    if (uri.IsAbsoluteUri && (uri.IsFile || uri.IsUnc))
                    {
                        normalizedSpecifier = uri.LocalPath;
                    }
                }

                string filter = Path.GetFileName(normalizedSpecifier);
                string directory = Path.GetDirectoryName(normalizedSpecifier);

                if (directory.Length == 0)
                {
                    directory = $".{Path.DirectorySeparatorChar}";
                }

                directory = Path.GetFullPath(directory);
                var directories = new Queue<string>();

                if (options.Recurse)
                {
                    EnqueueAllDirectories(directories, directory);
                }
                else
                {
                    directories.Enqueue(directory);
                }

                var sortedFiles = new SortedSet<string>();

                while (directories.Count > 0)
                {
                    sortedFiles.Clear();

                    directory = Path.GetFullPath(directories.Dequeue());

#if NETFRAMEWORK
                    // .NET Framework: Directory.Enumerate with empty filter returns NO files.
                    // .NET Core: Directory.Enumerate with empty filter returns all files in directory.
                    // We will standardize on the .NET Core behavior.
                    if (string.IsNullOrEmpty(filter))
                    {
                        filter = "*";
                    }
#endif

                    foreach (string file in FileSystem.DirectoryEnumerateFiles(directory,
                                                                               filter,
                                                                               SearchOption.TopDirectoryOnly))
                    {
                        sortedFiles.Add(file);
                    }

                    foreach (string file in sortedFiles)
                    {
                        _fileContexts.TryAdd(
                            _fileContextsCount,
                            CreateContext(options,
                                          new CachingLogger(options.Level, options.Kind),
                                          rootContext.RuntimeErrors,
                                          rootContext.Policy,
                                          filePath: file)
                        );

                        await _hashChannel.Writer.WriteAsync(_fileContextsCount++);
                    }
                }
            }

            _hashChannel.Writer.Complete();

            if (_fileContextsCount == 0)
            {
                Errors.LogNoValidAnalysisTargets(rootContext);
                ThrowExitApplicationException(rootContext, ExitReason.NoValidAnalysisTargets);
            }

            return true;
        }

        private void EnqueueAllDirectories(Queue<string> queue, string directory)
        {
            var sortedDiskItems = new SortedSet<string>();

            queue.Enqueue(directory);
            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                sortedDiskItems.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItems)
            {
                EnqueueAllDirectories(queue, childDirectory);
            }
        }

        private async Task<bool> HashAsync()
        {
            ChannelReader<int> reader = _hashChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out int index))
                {
                    if (_computeHashes)
                    {
                        TContext context = _fileContexts[index];
                        string localPath = context.TargetUri.LocalPath;

                        HashData hashData = HashUtilities.ComputeHashes(localPath);

                        if (!_hashToFilesMap.TryGetValue(hashData.Sha256, out List<string> paths))
                        {
                            paths = new List<string>();
                            _hashToFilesMap[hashData.Sha256] = paths;
                        }

                        _run?.GetFileIndex(new ArtifactLocation { Uri = context.TargetUri },
                                           dataToInsert: _dataToInsert,
                                           hashData: hashData);

                        paths.Add(localPath);
                        context.Hashes = hashData;
                    }

                    await _fileEnumerationChannel.Writer.WriteAsync(index);
                }
            }

            return true;
        }

        private async Task<bool> AnalyzeTargetAsync(IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            ChannelReader<int> reader = _fileEnumerationChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out int item))
                {
                    TContext context = default;

                    try
                    {
                        context = _fileContexts[item];

                        DetermineApplicabilityAndAnalyze(context, skimmers, disabledSkimmers);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                    finally
                    {
                        if (context != null) { context.AnalysisComplete = true; }
                        await _resultsWritingChannel.Writer.WriteAsync(item);
                    }
                }
            }
            return true;
        }

        protected virtual void ValidateOptions(TOptions options, TContext context)
        {
            _dataToInsert = options.DataToInsert.ToFlags();
            _computeHashes = (_dataToInsert & OptionallyEmittedData.Hashes) != 0;

            bool succeeded = true;

            succeeded &= ValidateFile(context, options.OutputFilePath, shouldExist: null);
            succeeded &= ValidateFile(context, options.ConfigurationFilePath, shouldExist: true);
            succeeded &= ValidateFiles(context, options.PluginFilePaths, shouldExist: true);
            succeeded &= ValidateFile(context, options.BaselineSarifFile, shouldExist: true);
            succeeded &= ValidateInvocationPropertiesToLog(context, options.InvocationPropertiesToLog);
            succeeded &= ValidateOutputFileCanBeCreated(context, options.OutputFilePath, options.Force);
            succeeded &= options.ValidateOutputOptions(context);

            if (!succeeded)
            {
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }
        }

        private bool ValidateFiles(TContext context, IEnumerable<string> filePaths, bool shouldExist)
        {
            if (filePaths == null) { return true; }

            bool succeeded = true;

            foreach (string filePath in filePaths)
            {
                succeeded &= ValidateFile(context, filePath, shouldExist);
            }

            return succeeded;
        }

        private bool ValidateFile(TContext context, string filePath, bool? shouldExist)
        {
            if (filePath == null || filePath == DefaultPolicyName) { return true; }

            Exception exception = null;

            try
            {
                bool fileExists = FileSystem.FileExists(filePath);

                if (fileExists || shouldExist == null || !shouldExist.Value)
                {
                    return true;
                }

                Errors.LogMissingFile(context, filePath);
            }
            catch (IOException ex) { exception = ex; }
            catch (SecurityException ex) { exception = ex; }
            catch (UnauthorizedAccessException ex) { exception = ex; }

            if (exception != null)
            {
                Errors.LogExceptionAccessingFile(context, filePath, exception);
            }

            return false;
        }

        private static bool ValidateInvocationPropertiesToLog(TContext context, IEnumerable<string> propertiesToLog)
        {
            bool succeeded = true;

            if (propertiesToLog != null)
            {
                var validPropertyNames = new HashSet<string>(
                    typeof(Invocation).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(propInfo => propInfo.Name),
                    StringComparer.OrdinalIgnoreCase);

                foreach (string propertyName in propertiesToLog)
                {
                    if (!validPropertyNames.Contains(propertyName))
                    {
                        Errors.LogInvalidInvocationPropertyName(context, propertyName);
                        succeeded = false;
                    }
                }
            }

            return succeeded;
        }

        private bool ValidateOutputFileCanBeCreated(TContext context, string outputFilePath, bool force)
        {
            bool succeeded = true;

            if (!DriverUtilities.CanCreateOutputFile(outputFilePath, force, FileSystem))
            {
                Errors.LogOutputFileAlreadyExists(context, outputFilePath);
                succeeded = false;
            }

            return succeeded;
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
                _analysisLoggerCache = new ConcurrentDictionary<string, IAnalysisLogger>();
            }

            return logger;
        }

        protected virtual TContext CreateContext(TOptions options,
                                                 IAnalysisLogger logger,
                                                 RuntimeConditions runtimeErrors,
                                                 PropertiesDictionary policy = null,
                                                 string filePath = null)
        {
            var context = new TContext
            {
                Logger = logger,
                RuntimeErrors = runtimeErrors,
                Policy = policy
            };

            if (filePath != null)
            {
                context.TargetUri = new Uri(filePath);
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
                    : this.DefaultConfigurationPath;
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

                        OptionallyEmittedData dataToInsert = _dataToInsert;
                        OptionallyEmittedData dataToRemove = analyzeOptions.DataToRemove.ToFlags();

                        SarifLogger sarifLogger;

                        _run = new Run();

                        if (analyzeOptions.SarifOutputVersion != SarifVersion.OneZeroZero)
                        {
                            sarifLogger = new SarifLogger(analyzeOptions.OutputFilePath,
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
                                                          kinds: analyzeOptions.Kind,
                                                          insertProperties: analyzeOptions.InsertProperties);
                        }
                        else
                        {
                            sarifLogger = new SarifOneZeroZeroLogger(analyzeOptions.OutputFilePath,
                                                                     logFilePersistenceOptions,
                                                                     dataToInsert,
                                                                     dataToRemove,
                                                                     tool: _tool,
                                                                     run: _run,
                                                                     analysisTargets: null,
                                                                     invocationTokensToRedact: GenerateSensitiveTokensList(),
                                                                     invocationPropertiesToLog: analyzeOptions.InvocationPropertiesToLog,
                                                                     levels: analyzeOptions.Level,
                                                                     kinds: analyzeOptions.Kind,
                                                                     insertProperties: analyzeOptions.InsertProperties);
                        }
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

        protected virtual ISet<Skimmer<TContext>> CreateSkimmers(TOptions options, TContext context)
        {
            IEnumerable<Skimmer<TContext>> skimmers;
            var result = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            try
            {
                skimmers = CompositionUtilities.GetExports<Skimmer<TContext>>(RetrievePluginAssemblies(DefaultPluginAssemblies, options.PluginFilePaths));

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
            TContext rootContext,
            IEnumerable<Skimmer<TContext>> skimmers)
        {
            var disabledSkimmers = new SortedSet<string>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                PerLanguageOption<RuleEnabledState> ruleEnabledProperty =
                    DefaultDriverOptions.CreateRuleSpecificOption(skimmer, DefaultDriverOptions.RuleEnabled);

                RuleEnabledState ruleEnabled = rootContext.Policy.GetProperty(ruleEnabledProperty);

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
            }

            if (disabledSkimmers.Count == skimmers.Count())
            {
                Errors.LogAllRulesExplicitlyDisabled(rootContext);
                ThrowExitApplicationException(rootContext, ExitReason.NoRulesLoaded);
            }

            MultithreadedAnalyzeTargets(options, rootContext, skimmers, disabledSkimmers);
        }

        protected virtual TContext DetermineApplicabilityAndAnalyze(
            TContext context,
            IEnumerable<Skimmer<TContext>> skimmers,
            ISet<string> disabledSkimmers)
        {
            // insert results caching replay logic here

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

            IAnalysisLogger logger = context.Logger;

            int numberOfFiles = 1;
            if (_computeHashes)
            {
                numberOfFiles = _hashToFilesMap[context.Hashes.Sha256].Count;
                if (numberOfFiles > 1 && _analysisLoggerCache.ContainsKey(context.Hashes.Sha256))
                {
                    return context;
                }
            }

            context.Logger.AnalyzingTarget(context);

            if (_computeHashes && numberOfFiles > 1)
            {
                _analysisLoggerCache[context.Hashes.Sha256] = logger;
            }

            IEnumerable<Skimmer<TContext>> applicableSkimmers = DetermineApplicabilityForTarget(context, skimmers, disabledSkimmers);
            AnalyzeTarget(context, applicableSkimmers, disabledSkimmers);

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

        protected virtual void AnalyzeTarget(TContext context, IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            AnalyzeTargetHelper(context, skimmers, disabledSkimmers);
        }

        public static void AnalyzeTargetHelper(TContext context, IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Count > 0)
                {
                    lock (disabledSkimmers)
                    {
                        if (disabledSkimmers.Contains(skimmer.Id)) { continue; }
                    }
                }

                context.Rule = skimmer;

                try
                {
                    skimmer.Analyze(context);
                }
                catch (Exception ex)
                {
                    Errors.LogUnhandledRuleExceptionAnalyzingTarget(disabledSkimmers, context, ex);
                }
            }
        }

        protected virtual IEnumerable<Skimmer<TContext>> DetermineApplicabilityForTarget(
        TContext context,
        IEnumerable<Skimmer<TContext>> skimmers,
        ISet<string> disabledSkimmers)
        {
            skimmers = DetermineApplicabilityForTargetHelper(context, skimmers, disabledSkimmers);

            // TODO single-threaded write?
            RuntimeErrors |= context.RuntimeErrors;

            return skimmers;
        }

        public static IEnumerable<Skimmer<TContext>> DetermineApplicabilityForTargetHelper(
            TContext context,
            IEnumerable<Skimmer<TContext>> skimmers,
            ISet<string> disabledSkimmers)
        {
            var candidateSkimmers = new List<Skimmer<TContext>>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                if (disabledSkimmers.Count > 0)
                {
                    lock (disabledSkimmers)
                    {
                        if (disabledSkimmers.Contains(skimmer.Id)) { continue; }
                    }
                }

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
            if (context != null)
            {
                RuntimeErrors |= context.RuntimeErrors;
            }

            throw new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, innerException)
            {
                ExitReason = exitReason
            };
        }

        protected virtual ISet<Skimmer<TContext>> InitializeSkimmers(ISet<Skimmer<TContext>> skimmers, TContext context)
        {
            SortedSet<Skimmer<TContext>> disabledSkimmers = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            // ONE-TIME initialization of skimmers. Do not call
            // Initialize more than once per skimmer instantiation
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
