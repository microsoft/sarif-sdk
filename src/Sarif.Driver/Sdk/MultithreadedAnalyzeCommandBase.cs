// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class MultithreadedAnalyzeCommandBase<TContext, TOptions> : PluginDriverCommand<TOptions>
        where TContext : AnalyzeContextBase, new()
        where TOptions : AnalyzeOptionsBase, new()
    {
        public const string DefaultPolicyName = "default";

        // This is a unit test knob that configures the console output logger to capture its output
        // in a string builder. This is important to do because tooling behavior differs in non-trivial
        // ways depending on whether output is captured to a log file disk or not. In the latter case,
        // the captured output is useful to verify behavior.
        internal bool _captureConsoleOutput;
        internal ConsoleLogger _consoleLogger;

        private uint _fileContextsCount;
        private long _filesMatchingGlobalFileDenyRegex;
        private long _filesExceedingSizeLimitCount;
        private Channel<uint> _resultsWritingChannel;
        private Channel<uint> readyToScanChannel;
        private ConcurrentDictionary<uint, TContext> _fileContexts;

        public static bool RaiseUnhandledExceptionInDriverCode { get; set; }

        public virtual Tool Tool { get; set; }

        public virtual FileFormat ConfigurationFormat => FileFormat.Json;

        protected MultithreadedAnalyzeCommandBase(IFileSystem fileSystem = null)
        {
            Tool ??= Tool.CreateFromAssemblyData();
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
            TContext context = default;
            return Run(options, ref context);
        }

        public virtual int Run(TOptions options, ref TContext globalContext)
        {
            try
            {
                globalContext ??= new TContext();
                options ??= new TOptions();
                globalContext = InitializeGlobalContextFromOptions(options, ref globalContext);

                // We must make a copy of the global context reference
                // to utilize it in a separate thread.
                TContext methodLocalContext = globalContext;

                if (!string.IsNullOrEmpty(globalContext.EventsFilePath))
                {
                    Guid guid = EventSource.GetGuid(typeof(DriverEventSource));

                    if (globalContext.EventsFilePath.Equals("console", StringComparison.OrdinalIgnoreCase))
                    {
                        globalContext.TraceEventSession = new TraceEventSession($"Sarif-Driver-{Guid.NewGuid()}");
                        globalContext.TraceEventSession.BufferSizeMB = globalContext.EventsBufferSizeInMegabytes;
                        TraceEventSession traceEventSession = globalContext.TraceEventSession;
                        globalContext.TraceEventSession.Source.Dynamic.All += (e =>
                        {
                            Console.WriteLine($"{e.TimeStamp:MM/dd/yyyy hh:mm:ss.ffff},{e.ThreadID}," +
                            $"{e.ProcessorNumber},{e.EventName},{e.TimeStampRelativeMSec},{e.FormattedMessage}");

                            if (e.EventName.Equals("SessionEnded"))
                            {
                                traceEventSession.Dispose();
                            }
                        });
                        traceEventSession.EnableProvider(guid);
                    }
                    else
                    {
                        string etlFilePath =
                        Path.GetExtension(globalContext.EventsFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase)
                            ? $"{Path.GetFileNameWithoutExtension(globalContext.EventsFilePath)}.etl"
                            : globalContext.EventsFilePath;

                        globalContext.TraceEventSession = new TraceEventSession($"Sarif-Driver-{Guid.NewGuid()}", etlFilePath);
                        globalContext.TraceEventSession.BufferSizeMB = globalContext.EventsBufferSizeInMegabytes;
                        globalContext.TraceEventSession.EnableProvider(guid);
                    }
                }

                Task<int> analyzeTask = Task.Run(() =>
                {
                    return Run(methodLocalContext);
                }, globalContext.CancellationToken);

                int msDelay = globalContext.TimeoutInMilliseconds;
                int result = FAILURE;

                if (Task.WhenAny(analyzeTask, Task.Delay(msDelay)).GetAwaiter().GetResult() == analyzeTask)
                {
                    result = analyzeTask.Result;
                }
                else
                {
                    lock (globalContext)
                    {
                        Errors.LogAnalysisTimedOut(globalContext);
                    }
                }

                DriverEventSource.Log.SessionEnded(result, globalContext.RuntimeErrors);
                return result;
            }
            catch (Exception ex)
            {
                globalContext.RuntimeExceptions ??= new List<Exception>();
                ProcessException(globalContext, ex);
            }
            finally
            {
                globalContext.Dispose();
            }

            return
                globalContext.RichReturnCode == true
                    ? (int)globalContext.RuntimeErrors
                    : FAILURE;
        }

        private void ProcessException(TContext globalContext, Exception ex)
        {
            if (ex is ExitApplicationException<ExitReason> eae)
            {
                globalContext.RuntimeExceptions.Add(ex);
                return;
            }

            if (ex is AggregateException ae)
            {
                foreach (Exception innerException in ae.InnerExceptions)
                {
                    ProcessException(globalContext, innerException);
                }
                return;
            }

            if (ex.InnerException != null)
            {
                ProcessException(globalContext, ex.InnerException);
                return;
            }

            if (ex is OperationCanceledException oce)
            {
                lock (globalContext)
                {
                    globalContext.RuntimeExceptions.Add(oce);
                }

                Errors.LogAnalysisCanceled(globalContext, oce);
                globalContext.RuntimeExceptions.Add(new ExitApplicationException<ExitReason>(SdkResources.ERR999_AnalysisCanceled, oce)
                {
                    ExitReason = ExitReason.AnalysisCanceled
                });
                return;
            }

            lock (globalContext)
            {
                Errors.LogUnhandledEngineException(globalContext, ex);
            }

            globalContext.RuntimeExceptions.Add(new ExitApplicationException<ExitReason>(DriverResources.MSG_UnexpectedApplicationExit, ex)
            {
                ExitReason = ExitReason.UnhandledExceptionInEngine
            });
        }

        private int Run(TContext globalContext)
        {
            bool succeeded;
            IDisposable disposableLogger;

            globalContext.FileSystem ??= FileSystem;
            globalContext = ValidateContext(globalContext);
            disposableLogger = globalContext.Logger as IDisposable;
            InitializeOutputs(globalContext);

            // 1. Instantiate skimmers. We need to do this before initializing
            //    the output file so that we can preconstruct the tool 
            //    extensions data written to the SARIF file. Due to this ordering, 
            //    we won't emit any failures or notifications in this operation 
            //    to the log file itself: it will only appear in console output.
            ISet<Skimmer<TContext>> skimmers = CreateSkimmers(globalContext);

            // 2. Initialize skimmers. Initialize occurs a single time only. This
            //    step needs to occur after initializing configuration in order
            //    to allow command-line override of rule settings
            skimmers = InitializeSkimmers(skimmers, globalContext);

            // 3. Log analysis initiation
            globalContext.Logger.AnalysisStarted();

            // 4. Run all multi-threaded analysis operations.
            AnalyzeTargets(globalContext, skimmers);

            // 5. For test purposes, raise an unhandled exception if indicated
            if (RaiseUnhandledExceptionInDriverCode)
            {
                throw new InvalidOperationException(GetType().Name);
            }

            // Analysis is complete. Generate our stopped event and also dispose
            // of any disposable logs (which will flush and release locks on
            // output files).
            globalContext.Logger.AnalysisStopped(globalContext.RuntimeErrors);
            disposableLogger?.Dispose();

            if (Path.GetExtension(globalContext.EventsFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var dumpEventsCommand = new DumpEventsCommand();

                var options = new DumpEventsOptions()
                {
                    EventsFilePath = $"{Path.GetFileNameWithoutExtension(globalContext.EventsFilePath)}.etl",
                    CsvFilePath = globalContext.EventsFilePath,
                };

                Console.WriteLine(options);
                Console.WriteLine("Dumping session events to CSV (this could take a while)...");
                dumpEventsCommand.Run(options);
                Console.WriteLine($"Events written to: {globalContext.EventsFilePath}");
            }

            // Note that we don't clear the logger here. That is because the console
            // logger and any other custom loggers can still be useful for these operations.
            if ((globalContext.RuntimeErrors & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None)
            {
                ProcessBaseline(globalContext);
            }

            // Even if there are fatal errors, if the log file is generated, we can upload it with the ToolExecutionNotifications.
            PostLogFile(globalContext);

            globalContext.Logger = null;
            succeeded = (globalContext.RuntimeErrors & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None;

            return
                globalContext.RichReturnCode
                    ? (int)globalContext?.RuntimeErrors
                    : succeeded ? SUCCESS : FAILURE;
        }

        public virtual TContext InitializeGlobalContextFromOptions(TOptions options, ref TContext context)
        {
            context ??= new TContext();
            context.FileSystem ??= this.FileSystem ?? Sarif.FileSystem.Instance;

            context.Quiet = options.Quiet != null ? options.Quiet.Value : context.Quiet;

            // We initialize a temporary console logger that's used strictly to emit
            // diagnostics output while we load/initialize various configurations settings.
            IAnalysisLogger savedLogger = context.Logger;

            try
            {
                context.Logger = new ConsoleLogger(quietConsole: context.Quiet,
                                                   levels: BaseLogger.ErrorWarningNote,
                                                   kinds: BaseLogger.Fail,
                                                   toolName: Tool.Driver.Name);

                // Next, we initialize ourselves from disk-based configuration, 
                // if specified. This allows users to operate against configuration
                // XML but to override specific settings within it via options.
                context = InitializeConfiguration(options.ConfigurationFilePath, context);

                // Now that our context if fully initialized, we can create
                // the actual loggers used to complete analysis.
                context.Logger = savedLogger;
                context.ResultKinds = options.Kind != null ? options.ResultKinds : context.ResultKinds;
                context.FailureLevels = options.Level != null ? options.FailureLevels : context.FailureLevels;
                context.Logger ??= InitializeLogger(context);
            }
            finally
            {
                // Handle the remaining options.  This is in a finally block so even if the logger initialization throws
                // (possible), we still have a reasonably well-constructed globalContext for controlling any still-relevant
                // behaviors (ie: return code behavior.)  As a precaution, we handle trivial (clearly non-throwing)
                // initialization first.
                context.RichReturnCode = options.RichReturnCode ?? context.RichReturnCode;
                context.PostUri = options.PostUri ?? context.PostUri;
                context.AutomationId = options.AutomationId ?? context.AutomationId;
                context.Threads = options.Threads > 0 ? options.Threads : context.Threads;
                context.OutputFilePath = options.OutputFilePath ?? context.OutputFilePath;
                context.BaselineFilePath = options.BaselineFilePath ?? context.BaselineFilePath;
                context.Recurse = options.Recurse != null ? options.Recurse.Value : context.Recurse;
                context.GlobalFilePathDenyRegex = options.GlobalFilePathDenyRegex ?? context.GlobalFilePathDenyRegex;
                context.AutomationGuid = options.AutomationGuid != default ? options.AutomationGuid : context.AutomationGuid;
                context.OutputConfigurationFilePath = options.OutputConfigurationFilePath ?? context.OutputConfigurationFilePath;
                context.MaxFileSizeInKilobytes = options.MaxFileSizeInKilobytes != null ? options.MaxFileSizeInKilobytes.Value : context.MaxFileSizeInKilobytes;
                context.PluginFilePaths = options.PluginFilePaths?.Any() == true ? options.PluginFilePaths?.ToImmutableHashSet() : context.PluginFilePaths;
                context.TimeoutInMilliseconds = options.TimeoutInSeconds != null ? Math.Max(options.TimeoutInSeconds.Value * 1000, 0) : context.TimeoutInMilliseconds;
                context.InsertProperties = options.InsertProperties?.Any() == true ? InitializeStringSet(options.InsertProperties) : context.InsertProperties;
                context.TargetFileSpecifiers = options.TargetFileSpecifiers?.Any() == true ? InitializeStringSet(options.TargetFileSpecifiers) : context.TargetFileSpecifiers;
                context.InvocationPropertiesToLog = options.InvocationPropertiesToLog?.Any() == true ? InitializeStringSet(options.InvocationPropertiesToLog) : context.InvocationPropertiesToLog;
                context.Traces = options.Trace.Any() ? InitializeStringSet(options.Trace) : context.Traces;
                context.RuleKinds = options.RuleKindOption != null ? options.RuleKinds : context.RuleKinds;
            }

            // Less-obviously throw-safe. We don't do these in the finally block because we'd prefer not to mask an earlier Exception during logger initialization. 
            context.DataToInsert = options.DataToInsert?.Any() == true ? options.DataToInsert.ToFlags() : context.DataToInsert;
            context.DataToRemove = options.DataToRemove?.Any() == true ? options.DataToRemove.ToFlags() : context.DataToRemove;
            context.OutputFileOptions = options.OutputFileOptions?.Any() == true ? options.OutputFileOptions.ToFlags() : context.OutputFileOptions;
            context.EventsFilePath = Environment.GetEnvironmentVariable("SPMI_ETW") ?? options.EventsFilePath ?? context.EventsFilePath;

            if (context.TargetsProvider == null)
            {
                context.TargetsProvider =
                    OrderedFileSpecifier.Create(
                        context.TargetFileSpecifiers,
                        context.Recurse,
                        context.MaxFileSizeInKilobytes,
                        context.FileSystem);
            }

            return context;
        }

        public virtual TContext ValidateContext(TContext globalContext)
        {
            bool succeeded = true;

            bool force = globalContext.OutputFileOptions.HasFlag(FilePersistenceOptions.ForceOverwrite);
            succeeded &= ValidateFile(globalContext,
                                      globalContext.OutputFilePath,
                                      shouldExist: force ? (bool?)null : false);

            succeeded &= ValidateBaselineFile(globalContext);

            bool required = !string.IsNullOrEmpty(globalContext.BaselineFilePath);
            succeeded &= ValidateFile(globalContext,
                                      globalContext.BaselineFilePath,
                                      shouldExist: required ? true : (bool?)null);

            required = globalContext.PluginFilePaths?.Any() == true;
            succeeded &= ValidateFiles(globalContext,
                                       globalContext.PluginFilePaths,
                                       shouldExist: required ? true : (bool?)null);


            if (!string.IsNullOrEmpty(globalContext.PostUri))
            {
                try
                {
                    using HttpClientWrapper httpClient = GetHttpClientWrapper();
                    string separator = globalContext.PostUri.Contains("?") ? "&" : "?";
                    string uri = $"{globalContext.PostUri}{separator}healthcheck=true";

                    var content = new StringContent(string.Empty);
                    HttpResponseMessage httpResponseMessage = httpClient.PostAsync(uri, content).GetAwaiter().GetResult();

                    // For health check with query parameter, we expect a 202 (Accepted) response.
                    // We also maintain backwards compatibility with 422 (unprocessable payload) for servers
                    // that don't support the healthcheck parameter but will accept valid SARIF files.
                    if (httpResponseMessage.StatusCode != HttpStatusCode.Accepted &&
                        httpResponseMessage.StatusCode != (HttpStatusCode)422)
                    {
                        Errors.LogErrorPostingLogFile(globalContext, globalContext.PostUri);
                        globalContext.PostUri = null;
                        succeeded = false;
                    }
                }
                catch (Exception e)
                {
                    Errors.LogErrorPostingLogFile(globalContext, globalContext.PostUri);
                    globalContext.PostUri = null;
                    succeeded = false;
                    globalContext.RuntimeExceptions ??= new List<Exception>();
                    globalContext.RuntimeExceptions.Add(e);
                }
                finally
                {
                    // TBD add logging if POST URI is null.
                }
            }

            succeeded &= ValidateInvocationPropertiesToLog(globalContext);

            if (!succeeded)
            {
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }

            return globalContext;
        }

        private static ISet<string> InitializeStringSet(IEnumerable<string> strings)
        {
            return strings?.Any() == true ?
                   new StringSet(strings) :
                   new StringSet();
        }

        private void MultithreadedAnalyzeTargets(TContext globalContext,
                                                 IEnumerable<Skimmer<TContext>> skimmers,
                                                 ISet<string> disabledSkimmers)
        {
            globalContext.CancellationToken.ThrowIfCancellationRequested();
            var channelOptions = new BoundedChannelOptions(globalContext.ChannelSize)
            {
                SingleWriter = true,
                SingleReader = false,
            };
            readyToScanChannel = Channel.CreateBounded<uint>(channelOptions);

            channelOptions = new BoundedChannelOptions(globalContext.ChannelSize)
            {
                SingleWriter = false,
                SingleReader = true,
            };
            _resultsWritingChannel = Channel.CreateBounded<uint>(channelOptions);

            var sw = Stopwatch.StartNew();

            if (!globalContext.Quiet)
            {
                Console.WriteLine($"THREADS: {globalContext.Threads}");
            }

            // 1: First we initiate an asynchronous operation to locate disk files for
            // analysis, as specified in analysis configuration (file names, wildcards).
            Task<bool> enumerateTargets = Task.Run(() => EnumerateTargetsAsync(globalContext));

            // 2: A dedicated set of threads pull scan targets and analyze them.
            //    On completing a scan, the thread writes the index of the 
            //    scanned item to a channel that drives logging.
            var scanWorkers = new Task[globalContext.Threads];
            for (int i = 0; i < globalContext.Threads; i++)
            {
                scanWorkers[i] = Task.Run(() => ScanTargetsAsync(globalContext, skimmers, disabledSkimmers));
            }

            // 3: A single-threaded consumer watches for completed scans
            //    and logs results, if any. This operation is single-threaded
            //    to ensure determinism in log output. i.e., any scan of the
            //    same targets using the same production code should produce
            //    a log file that is byte-for-byte identical to previous log.
            var logResults = Task.Run(() => LogResultsAsync(globalContext));

            Task.WhenAll(scanWorkers)
                .ContinueWith(_ => _resultsWritingChannel.Writer.Complete())
                .Wait();

            enumerateTargets.Wait();
            logResults.Wait();

            if (_filesMatchingGlobalFileDenyRegex > 0)
            {
                string reason = "file path(s) matched the global file deny regex";
                Warnings.LogOneOrMoreFilesSkipped(globalContext, _filesMatchingGlobalFileDenyRegex, reason);
            }

            if (!globalContext.Quiet) { Console.WriteLine(); }

            string id;
            if (globalContext.Traces.Contains(nameof(DefaultTraces.PeakWorkingSet)))
            {
                using (var currentProcess = Process.GetCurrentProcess())
                {
                    id = $"TRC101.{nameof(DefaultTraces.PeakWorkingSet)}";
                    string memoryUsage = $"Peak working set: {currentProcess.PeakWorkingSet64 / 1024 / 1024}MB.";
                    LogTrace(globalContext, memoryUsage, id);
                }
            }

            if (globalContext.Traces.Contains(nameof(DefaultTraces.ScanTime)))
            {
                id = $"TRC101.{nameof(DefaultTraces.ScanTime)}";
                string timing = $"Done. {_fileContextsCount:n0} files scanned, elapsed time {sw.Elapsed}.";
                LogTrace(globalContext, timing, id);
            }
            else if (!globalContext.Quiet)
            {
                Console.WriteLine($"Done. {_fileContextsCount:n0} files scanned.");
            }
        }

        private async Task LogResultsAsync(TContext globalContext)
        {
            uint currentIndex = 0;

            ChannelReader<uint> reader = _resultsWritingChannel.Reader;

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out uint item))
                {
                    // This condition can occur if currentIndex moves
                    // ahead in array processing due to operations
                    // against it by other threads. For this case,
                    // since the relevant file has already been
                    // processed, we just ignore this notification.
                    if (currentIndex > item) { break; }

                    TContext context;
                    context = _fileContexts[currentIndex];

                    while (context?.AnalysisComplete == true)
                    {
                        lock (globalContext)
                        {
                            DriverEventSource.Log.LogResultsStart();
                            globalContext.CurrentTarget = context.CurrentTarget;
                            LogCachingLogger(globalContext, (CachingLogger)context.Logger, clone: false);
                            DriverEventSource.Log.LogResultsStop();

                            globalContext.RuntimeErrors |= context.RuntimeErrors;
                            if (context.RuntimeExceptions != null)
                            {
                                globalContext.RuntimeExceptions ??= new List<Exception>();
                                foreach (Exception exception in context.RuntimeExceptions)
                                {
                                    globalContext.RuntimeExceptions.Add(exception);
                                }
                            }

                            globalContext.CurrentTarget = null;
                        }

                        _fileContexts.TryRemove(currentIndex, out _);
                        _fileContexts.TryGetValue(++currentIndex, out context);
                    }
                }
            }
        }

        private static void LogCachingLogger(TContext globalContext, CachingLogger cachingLogger, bool clone = false)
        {
            // Today, the signal to generate hash data in log files is synonymous with a decision
            // to perform target file results-caching (where we only analyze a copy of a file, by
            // hash, a single time). We need to separate configuring this mechanism. Some scan 
            // scenarios, such as binary analysis + crawl of PDB, greatly benefit from this mechanism,
            // other scan scenarios, such as lightweight linting of large #'s of source files, 
            // experience significant memory pressure from it. Disabling caching altogether for now.
            // 
            // https://github.com/microsoft/sarif-sdk/issues/2620
            //
            // As a result of this gap, `clone` will always be false, because we have a per-file
            // (and not per-file-by-hash) logger instance.

            IDictionary<ReportingDescriptor, IList<Tuple<Result, int?>>> results = cachingLogger.Results;
            globalContext.CancellationToken.ThrowIfCancellationRequested();
            IEnumeratedArtifact artifact = globalContext.CurrentTarget;

            if (results?.Count > 0)
            {
                foreach (KeyValuePair<ReportingDescriptor, IList<Tuple<Result, int?>>> kv in results)
                {
                    globalContext.CancellationToken.ThrowIfCancellationRequested();
                    foreach (Tuple<Result, int?> tuple in kv.Value)
                    {
                        Result result = tuple.Item1;
                        Result currentResult = result;
                        if (clone)
                        {
                            Result clonedResult = result.DeepClone();

                            UpdateLocationsAndMessageWithCurrentUri(clonedResult.Locations, clonedResult.Message, artifact.Uri);

                            currentResult = clonedResult;
                        }
                        globalContext.Logger.FileRegionsCache = cachingLogger.FileRegionsCache;
                        globalContext.Logger.Log(kv.Key, currentResult, tuple.Item2);
                    }
                }
            }

            if (cachingLogger.ToolNotifications != null)
            {
                foreach (Tuple<Notification, ReportingDescriptor> tuple in cachingLogger.ToolNotifications)
                {
                    globalContext.Logger.LogToolNotification(tuple.Item1, tuple.Item2);
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
                        UpdateLocationsAndMessageWithCurrentUri(clonedNotification.Locations, notification.Message, artifact.Uri);

                        currentNotification = clonedNotification;
                    }

                    globalContext.Logger.LogConfigurationNotification(currentNotification);
                }
            }

            globalContext.Logger.TargetAnalyzed(globalContext);
        }

        private async Task<bool> EnumerateTargetsAsync(TContext globalContext)
        {
            try
            {
                this._fileContextsCount = 0;
                this._fileContexts = new ConcurrentDictionary<uint, TContext>();

                DriverEventSource.Log.EnumerateArtifactsStart();
                await EnumerateFilesFromArtifactsProvider(globalContext, recursionDepth: 0);
                DriverEventSource.Log.EnumerateArtifactsStop();
            }
            finally
            {
                readyToScanChannel.Writer.Complete();
            }

            if (_filesExceedingSizeLimitCount > 0)
            {
                Warnings.LogOneOrMoreFilesSkippedDueToExceedingSizeLimit(globalContext, _filesExceedingSizeLimitCount);
            }

            if (_fileContextsCount == 0)
            {
                Errors.LogNoValidAnalysisTargets(globalContext);

                ThrowExitApplicationException(ExitReason.NoValidAnalysisTargets);
            }

            return true;
        }

        private async Task<bool> EnumerateArtifact(IEnumeratedArtifact artifact, TContext globalContext, int recursionDepth = 0)
        {
            globalContext.CancellationToken.ThrowIfCancellationRequested();

            string filePath = artifact.Uri.GetFilePath();
            string suffix = artifact.Uri.IsAbsoluteUri ? artifact.Uri.Query : string.Empty;
            filePath = $"{filePath}{suffix}";

            if (globalContext.CompiledGlobalFileDenyRegex?.Match(filePath).Success == true)
            {
                _filesMatchingGlobalFileDenyRegex++;
                DriverEventSource.Log.ArtifactNotScanned(filePath, DriverEventNames.FilePathDenied, artifact.SizeInBytes.Value, globalContext.GlobalFilePathDenyRegex);

                string reason = $"its file path matched the global file deny regex: {globalContext.GlobalFilePathDenyRegex}";
                Notes.LogFileSkipped(globalContext, filePath, reason);
                return false;
            }

            if (artifact.SizeInBytes == 0)
            {
                DriverEventSource.Log.ArtifactNotScanned(filePath, DriverEventNames.EmptyFile, 00, data2: null);
                Notes.LogEmptyFileSkipped(globalContext, filePath);
                return true;
            }

            if (!IsTargetWithinFileSizeLimit(artifact.SizeInBytes.Value, globalContext.MaxFileSizeInKilobytes))
            {
                _filesExceedingSizeLimitCount++;
                DriverEventSource.Log.ArtifactNotScanned(filePath, DriverEventNames.FileExceedsSizeLimits, artifact.SizeInBytes.Value, $"{globalContext.MaxFileSizeInKilobytes}");
                Notes.LogFileExceedingSizeLimitSkipped(globalContext, filePath, artifact.SizeInBytes.Value / 1000);
                return false;
            }

            if (IsOpcArtifact(artifact, filePath, globalContext))
            {
                if (recursionDepth >= globalContext.MaxArchiveRecursionDepth)
                {
                    string reason = $"archive nesting exceeded maximum depth of {globalContext.MaxArchiveRecursionDepth}";
                    Notes.LogFileSkipped(globalContext, filePath, reason);
                    return false;
                }

                var context = new TContext();
                context.Policy = globalContext.Policy;
                context.Logger = globalContext.Logger;

                ZipArchive archive = null;

                try
                {
                    if (artifact.Bytes != null)
                    {
                        context.CurrentTarget = artifact;
                        archive = new ZipArchive(new MemoryStream(artifact.Bytes), ZipArchiveMode.Read, leaveOpen: false);
                    }
                    else
                    {
                        context.CurrentTarget = new EnumeratedArtifact(globalContext.FileSystem)
                        {
                            Uri = new Uri(filePath, UriKind.RelativeOrAbsolute)
                        };
                        archive = ZipFile.OpenRead(filePath);
                    }
                }
                catch (Exception ex)
                {
                    string message = "An exception was raised attempting to open a zip archive or Open Packaging Conventions (OPC) document.";
                    bool possiblyProtectedDocument = ex.Message == "End of Central Directory record could not be found.";
                    message = possiblyProtectedDocument
                        ? $"{message} This may indicate the the file has been marked as sensitive or otherwise protected."
                        : message;

                    lock (globalContext)
                    {
                        Errors.LogTargetParseError(context, region: null, message, ex);
                    }

                    globalContext.RuntimeErrors |= context.RuntimeErrors;
                    return false;
                }

                var artifactProvider = new MultithreadedZipArchiveArtifactProvider(context.CurrentTarget.Uri,
                                                                                   archive,
                                                                                   globalContext.FileSystem);
                context.TargetsProvider = artifactProvider;
                context.CurrentTarget = null;

                await EnumerateFilesFromArtifactsProvider(context, recursionDepth: recursionDepth + 1);
                return true;
            }

            TContext fileContext = CreateScanTargetContext(globalContext);

            fileContext.Logger = new CachingLogger(globalContext.FailureLevels,
                                                   globalContext.ResultKinds);

            Debug.Assert(fileContext.Logger != null);
            fileContext.CurrentTarget = artifact;
            fileContext.CancellationToken = globalContext.CancellationToken;

            lock (globalContext)
            {
                // We need to generate this event on the global logger, though as
                // a result this event means 'target enumerated for analysis'
                // rather than literally 'we are analyzing the target'.
                //
                // This call needs to be protected with a lock as the actual
                // logging occurs on a separated thread.
                globalContext.Logger.AnalyzingTarget(fileContext);
            }

            bool added = _fileContexts.TryAdd(_fileContextsCount, fileContext);
            Debug.Assert(added);

            if (_fileContextsCount == 0)
            {
                DriverEventSource.Log.FirstArtifactQueued(fileContext.CurrentTarget.Uri.GetFilePath());
            }

            await readyToScanChannel.Writer.WriteAsync(_fileContextsCount++);

            return true;
        }

        private static bool IsOpcArtifact(IEnumeratedArtifact artifact, string filePath, TContext globalContext)
        {
            // If the file extension is recognized as an OPC type, it qualifies.
            string extension = Path.GetExtension(filePath);
            if (!globalContext.OpcFileExtensions.Contains(extension))
            {
                return false;
            }

            // If we have bytes (e.g., stream-supplied ZIP), it qualifies.
            if (artifact.Bytes != null)
            {
                return true;
            }

            // Otherwise, it must be a URI-based artifact with no query string.
            return artifact.Uri.IsAbsoluteUri && string.IsNullOrEmpty(artifact.Uri.Query);
        }

        private async Task<bool> EnumerateFilesFromArtifactsProvider(TContext globalContext, int recursionDepth = 0)
        {
            foreach (IEnumeratedArtifact artifact in globalContext.TargetsProvider.Artifacts)
            {
                await EnumerateArtifact(artifact, globalContext, recursionDepth);
            }

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

        private async Task ScanTargetsAsync(TContext globalContext, IEnumerable<Skimmer<TContext>> skimmers, ISet<string> disabledSkimmers)
        {
            ChannelReader<uint> reader = readyToScanChannel.Reader;
            globalContext.CancellationToken.ThrowIfCancellationRequested();

            // Wait until there is work or the channel is closed.
            while (await reader.WaitToReadAsync())
            {
                // Loop while there is work to do.
                while (reader.TryRead(out uint item))
                {
                    TContext perFileContext = _fileContexts[item]; ;
                    perFileContext.CancellationToken.ThrowIfCancellationRequested();
                    string filePath = perFileContext.CurrentTarget.Uri.GetFilePath();

                    DriverEventSource.Log.ReadArtifactStart(filePath);
                    // Reading the length property faults in the file contents.
                    long sizeInBytes = perFileContext.CurrentTarget.SizeInBytes.Value;
                    DriverEventSource.Log.ReadArtifactStop(filePath, sizeInBytes);

                    DetermineApplicabilityAndAnalyze(perFileContext, skimmers, disabledSkimmers);
                    globalContext.RuntimeErrors |= perFileContext.RuntimeErrors;
                    if (perFileContext != null) { perFileContext.AnalysisComplete = true; }
                    await _resultsWritingChannel.Writer.WriteAsync(item);
                }
            }
        }

        protected virtual void ValidateOptions(TOptions options, TContext context)
        {
            bool succeeded = true;

            succeeded &= ValidateFile(context, options.OutputFilePath, shouldExist: null);
            succeeded &= ValidateFile(context, options.ConfigurationFilePath, shouldExist: true);
            succeeded &= ValidateFile(context, options.BaselineFilePath, shouldExist: true);
            succeeded &= ValidateFiles(context, options.PluginFilePaths, shouldExist: true);
            succeeded &= ValidateInvocationPropertiesToLog(context);
            succeeded &= options.ValidateOutputOptions(context);
            succeeded &= context.MaxFileSizeInKilobytes >= 0;

            succeeded &= ValidateOutputFileCanBeCreated(context);

            if (!succeeded)
            {
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }
        }

        internal AggregatingLogger InitializeLogger(TContext globalContext)
        {
            var logger = new AggregatingLogger();

            if (!(globalContext.Quiet == true))
            {
                _consoleLogger =
                    new ConsoleLogger(quietConsole: false,
                                      Tool.Driver.Name,
                                      globalContext.FailureLevels,
                                      globalContext.ResultKinds)
                    {
                        CaptureOutput = _captureConsoleOutput
                    };

                logger.Loggers.Add(_consoleLogger);
            }

            return logger;
        }

        protected virtual TContext CreateScanTargetContext(TContext globalContext)
        {
            var context = new TContext
            {
                Logger = globalContext.Logger,
                RuntimeErrors = globalContext.RuntimeErrors,
                FileSystem = globalContext.FileSystem ?? this.FileSystem,
                Policy = globalContext.Policy ?? new PropertiesDictionary(),
            };

            return context;
        }

        /// <summary>
        /// Calculate the file to load the configuration from.
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Configuration file path, or null if the built in configuration should be used.</returns>
        internal string GetConfigurationFileName(string configurationFilePath, IFileSystem fileSystem)
        {
            // If the user passes our default file name, return null, indicating that no
            // configuration should be loaded. As a result, we will pick up all runtime defaults.
            if (configurationFilePath == DefaultPolicyName)
            {
                return null;
            }

            if (string.IsNullOrEmpty(configurationFilePath))
            {
                // If a configuration file is not explicitly specified but we see that 
                // a default configuration file has been placed in the current working
                // directory, we will load it. TBD: ensure this location is alongside
                // the client executable?
                return fileSystem.FileExists(this.DefaultConfigurationPath)
                    ? this.DefaultConfigurationPath
                    : null;
            }

            if (!File.Exists(configurationFilePath))
            {
                string fileName = Path.GetFileNameWithoutExtension(configurationFilePath);
                string spamDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                fileName = Path.Combine(spamDirectory, $"{fileName}.xml");

                if (fileSystem.FileExists(fileName))
                {
                    return fileName;
                }
            }

            return configurationFilePath;
        }

        protected virtual TContext InitializeConfiguration(string configurationFileName, TContext context)
        {
            context.Policy ??= new PropertiesDictionary();

            configurationFileName = GetConfigurationFileName(configurationFileName, context.FileSystem);
            context.ConfigurationFilePath = configurationFileName;

            if (!ValidateFile(context,
                              configurationFileName,
                              shouldExist: !string.IsNullOrEmpty(configurationFileName) ? true : (bool?)null))
            {
                // TBD exit reason update to invalid config?
                ThrowExitApplicationException(ExitReason.InvalidCommandLineOption);
            }

            if (string.IsNullOrEmpty(configurationFileName)) { return context; }

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
            return context;
        }

        public virtual void InitializeOutputs(TContext globalContext)
        {
            string filePath = globalContext.OutputFilePath;

            if (!string.IsNullOrEmpty(filePath))
            {
                var aggregatingLogger = globalContext.Logger as AggregatingLogger;
                if (aggregatingLogger == null)
                {
                    aggregatingLogger = new AggregatingLogger();
                    aggregatingLogger.Loggers.Add(globalContext.Logger);
                    globalContext.Logger = aggregatingLogger;
                }

                if (globalContext.Traces.Contains(nameof(DefaultTraces.ResultsSummary)))
                {
                    aggregatingLogger.Loggers.Add(new ResultsSummaryLogger());
                }

                InvokeCatchingRelevantIOExceptions
                (
                    () =>
                    {
                        FilePersistenceOptions logFilePersistenceOptions = globalContext.OutputFileOptions;

                        OptionallyEmittedData dataToInsert = globalContext.DataToInsert;
                        OptionallyEmittedData dataToRemove = globalContext.DataToRemove;

                        SarifLogger sarifLogger;

                        var run = new Run()
                        {
                            AutomationDetails = new RunAutomationDetails
                            {
                                Id = globalContext.AutomationId,
                                Guid = globalContext.AutomationGuid
                            },
                            VersionControlProvenance = globalContext.VersionControlProvenance,
                            Tool = Tool,
                        };

                        var fileRegionsCache = new FileRegionsCache(fileSystem: globalContext.FileSystem);
                        sarifLogger = new SarifLogger(globalContext.OutputFilePath,
                                                      logFilePersistenceOptions,
                                                      dataToInsert,
                                                      dataToRemove,
                                                      run,
                                                      analysisTargets: null,
                                                      fileRegionsCache: fileRegionsCache,
                                                      invocationTokensToRedact: GenerateSensitiveTokensList(),
                                                      invocationPropertiesToLog: globalContext.InvocationPropertiesToLog,
                                                      levels: globalContext.FailureLevels,
                                                      kinds: globalContext.ResultKinds,
                                                      insertProperties: globalContext.InsertProperties);

                        aggregatingLogger.Loggers.Add(sarifLogger);
                    },
                    (ex) =>
                    {
                        Errors.LogExceptionCreatingOutputFile(globalContext, filePath, ex);
                        ThrowExitApplicationException(ExitReason.ExceptionCreatingLogFile, ex);
                    }
                );
            }

            globalContext.Logger ??= new ConsoleLogger(quietConsole: false, "SPMI");
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

        protected virtual ISet<Skimmer<TContext>> CreateSkimmers(TContext context)
        {
            IEnumerable<Skimmer<TContext>> skimmers;
            var result = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            try
            {
                skimmers = CompositionUtilities.GetExports<Skimmer<TContext>>(RetrievePluginAssemblies(DefaultPluginAssemblies, context.PluginFilePaths));

                SupportedPlatform currentOS = GetCurrentRunningOS();
                foreach (Skimmer<TContext> skimmer in skimmers)
                {
                    if (!context.RuleKinds.Any(r => skimmer.RuleKinds.Contains(r)))
                    {
                        continue;
                    }
                    else if (skimmer.SupportedPlatforms.HasFlag(currentOS))
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
                ThrowExitApplicationException(ExitReason.UnhandledExceptionInstantiatingSkimmers, ex);
            }

            if (result.Count == 0)
            {
                Errors.LogNoRulesLoaded(context);
                ThrowExitApplicationException(ExitReason.NoRulesLoaded);
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

        protected virtual void AnalyzeTargets(TContext context,
                                              IEnumerable<Skimmer<TContext>> skimmers)
        {
            if (skimmers == null)
            {
                Errors.LogNoRulesLoaded(context);
                ThrowExitApplicationException(ExitReason.NoRulesLoaded);
            }

            ISet<string> disabledSkimmers = BuildDisabledSkimmersSet(context, skimmers);

            if (disabledSkimmers.Count == skimmers.Count())
            {
                Errors.LogAllRulesExplicitlyDisabled(context);
                ThrowExitApplicationException(ExitReason.NoRulesLoaded);
            }

            this.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            MultithreadedAnalyzeTargets(context, skimmers, disabledSkimmers);
        }

        public static ISet<string> BuildDisabledSkimmersSet(TContext context, IEnumerable<Skimmer<TContext>> skimmers)
        {
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

            return disabledSkimmers;
        }

        protected virtual TContext DetermineApplicabilityAndAnalyze(TContext context,
                                                                    IEnumerable<Skimmer<TContext>> skimmers,
                                                                    ISet<string> disabledSkimmers)
        {
            string filePath = context.CurrentTarget.Uri.GetFilePath();

            if (context.RuntimeExceptions != null)
            {
                Debug.Assert(context.RuntimeExceptions.Count == 1);
                Errors.LogExceptionLoadingTarget(context, context.RuntimeExceptions[0]);
                return context;
            }
            else if (!context.IsValidAnalysisTarget)
            {
                Warnings.LogExceptionInvalidTarget(context);
                return context;
            }

            var logger = (CachingLogger)context.Logger;
            logger.AnalyzingTarget(context);

            if (logger.CacheFinalized)
            {
                context.Logger = logger;
                logger.TargetAnalyzed(context);
                return context;
            }

            context.CancellationToken.ThrowIfCancellationRequested();
            IEnumerable<Skimmer<TContext>> applicableSkimmers = DetermineApplicabilityForTarget(context, skimmers, disabledSkimmers);

            AnalyzeTarget(context, applicableSkimmers, disabledSkimmers);

            logger.TargetAnalyzed(context);
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
            string filePath = context.CurrentTarget.Uri.GetFilePath();
            long sizeInBytes = context.CurrentTarget.SizeInBytes.Value;

            DriverEventSource.Log.ScanArtifactStart(filePath, sizeInBytes);
            AnalyzeTargetHelper(context, skimmers, disabledSkimmers);
            DriverEventSource.Log.ScanArtifactStop(filePath, sizeInBytes);
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

                    DriverEventSource.Log.RuleStart(context.CurrentTarget.Uri.GetFilePath(), skimmer.Id, skimmer.Name);
                    skimmer.Analyze(context);
                    DriverEventSource.Log.RuleStop(context.CurrentTarget.Uri.GetFilePath(), skimmer.Id, skimmer.Name);

                    Uri uri = context.CurrentTarget.Uri;

                    if (stopwatch != null)
                    {
                        string file = uri.GetFilePath();
                        string directory = Path.GetDirectoryName(file);
                        file = Path.GetFileName(file);

                        string id = $"TRC101.{nameof(DefaultTraces.RuleScanTime)}";
                        string timing = $"'{file}' : elapsed {stopwatch.Elapsed} : '{skimmer.Name}' : at '{directory}'";
                        LogTrace(context, timing, id, context.Rule);
                    }
                }
                catch (Exception ex)
                {
                    Errors.LogUnhandledRuleExceptionAnalyzingTarget(disabledSkimmers, context, ex);

                    context.RuntimeExceptions ??= new List<Exception>();
                    context.RuntimeExceptions.Add(ex);
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

                string reasonForNotAnalyzing;
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

        protected void ThrowExitApplicationException(ExitReason exitReason, Exception innerException = null)
        {
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
                        ThrowExitApplicationException(ExitReason.IncompatibleRulesDetected);
                    }
                }
            }
        }

        protected virtual ISet<Skimmer<TContext>> InitializeSkimmers(ISet<Skimmer<TContext>> skimmers, TContext globalContext)
        {
            var disabledSkimmers = new SortedSet<Skimmer<TContext>>(SkimmerIdComparer<TContext>.Instance);

            // ONE-TIME initialization of skimmers. Do not call
            // Initialize more than once per skimmer instantiation
            foreach (Skimmer<TContext> skimmer in skimmers)
            {
                try
                {
                    globalContext.Rule = skimmer;
                    skimmer.Initialize(globalContext);
                }
                catch (Exception ex)
                {
                    globalContext.RuntimeErrors |= RuntimeConditions.ExceptionInSkimmerInitialize;
                    Errors.LogUnhandledExceptionInitializingRule(globalContext, ex);
                    disabledSkimmers.Add(skimmer);
                }
            }

            foreach (Skimmer<TContext> disabledSkimmer in disabledSkimmers)
            {
                skimmers.Remove(disabledSkimmer);
            }

            return skimmers;
        }

        protected static void LogTrace(
            TContext globalContext,
            string message,
            string id = null,
            ReportingDescriptor associatedRule = null,
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

            if (!globalContext.FailureLevels.Contains(level))
            {
                // If our analysis run isn't configured to show the current failure level
                // of this notification, we still write it out to the console, as it is
                // a trace message that's explicitly enabled on the command-line.
                TextWriter writer = level == FailureLevel.Error ? Console.Error : Console.Out;
                writer.WriteLine(message);
                return;
            }

            globalContext.Logger.LogToolNotification(new Notification
            {
                Level = level,
                Descriptor = new ReportingDescriptorReference
                {
                    Id = id
                },
                AssociatedRule = new ReportingDescriptorReference { Id = associatedRule?.Id },
                Message = new Message { Text = message },
                Exception = exceptionData,
                TimeUtc = DateTime.UtcNow
            },
                associatedRule);
        }

        internal static bool IsTargetWithinFileSizeLimit(long size, long maxFileSizeInKB)
        {
            if (size == 0) { return false; };
            size = Math.Min(long.MaxValue - 1023, size);
            long fileSizeInKb = (size + 1023) / 1024;
            return fileSizeInKb <= maxFileSizeInKB;
        }
    }
}
