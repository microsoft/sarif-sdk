// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        internal static long DefaultMaxFileInKBValue = 1024;
        internal ConsoleLogger _consoleLogger;

        private Run _run;
        private Tool _tool;
        private bool _computeHashes;
        internal TContext _rootContext;
        private int _fileContextsCount;
        private uint _ignoredFilesCount;
        private Channel<int> readyToHashChannel;
        private OptionallyEmittedData _dataToInsert;
        private Channel<int> _resultsWritingChannel;
        private Channel<int> readyToScanChannel;
        private IDictionary<string, HashData> _pathToHashDataMap;
        private ConcurrentDictionary<int, TContext> _fileContexts;

        public Exception ExecutionException { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        public virtual FileFormat ConfigurationFormat => FileFormat.Json;

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
            try
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
                            Errors.LogUnhandledEngineException(_rootContext, ex);
                        }
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

                return options.RichReturnCode 
                    ? (int)RuntimeErrors
                    : succeeded ? SUCCESS : FAILURE;
            }
            finally
            {
                _rootContext?.Dispose();
            }
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
            options.Threads = options.Threads > 0
                ? options.Threads
                : (Debugger.IsAttached) ? 1 : Environment.ProcessorCount;

            var channelOptions = new BoundedChannelOptions(2000)
            {
                SingleWriter = true,
                SingleReader = true,
            };
            readyToHashChannel = Channel.CreateBounded<int>(channelOptions);

            channelOptions = new BoundedChannelOptions(2000)
            {
                SingleWriter = true,
                SingleReader = false,
            };
            readyToScanChannel = Channel.CreateBounded<int>(channelOptions);

            channelOptions = new BoundedChannelOptions(2000)
            {
                SingleWriter = false,
                SingleReader = true,
            };
            _resultsWritingChannel = Channel.CreateBounded<int>(channelOptions);

            var sw = Stopwatch.StartNew();

            // 1: First we initiate an asynchronous operation to locate disk files for
            // analysis, as specified in analysis configuration (file names, wildcards).
            Task<bool> enumerateFilesOnDisk = EnumerateFilesOnDiskAsync(options);

            // 2: Files found on disk are put in a specific sort order, after which a 
            // reference to each scan target is put into a channel for hashing,
            // if hashing is enabled.
            Task<bool> hashFilesAndPutInAnalysisQueue = HashFilesAndPutInAnalysisQueueAsnc();

            // 3: A dedicated set of threads pull scan targets and analyze them.
            //    On completing a scan, the thread writes the index of the 
            //    scanned item to a channel that drives logging.
            var workers = new Task<bool>[options.Threads];
            for (int i = 0; i < options.Threads; i++)
            {
                workers[i] = ScanTargetsAsync(skimmers, disabledSkimmers);
            }

            // 4: A single-threaded consumer watches for completed scans
            //    and logs results, if any. This operation is singlle-threaded
            //    in order to help ensure determinism in log output. i.e., any
            //    scan of the same targets using the same production code
            //    should produce a log file that is byte-for-byte identical
            //    to the previous output.
            Task<bool> logScanResults = LogScanResultsAsync(rootContext);

            Task.WhenAll(workers)
                .ContinueWith(_ => _resultsWritingChannel.Writer.Complete())
                .Wait();

            enumerateFilesOnDisk.Wait();
            hashFilesAndPutInAnalysisQueue.Wait();
            logScanResults.Wait();

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

        private async Task<bool> LogScanResultsAsync(TContext rootContext)
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
                            LogCachingLogger(rootContext, context, clone: _computeHashes);

                            RuntimeErrors |= context.RuntimeErrors;

                            context.Dispose();
                            _fileContexts.TryRemove(currentIndex, out _);
                            _fileContexts.TryGetValue(currentIndex + 1, out context);

                            currentIndex++;
                        }
                    }
                    catch (Exception e)
                    {
                        if (context != null)
                        {
                            RuntimeErrors |= context.RuntimeErrors;
                            context?.Dispose();
                        }
                        context = default;
                        Errors.LogUnhandledEngineException(rootContext, e);
                        RuntimeErrors |= rootContext.RuntimeErrors;
                        ThrowExitApplicationException(context, ExitReason.ExceptionWritingToLogFile, e);
                    }
                }
            }

            Debug.Assert(_fileContexts.IsEmpty);
            Debug.Assert(_fileContextsCount == currentIndex);

            return true;
        }

        private static void LogCachingLogger(TContext rootContext, TContext context, bool clone = false)
        {
            var cachingLogger = (CachingLogger)context.Logger;
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

        protected virtual bool ShouldEnqueue(string file, TContext context)
        {
            bool shouldEnqueue = IsTargetWithinFileSizeLimit(file, context.MaxFileSizeInKilobytes, out long fileSizeInKb);

            if (!shouldEnqueue)
            {
                Warnings.LogFileSkippedDueToSize(context, file, fileSizeInKb);
            }

            return shouldEnqueue;
        }

        private async Task<bool> EnumerateFilesOnDiskAsync(TOptions options)
        {
            try
            {
                this._fileContextsCount = 0;
                this._fileContexts = new ConcurrentDictionary<int, TContext>();

                // INTERESTING BREAKPOINT: debug 'ERR997.NoValidAnalysisTargets : No valid analysis targets were specified.'
                // Set a conditional breakpoint on 'matchExpression.Name' to filter by specific rules.
                // Set a conditional breakpoint on 'searchText' to filter on specific target text patterns.
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

                    if (!FileSystem.DirectoryExists(directory))
                    {
                        continue;
                    }

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

                        foreach (string file in FileSystem.DirectoryEnumerateFiles(directory, filter, SearchOption.TopDirectoryOnly))
                        {
                            // Only include files that are below the max size limit.
                            if (ShouldEnqueue(file, _rootContext))
                            {
                                sortedFiles.Add(file);
                                continue;
                            }
                            _ignoredFilesCount++;
                        }

                        foreach (string file in sortedFiles)
                        {
                            _fileContexts.TryAdd(
                                _fileContextsCount,
                                CreateContext(options,
                                              new CachingLogger(options.Level, options.Kind),
                                              _rootContext.RuntimeErrors,
                                              _rootContext.Policy,
                                              filePath: file)
                            );

                            await readyToHashChannel.Writer.WriteAsync(_fileContextsCount++);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Errors.LogUnhandledEngineException(_rootContext, e);
                ThrowExitApplicationException(_rootContext, ExitReason.UnhandledExceptionInEngine);
            }
            finally
            {
                readyToHashChannel.Writer.Complete();
            }

            if (_ignoredFilesCount > 0)
            {
                Warnings.LogOneOrMoreFilesSkippedDueToSize(_rootContext);
            }

            if (_fileContextsCount == 0)
            {
                Errors.LogNoValidAnalysisTargets(_rootContext);
                ThrowExitApplicationException(_rootContext, ExitReason.NoValidAnalysisTargets);
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

        private async Task<bool> HashFilesAndPutInAnalysisQueueAsnc()
        {
            ChannelReader<int> reader = readyToHashChannel.Reader;

            Dictionary<string, CachingLogger> loggerCache = null;

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

                        HashData hashData = HashUtilities.ComputeHashes(localPath, FileSystem);

                        context.Hashes = hashData;

                        if (_pathToHashDataMap != null && !_pathToHashDataMap.ContainsKey(localPath))
                        {
                            _pathToHashDataMap.Add(localPath, hashData);
                        }

                        loggerCache ??= new Dictionary<string, CachingLogger>();

                        if (!loggerCache.TryGetValue(hashData.Sha256, out CachingLogger logger))
                        {
                            logger = loggerCache[hashData.Sha256] = (CachingLogger)context.Logger;
                        }
                        context.Logger = logger;
                    }

                    await readyToScanChannel.Writer.WriteAsync(index);
                }
            }

            readyToScanChannel.Writer.Complete();

            return true;
        }

        private async Task<bool> ScanTargetsAsync(IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            ChannelReader<int> reader = readyToScanChannel.Reader;

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
            bool succeeded = true;

            succeeded &= ValidateFile(context, options.OutputFilePath, DefaultPolicyName, shouldExist: null);
            succeeded &= ValidateFile(context, options.ConfigurationFilePath, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateFile(context, options.BaselineSarifFile, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateFiles(context, options.PluginFilePaths, DefaultPolicyName, shouldExist: true);
            succeeded &= ValidateOutputFileCanBeCreated(context, options.OutputFilePath, options.Force);
            succeeded &= ValidateInvocationPropertiesToLog(context, options.InvocationPropertiesToLog);
            succeeded &= options.ValidateOutputOptions(context);
            succeeded &= options.MaxFileSizeInKilobytes > 0;

            if (!succeeded)
            {
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }
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

            _dataToInsert = analyzeOptions.DataToInsert.ToFlags();
            _computeHashes = (_dataToInsert & OptionallyEmittedData.Hashes) != 0;

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
                Policy = policy ?? new PropertiesDictionary(),
                Logger = logger,
                RuntimeErrors = runtimeErrors,
                MaxFileSizeInKilobytes = options.MaxFileSizeInKilobytes
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
            var aggregatingLogger = (AggregatingLogger)context.Logger;

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

        private static IEnumerable<string> GenerateSensitiveTokensList()
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

        private static SupportedPlatform GetCurrentRunningOS()
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

        protected virtual void AnalyzeTargets(TOptions options,
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

            var logger = (CachingLogger)context.Logger;
            logger.AnalyzingTarget(context);

            if (logger.CacheFinalized)
            {
                context.Logger = logger;
                logger.ReleaseLock();
                return context;
            }

            IEnumerable<Skimmer<TContext>> applicableSkimmers = DetermineApplicabilityForTarget(context, skimmers, disabledSkimmers);
            AnalyzeTarget(context, applicableSkimmers, disabledSkimmers);

            logger.ReleaseLock();
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
            var disabledSkimmers = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

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
