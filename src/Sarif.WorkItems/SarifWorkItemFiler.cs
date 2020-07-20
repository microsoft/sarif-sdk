// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.WorkItems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WorkItems;
using Microsoft.WorkItems.Logging;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFiler : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SarifWorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="filingUri">
        /// The uri to the remote filing host.
        /// </param>
        /// <param name="filingContext">
        /// A starting context object that configures the work item filing operation. In the
        /// current implementation, this context is copied for each SARIF file (if any) split
        /// from the input log and then further elaborated upon.
        /// </param>
        public SarifWorkItemFiler(Uri filingUri = null, SarifWorkItemContext filingContext = null)
        {
            this.FilingContext = filingContext ?? new SarifWorkItemContext { HostUri = filingUri };
            filingUri = filingUri ?? this.FilingContext.HostUri;

            if (filingUri == null) { throw new ArgumentNullException(nameof(filingUri)); };

            if (filingUri != this.FilingContext.HostUri)
            {
                // Inconsistent URIs were provided in 'filingContext' and 'filingUri'; arguments.
                throw new InvalidOperationException(WorkItemsResources.InconsistentHostUrisProvided);
            }

            this.FilingClient = FilingClientFactory.Create(this.FilingContext.HostUri);

            this.Logger = ServiceProviderFactory.ServiceProvider.GetService<ILogger>();
            Assembly.GetExecutingAssembly().LogIdentity();

            this.FiledWorkItems = new List<WorkItemModel>();
        }

        public FilingClient FilingClient { get; set; }

        public SarifWorkItemContext FilingContext { get; }

        public List<WorkItemModel> FiledWorkItems { get; private set; }

        public FilingResult FilingResult { get; private set; }

        internal ILogger Logger { get; }

        public virtual SarifLog FileWorkItems(Uri sarifLogFileLocation)
        {
            sarifLogFileLocation = sarifLogFileLocation ?? throw new ArgumentNullException(nameof(sarifLogFileLocation));

            if (sarifLogFileLocation.IsAbsoluteUri && sarifLogFileLocation.Scheme == UriUtilities.FileScheme)
            {
                if (sarifLogFileLocation.IsAbsoluteUri && sarifLogFileLocation.Scheme == UriUtilities.FileScheme.WithColon())
                {
                    using (var stream = new FileStream(sarifLogFileLocation.LocalPath, FileMode.Open, FileAccess.Read))
                    using (var reader = new StreamReader(stream))
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        var serializer = new JsonSerializer();
                        SarifLog sarifLog = serializer.Deserialize<SarifLog>(jsonReader);
                        return FileWorkItems(sarifLog);
                    }
                }
            }
            throw new ArgumentException($"Specified URI was not an absolute file URI: {sarifLogFileLocation}");
        }

        public virtual SarifLog FileWorkItems(string sarifLogFileContents)
        {
            SarifLog sarifLog = ConvertToSarifLog(sarifLogFileContents);
            return FileWorkItems(sarifLog);
        }

        public virtual SarifLog FileWorkItems(SarifLog sarifLog)
        {
            using (Logger.BeginScopeContext(nameof(FileWorkItems)))
            {

                sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));

                IReadOnlyList<SarifLog> logsToProcess = SplitLogFile(sarifLog);

                int logsToProcessCount = logsToProcess.Count;

#if DEBUG
                if (!int.TryParse(Environment.GetEnvironmentVariable("SARIFTEST_FILINGLIMIT"), out logsToProcessCount))
                {
                    logsToProcessCount = logsToProcess.Count;
                }
#endif

                for (int splitFileIndex = 0; splitFileIndex < logsToProcessCount; splitFileIndex++)
                {
                    SarifLog splitLog = logsToProcess[splitFileIndex];
                    SarifWorkItemModel sarifWorkItemModel = FileWorkItemInternal(splitLog, this.FilingContext, this.FilingClient);

                    // IMPORTANT: as we update our partitioned logs, we are actually modifying the input log file 
                    // as well. That's because our partitioning is configured to reuse references to existing
                    // run and result objects, even though they are partitioned into a separate log file. 
                    // This approach also us to update the original log file with the filed work item details
                    // without requiring us to build a map of results between the original log and its
                    // partioned log files.
                    //
                    if (sarifWorkItemModel != null)
                    {
                        UpdateLogWithWorkItemDetails(splitLog, sarifWorkItemModel.HtmlUri, sarifWorkItemModel.Uri);
                    }
                }

                return sarifLog;
            }
        }

        public static SarifLog ConvertToSarifLog(string sarifLogFileContents)
        {
            sarifLogFileContents = sarifLogFileContents ?? throw new ArgumentNullException(nameof(sarifLogFileContents));

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLogFileContents);

            return sarifLog;
        }

        public virtual IReadOnlyList<SarifLog> SplitLogFile(SarifLog sarifLog)
        {
            IList<SarifLog> logsToProcess;

            using (Logger.BeginScopeContext(nameof(SplitLogFile)))
            {
                sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));
                sarifLog.SetProperty("guid", Guid.NewGuid());

                this.FilingResult = FilingResult.None;
                this.FiledWorkItems = new List<WorkItemModel>();

                sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));

                Logger.LogInformation("Connecting to filing client: {accountOrOrganization}", this.FilingClient.AccountOrOrganization);
                this.FilingClient.Connect(this.FilingContext.PersonalAccessToken).Wait();

                OptionallyEmittedData optionallyEmittedData = this.FilingContext.DataToRemove;
                if (optionallyEmittedData != OptionallyEmittedData.None)
                {
                    Logger.LogDebug("Removing optional data.");
                    var dataRemovingVisitor = new RemoveOptionalDataVisitor(optionallyEmittedData);
                    dataRemovingVisitor.Visit(sarifLog);
                }

                optionallyEmittedData = this.FilingContext.DataToInsert;
                if (optionallyEmittedData != OptionallyEmittedData.None)
                {
                    Logger.LogDebug("Inserting optional data.");
                    var dataInsertingVisitor = new InsertOptionalDataVisitor(optionallyEmittedData);
                    dataInsertingVisitor.Visit(sarifLog);
                }

                using (Logger.BeginScopeContext("Splitting visitor"))
                {
                    SplittingStrategy splittingStrategy = this.FilingContext.SplittingStrategy;

                    Logger.LogInformation($"Splitting strategy - {splittingStrategy}");

                    if (splittingStrategy == SplittingStrategy.None)
                    {
                        return new[] { sarifLog };
                    }

                    PartitionFunction<string> partitionFunction = null;

                    Stopwatch splittingStopwatch = Stopwatch.StartNew();

                    switch (splittingStrategy)
                    {
                        case SplittingStrategy.PerRun:
                        {
                            partitionFunction = (result) => result.ShouldBeFiled() ? "Include" : null;
                            break;
                        }
                        case SplittingStrategy.PerResult:
                        {
                            partitionFunction = (result) => result.ShouldBeFiled() ? Guid.NewGuid().ToString() : null;
                            break;
                        }
                        case SplittingStrategy.PerRunPerOrgPerEntityTypePerPartialFingerprint:
                        {
                            partitionFunction = (result) => result.ShouldBeFiled() ? result.GetFingerprintSplittingStrategyId() : null;
                            break;
                        }
                        case SplittingStrategy.PerRunPerOrgPerEntityTypePerRepositoryPerPartialFingerprint:
                        {
                            partitionFunction = (result) => result.ShouldBeFiled() ? result.GetPerRepositoryFingerprintSplittingStrategyId() : null;
                            break;
                        }
                        default:
                        {
                            throw new ArgumentOutOfRangeException($"SplittingStrategy: {splittingStrategy}");
                        }
                    }

                    Logger.LogDebug("Begin splitting logs");
                    var partitioningVisitor = new PartitioningVisitor<string>(partitionFunction, deepClone: false);
                    partitioningVisitor.VisitSarifLog(sarifLog);

                    Logger.LogDebug("Begin retrieving split logs");
                    logsToProcess = new List<SarifLog>(partitioningVisitor.GetPartitionLogs().Values);

                    Logger.LogDebug("End retrieving split logs");

                    var logsToProcessMetrics = new Dictionary<string, object>
                    {
                        { "splittingStrategy", splittingStrategy },
                        { "logsToProcessCount", logsToProcess.Count },
                        { "splittingDurationInMilliseconds", splittingStopwatch.ElapsedMilliseconds },
                    };

                    this.Logger.LogMetrics(EventIds.LogsToProcessMetrics, logsToProcessMetrics);
                    splittingStopwatch.Stop();
                }
            }

            if (logsToProcess != null && !this.FilingContext.ShouldFileUnchanged)
            {
                // Remove any logs that do not contain at least one result with a New or None baselinestate.
                logsToProcess = logsToProcess.Where(log => log?.Runs?.Any(run => run.Results?.Any(result => result.BaselineState == BaselineState.New || result.BaselineState == BaselineState.None) == true) == true).ToList();
            }

            return logsToProcess.ToArray();
        }

        internal const string PROGRAMMABLE_URIS_PROPERTY_NAME = "programmableWorkItemUris";

        public SarifWorkItemModel FileWorkItemInternal(SarifLog sarifLog, SarifWorkItemContext filingContext, FilingClient filingClient)
        {
            using (Logger.BeginScopeContext(nameof(FileWorkItemInternal)))
            {
                string logGuid = sarifLog.GetProperty<Guid>("guid").ToString();

                // The helper below will initialize the sarif work item model with a copy
                // of the root pipeline filing context. This context will then be initialized
                // based on the current sarif log file that we're processing.
                // First intializes the contexts provider to the value in the current filing client.
                filingContext.CurrentProvider = filingClient.Provider;
                var sarifWorkItemModel = new SarifWorkItemModel(sarifLog, filingContext);

                try
                {
                    // Populate the work item with the target organization/repository information.
                    // In ADO, certain fields (such as the area path) will defaut to the 
                    // project name and so this information is used in at least that context.
                    sarifWorkItemModel.OwnerOrAccount = filingClient.AccountOrOrganization;
                    sarifWorkItemModel.RepositoryOrProject = filingClient.ProjectOrRepository;

                    if (filingContext.SyncWorkItemMetadata)
                    {
                        Task<WorkItemModel> getMetadataTask = filingClient.GetWorkItemMetadata(sarifWorkItemModel);
                        getMetadataTask.Wait();
                        sarifWorkItemModel = (SarifWorkItemModel)getMetadataTask.Result;
                    }

                    using (Logger.BeginScopeContext("RunTransformers"))
                    {
                        foreach (SarifWorkItemModelTransformer transformer in sarifWorkItemModel.Context.Transformers)
                        {
                            SarifWorkItemModel updatedSarifWorkItemModel = transformer.Transform(sarifWorkItemModel);

                            // If a transformer has set the model to null, that indicates 
                            // it should be pulled from the work flow (i.e., not filed).
                            if (updatedSarifWorkItemModel == null)
                            {
                                Dictionary<string, object> customDimentions = new Dictionary<string, object>();
                                customDimentions.Add("TransformerType", transformer.GetType().FullName);
                                LogMetricsForProcessedModel(sarifLog, sarifWorkItemModel, FilingResult.Canceled, customDimentions);
                                return null;
                            }

                            sarifWorkItemModel = updatedSarifWorkItemModel;
                        }
                    }

                    Task<IEnumerable<WorkItemModel>> task = filingClient.FileWorkItems(new[] { sarifWorkItemModel });
                    task.Wait();
                    this.FiledWorkItems.AddRange(task.Result);

                    LogMetricsForProcessedModel(sarifLog, sarifWorkItemModel, FilingResult.Succeeded);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "An exception was raised filing log '{logGuid}'.", logGuid);

                    Dictionary<string, object> customDimentions = new Dictionary<string, object>();
                    customDimentions.Add("ExceptionType", ex.GetType().FullName);
                    customDimentions.Add("ExceptionMessage", ex.Message);
                    customDimentions.Add("ExceptionStackTrace", ex.ToString());
                    LogMetricsForProcessedModel(sarifLog, sarifWorkItemModel, FilingResult.ExceptionRaised, customDimentions);
                }

                return sarifWorkItemModel;
            }
        }

        public static void UpdateLogWithWorkItemDetails(SarifLog sarifLog, Uri htmlUri, Uri uri)
        {
            foreach (Run run in sarifLog.Runs)
            {
                if (run.Results == null) { continue; }

                foreach (Result result in run.Results)
                {
                    result.WorkItemUris ??= new List<Uri>();
                    result.WorkItemUris.Add(htmlUri);

                    result.TryGetProperty(PROGRAMMABLE_URIS_PROPERTY_NAME, out List<Uri> programmableUris);

                    programmableUris ??= new List<Uri>();
                    programmableUris.Add(uri);

                    result.SetProperty(PROGRAMMABLE_URIS_PROPERTY_NAME, programmableUris);
                }
            }
        }

        private void LogMetricsForProcessedModel(SarifLog sarifLog, SarifWorkItemModel sarifWorkItemModel, FilingResult filingResult, Dictionary<string, object> additionalCustomDimensions = null)
        {
            additionalCustomDimensions ??= new Dictionary<string, object>();

            this.FilingResult = filingResult;

            string logGuid = sarifLog.GetProperty<Guid>("guid").ToString();
            string tags = string.Join(",", sarifWorkItemModel.LabelsOrTags);
            string uris = sarifWorkItemModel.LocationUris?.Count > 0
                ? string.Join(",", sarifWorkItemModel.LocationUris)
                : "";

            var workItemMetrics = new Dictionary<string, object>
                {
                    { "LogGuid", logGuid },
                    { "WorkItemModelGuid", sarifWorkItemModel.Guid },
                    { nameof(sarifWorkItemModel.Area), sarifWorkItemModel.Area },
                    { nameof(sarifWorkItemModel.BodyOrDescription), sarifWorkItemModel.BodyOrDescription },
                    { "FilingResult", filingResult.ToString() },
                    { nameof(sarifWorkItemModel.CommentOrDiscussion), sarifWorkItemModel.CommentOrDiscussion },
                    { nameof(sarifWorkItemModel.HtmlUri), sarifWorkItemModel.HtmlUri },
                    { nameof(sarifWorkItemModel.Iteration), sarifWorkItemModel.Iteration },
                    { "LabelsOrTags", tags },
                    { "LocationUri", uris },
                    { nameof(sarifWorkItemModel.Milestone), sarifWorkItemModel.Milestone },
                    { nameof(sarifWorkItemModel.OwnerOrAccount), sarifWorkItemModel.OwnerOrAccount },
                    { nameof(sarifWorkItemModel.RepositoryOrProject), sarifWorkItemModel.RepositoryOrProject },
                    { nameof(sarifWorkItemModel.Title), sarifWorkItemModel.Title },
                    { nameof(sarifWorkItemModel.Uri), sarifWorkItemModel.Uri },
                };

            foreach (string key in additionalCustomDimensions.Keys)
            {
                workItemMetrics.Add(key, additionalCustomDimensions[key]);
            }

            EventId coreEventId;
            switch (filingResult)
            {
                case FilingResult.Canceled:
                    coreEventId = EventIds.WorkItemCanceledCoreMetrics;
                    break;
                case FilingResult.ExceptionRaised:
                    coreEventId = EventIds.WorkItemExceptionCoreMetrics;
                    break;
                default:
                    coreEventId = EventIds.WorkItemFiledCoreMetrics;
                    break;
            }

            this.Logger.LogMetrics(coreEventId, workItemMetrics);

            Dictionary<string, RuleMetrics> ruleIdToMetricsMap = CreateRuleMetricsMap(sarifLog);

            foreach (string tool in ruleIdToMetricsMap.Keys)
            {
                RuleMetrics ruleMetrics = ruleIdToMetricsMap[tool];

                var workItemDetailMetrics = new Dictionary<string, object>
                {
                    { "LogGuid", logGuid },
                    { "WorkItemModelGuid", sarifWorkItemModel.Guid },
                    { nameof(ruleMetrics.Tool), ruleMetrics.Tool },
                    { nameof(ruleMetrics.RuleId), ruleMetrics.RuleId },
                    { nameof(ruleMetrics.ErrorCount), ruleMetrics.ErrorCount },
                    { nameof(ruleMetrics.WarningCount), ruleMetrics.WarningCount },
                    { nameof(ruleMetrics.NoteCount), ruleMetrics.NoteCount },
                    { nameof(ruleMetrics.OpenCount), ruleMetrics.OpenCount },
                    { nameof(ruleMetrics.ReviewCount), ruleMetrics.ReviewCount },
                    { nameof(ruleMetrics.SuppressedByTransformerCount), ruleMetrics.SuppressedByTransformerCount }
                };

                this.Logger.LogMetrics(EventIds.WorkItemFiledDetailMetrics, workItemDetailMetrics);
            }
        }

        private Dictionary<string, RuleMetrics> CreateRuleMetricsMap(SarifLog sarifLog)
        {
            var ruleIdToRuleMetricsMap = new Dictionary<string, RuleMetrics>();

            foreach (Run run in sarifLog.Runs)
            {
                if (run.Results == null) { continue; }

                string toolName = run.Tool.Driver.Name;

                foreach (Result result in run.Results)
                {
                    // In our current design, input log files contain only candidate results
                    // to file. A transformer, however, might suppress a result as part of its
                    // operation.
                    Debug.Assert(result.ShouldBeFiled() || result.Suppressions?.Count > 0);

                    string ruleId = result.GetRule(run).Id;
                    string key = toolName + ruleId;

                    if (!ruleIdToRuleMetricsMap.TryGetValue(key, out RuleMetrics ruleMetrics))
                    {
                        ruleIdToRuleMetricsMap[key] = ruleMetrics =
                            new RuleMetrics
                            {
                                Tool = toolName,
                                RuleId = ruleId
                            };
                    }

                    if (result.Kind == ResultKind.Open)
                    {
                        ruleMetrics.OpenCount++;
                        continue;
                    }

                    if (result.Kind == ResultKind.Review)
                    {
                        ruleMetrics.ReviewCount++;
                        continue;
                    }

                    if (result.Suppressions?.Count > 0)
                    {
                        ruleMetrics.SuppressedByTransformerCount++;
                        continue;
                    }

                    switch (result.Level)
                    {
                        case FailureLevel.Error:
                        {
                            ruleMetrics.ErrorCount++;
                            break;
                        }
                        case FailureLevel.Warning:
                        {
                            ruleMetrics.WarningCount++;
                            break;
                        }
                        case FailureLevel.Note:
                        {
                            ruleMetrics.NoteCount++;
                            break;
                        }
                        case FailureLevel.None:
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            return ruleIdToRuleMetricsMap;
        }

        private class RuleMetrics
        {
            public string Tool;
            public string RuleId;
            public int ErrorCount;
            public int WarningCount;
            public int NoteCount;
            public int OpenCount;
            public int ReviewCount;
            public int SuppressedByTransformerCount;
        }

        public void Dispose()
        {
            this.FilingClient?.Dispose();
            this.FilingClient = null;

            ITelemetryChannel channel = ServiceProviderFactory.ServiceProvider.GetService<ITelemetryChannel>();
            channel?.Flush();
            channel?.Dispose();
        }
    }
}