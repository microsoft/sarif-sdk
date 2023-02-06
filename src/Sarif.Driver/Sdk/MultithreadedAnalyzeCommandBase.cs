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
        internal ConsoleLogger _consoleLogger;

        private int _fileContextsCount;
        private Channel<int> readyToHashChannel;
        private Channel<int> _resultsWritingChannel;
        private Channel<int> readyToScanChannel;
        private IDictionary<string, HashData> _pathToHashDataMap;
        private ConcurrentDictionary<int, TContext> _fileContexts;

        public Exception ExecutionException { get; set; }

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        protected virtual Tool Tool { get; set; }

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
            return Run(options, out TContext globalContext);
        }

        public virtual int Run(TOptions options, out TContext globalContext, IAnalysisLogger logger = null)
        {
            bool succeeded = false;
            globalContext = default;

            logger ??= InitializeLogger(options);

            try
            {
                // 0. Log analysis initiation
                logger.AnalysisStarted();

                // 1. Create context object to pass to skimmers. The logger
                //    and configuration objects are common to all context
                //    instances and will be passed on again for analysis.
                globalContext = CreateContext(options, logger, 0);

                // 2. Perform any command line argument validation beyond what
                //    the command line parser library is capable of.
                ValidateOptions(options, globalContext);

                // 5. Instantiate skimmers. We need to do this before initializing
                //    the output file so that we can preconstruct the tool 
                //    extensions data written to the SARIF file. Due to this ordering, 
                //    we won't emit any failures or notifications in this operation 
                //    to the log file itself: it will only appear in console output.
                ISet<Skimmer<TContext>> skimmers = CreateSkimmers(options, globalContext);

                // 6. Initialize report file, if configured.
                InitializeOutputFile(options, globalContext);

                // 7. Initialize configuration. This step must be done after initializing
                //    the skimmers, as rules define their specific context objects and
                //    so those assemblies must be loaded.
                InitializeConfiguration(options, globalContext);

                // 8. Initialize skimmers. Initialize occurs a single time only. This
                //    step needs to occurs after initializing configuration in order
                //    to allow command-line override of rule settings
                skimmers = InitializeSkimmers(skimmers, globalContext);

                // 9. Run all multi-threaded analysis operations.
                AnalyzeTargets(options, globalContext, skimmers);

                // 10. For test purposes, raise an unhandled exception if indicated
                if (RaiseUnhandledExceptionInDriverCode)
                {
                    throw new InvalidOperationException(GetType().Name);
                }
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
                    globalContext ??= new TContext { Logger = logger };
                    Errors.LogUnhandledEngineException(globalContext, ex);
                }
                ExecutionException = ex;
                return FAILURE;
            }
            finally
            {
                logger.AnalysisStopped(globalContext.RuntimeErrors);
                globalContext?.Dispose();
            }

            succeeded = (globalContext.RuntimeErrors & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None;


            // TBD we should have a better pattern for these post-processing operations.
            if (succeeded)
            {
                try
                {
                    ProcessBaseline(globalContext);
                }
                catch (Exception ex)
                {
                    globalContext.RuntimeErrors |= RuntimeConditions.ExceptionProcessingBaseline;
                    ExecutionException = ex;
                    return FAILURE;
                }

                try
                {
                    PostLogFile(options.PostUri, options.OutputFilePath, FileSystem);
                }
                catch (Exception ex)
                {
                    globalContext.RuntimeErrors |= RuntimeConditions.ExceptionPostingLogFile;
                    ExecutionException = ex;
                    return FAILURE;
                }
            }
            return options.RichReturnCode
                ? (int)globalContext?.RuntimeErrors
                : succeeded ? SUCCESS : FAILURE;
        }

        private void MultithreadedAnalyzeTargets(TContext context,
                                                 IEnumerable<Skimmer<TContext>> skimmers,
                                                 ISet<string> disabledSkimmers)
        {
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
            Task<bool> enumerateFilesOnDisk = EnumerateFilesOnDiskAsync(context);

            // 2: Files found on disk are put in a specific sort order, after which a 
            // reference to each scan target is put into a channel for hashing,
            // if hashing is enabled.
            Task<bool> hashFilesAndPutInAnalysisQueue = HashFilesAndPutInAnalysisQueueAsnc(context);

            // 3: A dedicated set of threads pull scan targets and analyze them.
            //    On completing a scan, the thread writes the index of the 
            //    scanned item to a channel that drives logging.
            var workers = new Task<bool>[context.Threads];
            for (int i = 0; i < context.Threads; i++)
            {
                workers[i] = ScanTargetsAsync(context, skimmers, disabledSkimmers);
            }

            // 4: A single-threaded consumer watches for completed scans
            //    and logs results, if any. This operation is single-threaded
            //    in order to help ensure determinism in log output. i.e., any
            //    scan of the same targets using the same production code
            //    should produce a log file that is byte-for-byte identical
            //    to the previous output.
            Task<bool> logScanResults = LogScanResultsAsync(context);

            Task.WhenAll(workers)
                .ContinueWith(_ => _resultsWritingChannel.Writer.Complete())
                .Wait();

            enumerateFilesOnDisk.Wait();
            hashFilesAndPutInAnalysisQueue.Wait();
            logScanResults.Wait();

            Console.WriteLine();

            if (context.Traces.Contains(nameof(DefaultTraces.ScanTime)))
            {
                string timing = $"Done. {_fileContextsCount:n0} files scanned, elapsed time {sw.Elapsed}.";

                context.Logger.LogToolNotification(
                    new Notification
                    {
                        Level = FailureLevel.Warning,
                        Descriptor = new ReportingDescriptorReference
                        {
                            Id = $"TRC101.{nameof(DefaultTraces.ScanTime)}"
                        },
                        Message = new Message
                        {
                            Text = timing,
                        },
                        TimeUtc = DateTime.UtcNow,
                    });
            }
            else
            {
                Console.WriteLine($"Done. {_fileContextsCount:n0} files scanned.");
            }
        }

        private async Task<bool> LogScanResultsAsync(TContext globalContext)
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
                            bool clone = context.DataToInsert.HasFlag(OptionallyEmittedData.Hashes);
                            try
                            {
                                LogCachingLogger(globalContext, context, clone);
                            }
                            finally
                            {
                                context.Dispose();
                            }

                            _fileContexts.TryRemove(currentIndex, out _);
                            _fileContexts.TryGetValue(currentIndex + 1, out context);

                            currentIndex++;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Errors.LogAnalysisCanceled(context);
                        ThrowExitApplicationException(context, ExitReason.ExceptionWritingToLogFile);
                    }
                    catch (Exception e)
                    {
                        Errors.LogUnhandledEngineException(globalContext, e);
                        ThrowExitApplicationException(context, ExitReason.ExceptionWritingToLogFile, e);
                    }
                    finally
                    {
                        if (context != null)
                        {
                            globalContext.RuntimeErrors |= context.RuntimeErrors;
                            context.Dispose();
                        }
                    }
                }
            }

            if (!_fileContexts.IsEmpty || _fileContextsCount != currentIndex)
            {
                // If we have abandoned context state, we must have halted analysis
                // due to some catastrophic condition.
                Debug.Assert(
                    globalContext.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisCanceled) ||
                    globalContext.RuntimeErrors.HasFlag(RuntimeConditions.AnalysisTimedOut) ||
                    globalContext.RuntimeErrors.HasFlag(RuntimeConditions.ExceptionInEngine));
            }

            return true;
        }

        private static void LogCachingLogger(TContext rootContext, TContext context, bool clone = false)
        {
            var cachingLogger = (CachingLogger)context.Logger;
            IDictionary<ReportingDescriptor, IList<Tuple<Result, int?>>> results = cachingLogger.Results;

            if (results?.Count > 0)
            {
                foreach (KeyValuePair<ReportingDescriptor, IList<Tuple<Result, int?>>> kv in results)
                {
                    foreach (Tuple<Result, int?> tuple in kv.Value)
                    {
                        Result result = tuple.Item1;
                        Result currentResult = result;
                        if (clone)
                        {
                            Result clonedResult = result.DeepClone();

                            UpdateLocationsAndMessageWithCurrentUri(clonedResult.Locations, clonedResult.Message, context.CurrentTarget.Uri);

                            currentResult = clonedResult;
                        }

                        rootContext.Logger.Log(kv.Key, currentResult, tuple.Item2);
                    }
                }
            }

            if (cachingLogger.ToolNotifications != null)
            {
                foreach (Tuple<Notification, ReportingDescriptor> tuple in cachingLogger.ToolNotifications)
                {
                    rootContext.Logger.LogToolNotification(tuple.Item1, tuple.Item2);
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
                        UpdateLocationsAndMessageWithCurrentUri(clonedNotification.Locations, notification.Message, context.CurrentTarget.Uri);

                        currentNotification = clonedNotification;
                    }

                    rootContext.Logger.LogConfigurationNotification(currentNotification);
                }
            }
        }

        protected virtual bool ShouldComputeHashes(string file, TContext context)
        {
            return true;
        }

        private async Task<bool> EnumerateFilesOnDiskAsync(TContext context)
        {
            try
            {
                this._fileContextsCount = 0;
                this._fileContexts = new ConcurrentDictionary<int, TContext>();

                await EnumerateFilesFromArtifactsProvider(context);
            }
            catch (OperationCanceledException)
            {
                Errors.LogAnalysisCanceled(context);
                ThrowExitApplicationException(context, ExitReason.ExceptionWritingToLogFile);

            }
            catch (Exception e)
            {
                lock (context)
                {
                    Errors.LogUnhandledEngineException(context, e);
                }
                ThrowExitApplicationException(context, ExitReason.UnhandledExceptionInEngine);
            }
            finally
            {
                readyToHashChannel.Writer.Complete();
            }

            if (context.TargetsProvider.Skipped?.Count > 0)
            {
                Warnings.LogOneOrMoreFilesSkippedDueToSize(context);

                foreach (IEnumeratedArtifact artifact in context.TargetsProvider.Skipped)
                {
                    Notes.LogFileSkippedDueToSize(context, artifact.Uri.GetFilePath(), (long)artifact.SizeInBytes);
                }
            }

            if (_fileContextsCount == 0)
            {
                Errors.LogNoValidAnalysisTargets(context);
                ThrowExitApplicationException(context, ExitReason.NoValidAnalysisTargets);
            }

            return true;
        }

        private async Task<bool> EnumerateFilesFromArtifactsProvider(TContext context)
        {
            foreach (IEnumeratedArtifact artifact in context.TargetsProvider.Artifacts)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                TContext fileContext =
                    CreateContext(options: null,
                                  new CachingLogger(context.FailureLevels, context.ResultKinds),
                                  context.RuntimeErrors,
                                  context.Policy);

                // TBD: Push current target down into base?
                fileContext.CurrentTarget = artifact;
                fileContext.CancellationToken = context.CancellationToken;

                bool added = _fileContexts.TryAdd(_fileContextsCount, fileContext);
                Debug.Assert(added);

                await readyToHashChannel.Writer.WriteAsync(_fileContextsCount++);
            }

            // TBD get all skipped artifacts.

            return true;
        }

        private void EnqueueAllDirectories(TContext context, Queue<string> queue, string directory)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var sortedDiskItems = new SortedSet<string>();

            queue.Enqueue(directory);
            foreach (string childDirectory in FileSystem.DirectoryEnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                sortedDiskItems.Add(childDirectory);
            }

            foreach (string childDirectory in sortedDiskItems)
            {
                EnqueueAllDirectories(context, queue, childDirectory);
            }
        }

        private async Task<bool> HashFilesAndPutInAnalysisQueueAsnc(TContext globalContext)
        {
            ChannelReader<int> reader = readyToHashChannel.Reader;

            Dictionary<string, CachingLogger> loggerCache = null;
            bool computeHashes = globalContext.DataToInsert.HasFlag(OptionallyEmittedData.Hashes);

            try
            {
                // Wait until there is work or the channel is closed.
                while (await reader.WaitToReadAsync())
                {
                    // Loop while there is work to do.
                    while (reader.TryRead(out int index))
                    {
                        if (computeHashes)
                        {
                            TContext context = _fileContexts[index];

                            context.CancellationToken.ThrowIfCancellationRequested();

                            string localPath = context.CurrentTarget.Uri.GetFilePath();

                            HashData hashData = ShouldComputeHashes(localPath, globalContext)
                                ? HashUtilities.ComputeHashes(localPath, FileSystem)
                                : null;

                            context.Hashes = hashData;

                            if (_pathToHashDataMap != null && !_pathToHashDataMap.ContainsKey(localPath))
                            {
                                _pathToHashDataMap.Add(localPath, hashData);
                            }

                            loggerCache ??= new Dictionary<string, CachingLogger>();

                            if (hashData?.Sha256 != null)
                            {
                                context.Logger = loggerCache.TryGetValue(hashData.Sha256, out CachingLogger logger)
                                    ? logger
                                    : (loggerCache[hashData.Sha256] = (CachingLogger)context.Logger);
                            }
                        }

                        await readyToScanChannel.Writer.WriteAsync(index);
                    }
                }
            }
            catch (Exception e)
            {
                lock (globalContext)
                {
                    Errors.LogUnhandledEngineException(globalContext, e);
                }

                ThrowExitApplicationException(globalContext, ExitReason.UnhandledExceptionInEngine);
            }
            finally
            {
                readyToScanChannel.Writer.Complete();
            }

            return true;
        }

        private async Task<bool> ScanTargetsAsync(TContext globalContext, IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            ChannelReader<int> reader = readyToScanChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out int item))
                {
                    TContext perFileContext = default;

                    try
                    {
                        perFileContext = _fileContexts[item];
                        perFileContext.CancellationToken.ThrowIfCancellationRequested();
                        DetermineApplicabilityAndAnalyze(perFileContext, skimmers, disabledSkimmers);
                        globalContext.RuntimeErrors |= perFileContext.RuntimeErrors;
                    }
                    catch (OperationCanceledException)
                    {
                        Errors.LogAnalysisCanceled(globalContext);
                        ThrowExitApplicationException(globalContext, ExitReason.AnalysisCanceled);
                    }
                    catch (Exception e)
                    {
                        Errors.LogUnhandledEngineException(globalContext, e);
                        ThrowExitApplicationException(globalContext, ExitReason.UnhandledExceptionInEngine);
                    }
                    finally
                    {
                        if (perFileContext != null) { perFileContext.AnalysisComplete = true; }
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

            succeeded &= context.MaxFileSizeInKilobytes >= 0;

            if (!succeeded)
            {
                ThrowExitApplicationException(context, ExitReason.InvalidCommandLineOption);
            }
        }

        internal AggregatingLogger InitializeLogger(AnalyzeOptionsBase analyzeOptions)
        {
            Tool ??= Tool.CreateFromAssemblyData();

            var logger = new AggregatingLogger();

            if (!analyzeOptions.Quiet)
            {
                _consoleLogger = new ConsoleLogger(analyzeOptions.Quiet, Tool.Driver.Name, analyzeOptions.Level, analyzeOptions.Kind) { CaptureOutput = _captureConsoleOutput };
                logger.Loggers.Add(_consoleLogger);
            }

            return logger;
        }

        protected virtual TContext CreateContext(TOptions options,
                                                 IAnalysisLogger logger,
                                                 RuntimeConditions runtimeErrors,
                                                 PropertiesDictionary policy = null)
        {
            var context = new TContext
            {
                Logger = logger,
                FileSystem = FileSystem,
                RuntimeErrors = runtimeErrors,
                Policy = policy ?? new PropertiesDictionary(),
            };

            if (options != null)
            {
                context.Traces =
                    options.Traces.Any() == true ?
                       new StringSet(options.Traces) :
                       new StringSet();

                if (options.MaxFileSizeInKilobytes != -1)
                {
                    context.MaxFileSizeInKilobytes = options.MaxFileSizeInKilobytes;
                }

                if (options.TargetFileSpecifiers != null)
                {
                    context.TargetsProvider =
                        OrderedFileSpecifier.Create(
                            options.TargetFileSpecifiers,
                            options.Recurse,
                            context.MaxFileSizeInKilobytes,
                            FileSystem);
                }

                context.Inline = options.Inline;
                context.OutputFilePath = options.OutputFilePath;
                context.DataToInsert = options.DataToInsert.ToFlags();
                context.BaselineFilePath = options.BaselineSarifFile;
                context.ResultKinds = new HashSet<ResultKind>(options.Kind);
                context.FailureLevels = new HashSet<FailureLevel>(options.Level);
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

            var configuration = new PropertiesDictionary();
            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                configuration.LoadFromXml(configurationFileName);
            }
            else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                configuration.LoadFromJson(configurationFileName);
            }
            else if (ConfigurationFormat == FileFormat.Xml)
            {
                configuration.LoadFromXml(configurationFileName);
            }
            else
            {
                configuration.LoadFromJson(configurationFileName);
            }
            configuration.MergePreferFirst(context.Policy);
            context.Policy = configuration;
        }

        public virtual void InitializeOutputFile(TOptions analyzeOptions, TContext context)
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

                        OptionallyEmittedData dataToInsert = context.DataToInsert;
                        OptionallyEmittedData dataToRemove = analyzeOptions.DataToRemove.ToFlags();

                        SarifLogger sarifLogger;

                        var run = new Run()
                        {
                            AutomationDetails = new RunAutomationDetails
                            {
                                Id = analyzeOptions.AutomationId,
                                Guid = analyzeOptions.AutomationGuid
                            },
                            Tool = Tool,
                        };

                        if (analyzeOptions.SarifOutputVersion != SarifVersion.OneZeroZero)
                        {
                            sarifLogger = new SarifLogger(analyzeOptions.OutputFilePath,
                                                          logFilePersistenceOptions,
                                                          dataToInsert,
                                                          dataToRemove,
                                                          run,
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
                                                                     run,
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
                                              TContext context,
                                              IEnumerable<Skimmer<TContext>> skimmers)
        {
            if (skimmers == null)
            {
                Errors.LogNoRulesLoaded(context);
                ThrowExitApplicationException(context, ExitReason.NoRulesLoaded);
            }

            var disabledSkimmers = new SortedSet<string>();

            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                PerLanguageOption<RuleEnabledState> ruleEnabledProperty =
                    DefaultDriverOptions.CreateRuleSpecificOption(skimmer, DefaultDriverOptions.RuleEnabled);

                RuleEnabledState ruleEnabled = context.Policy.GetProperty(ruleEnabledProperty);

                FailureLevel failureLevel = (ruleEnabled == RuleEnabledState.Default || ruleEnabled == RuleEnabledState.Disabled)
                    ? default
                    : (FailureLevel)Enum.Parse(typeof(FailureLevel), ruleEnabled.ToString());

                if (ruleEnabled == RuleEnabledState.Disabled)
                {
                    disabledSkimmers.Add(skimmer.Id);
                    Warnings.LogRuleExplicitlyDisabled(context, skimmer.Id);
                    context.RuntimeErrors |= RuntimeConditions.RuleWasExplicitlyDisabled;
                }
                else if (!skimmer.DefaultConfiguration.Enabled && ruleEnabled == RuleEnabledState.Default)
                {
                    // This skimmer is disabled by default, and the configuration file didn't mention it.
                    // So disable it, but don't complain that the rule was explicitly disabled.
                    disabledSkimmers.Add(skimmer.Id);
                }
                else if (skimmer.DefaultConfiguration.Level != failureLevel &&
                         ruleEnabled != RuleEnabledState.Default &&
                         ruleEnabled != RuleEnabledState.Disabled)
                {
                    skimmer.DefaultConfiguration.Level = failureLevel;
                }
            }

            if (disabledSkimmers.Count == skimmers.Count())
            {
                Errors.LogAllRulesExplicitlyDisabled(context);
                ThrowExitApplicationException(context, ExitReason.NoRulesLoaded);
            }

            this.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            MultithreadedAnalyzeTargets(context, skimmers, disabledSkimmers);
        }

        protected virtual TContext DetermineApplicabilityAndAnalyze(TContext context,
                                                                    IEnumerable<Skimmer<TContext>> skimmers,
                                                                    ISet<string> disabledSkimmers)
        {
            // insert results caching replay logic here

            if (context.TargetLoadException != null)
            {
                Errors.LogExceptionLoadingTarget(context);
                return context;
            }
            else if (!context.IsValidAnalysisTarget)
            {
                Warnings.LogExceptionInvalidTarget(context);
                return context;
            }

            var logger = (CachingLogger)(CachingLogger)context.Logger;
            logger.AnalyzingTarget(context);

            if (logger.CacheFinalized)
            {
                context.Logger = logger;
                logger.ReleaseLock();
                return context;
            }

            context.CancellationToken.ThrowIfCancellationRequested();
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
                context.CancellationToken.ThrowIfCancellationRequested();
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
                    Stopwatch stopwatch = context.Traces.Contains(nameof(DefaultTraces.RuleScanTime))
                        ? Stopwatch.StartNew()
                        : null;

                    skimmer.Analyze(context);

                    Uri uri = context.CurrentTarget != null
                        ? context.CurrentTarget.Uri
                        : context.CurrentTarget.Uri;

                    if (stopwatch != null && uri.IsAbsoluteUri)
                    {
                        string file = uri.LocalPath;
                        string directory = Path.GetDirectoryName(file);
                        file = Path.GetFileName(file);
                        string timing = $"'{file}' : elapsed {stopwatch.Elapsed} : '{skimmer.Name}' : at '{directory}'";

                        context.Logger.LogToolNotification(
                            new Notification
                            {
                                Level = FailureLevel.Note,
                                Descriptor = new ReportingDescriptorReference
                                {
                                    Id = $"TRC101.{nameof(DefaultTraces.RuleScanTime)}"
                                },
                                Message = new Message
                                {
                                    Text = timing,
                                },
                                TimeUtc = DateTime.UtcNow,
                            },
                            skimmer);
                    }

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
            return DetermineApplicabilityForTargetHelper(context, skimmers, disabledSkimmers);
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

        protected void ThrowExitApplicationException(TContext _, ExitReason exitReason, Exception innerException = null)
        {
            // TBD context is unused!
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
                    context.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
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
