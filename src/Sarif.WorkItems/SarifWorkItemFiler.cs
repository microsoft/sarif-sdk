﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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

            this.Logger = ServiceProviderFactory.ServiceProvider.GetService<ILogger<FilingClient>>();
        }

        public FilingClient FilingClient { get; set; }

        public SarifWorkItemContext FilingContext { get; }

        public List<WorkItemModel> FiledWorkItems { get; private set; }

        public bool FilingSucceeded { get; private set; }

        internal ILogger Logger { get; }

        public virtual SarifLog FileWorkItems(Uri sarifLogFileLocation)
        {
            sarifLogFileLocation = sarifLogFileLocation ?? throw new ArgumentNullException(nameof(sarifLogFileLocation));

            if (sarifLogFileLocation.IsAbsoluteUri && sarifLogFileLocation.Scheme == "file")
            {
                if (sarifLogFileLocation.IsAbsoluteUri && sarifLogFileLocation.Scheme == "file:")
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
            sarifLogFileContents = sarifLogFileContents ?? throw new ArgumentNullException(nameof(sarifLogFileContents));

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLogFileContents);

            return FileWorkItems(sarifLog);
        }

        public virtual SarifLog FileWorkItems(SarifLog sarifLog)
        {
            using (Logger.BeginScope(nameof(FileWorkItems)))
            {
                this.FilingSucceeded = false;
                this.FiledWorkItems = new List<WorkItemModel>();

                sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));

                Logger.LogInformation("Connecting to filing client: {accountOrOrganization}", this.FilingClient.AccountOrOrganization);
                this.FilingClient.Connect(this.FilingContext.PersonalAccessToken).Wait();

                OptionallyEmittedData optionallyEmittedData = this.FilingContext.DataToRemove;
                if (optionallyEmittedData != OptionallyEmittedData.None)
                {
                    var dataRemovingVisitor = new RemoveOptionalDataVisitor(optionallyEmittedData);
                    dataRemovingVisitor.Visit(sarifLog);
                }

                optionallyEmittedData = this.FilingContext.DataToInsert;
                if (optionallyEmittedData != OptionallyEmittedData.None)
                {
                    var dataInsertingVisitor = new InsertOptionalDataVisitor(optionallyEmittedData);
                    dataInsertingVisitor.Visit(sarifLog);
                }

                SplittingStrategy splittingStrategy = this.FilingContext.SplittingStrategy;

                if (splittingStrategy == SplittingStrategy.None)
                {
                    FileWorkItemsHelper(sarifLog, this.FilingContext, this.FilingClient);
                    return sarifLog;
                }

                IList<SarifLog> logsToProcess;

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
                    default:
                    {
                        throw new ArgumentOutOfRangeException($"SplittingStrategy: {splittingStrategy}");
                    }
                }

                var partitioningVisitor = new PartitioningVisitor<string>(partitionFunction, deepClone: false);
                partitioningVisitor.VisitSarifLog(sarifLog);

                logsToProcess = new List<SarifLog>(partitioningVisitor.GetPartitionLogs().Values);

                var logsToProcessMetrics = new Dictionary<string, object>
                {
                    { "splittingStrategy", splittingStrategy },
                    { "logsToProcessCount", logsToProcess.Count },
                    { "splittingDurationInMilliseconds", splittingStopwatch.ElapsedMilliseconds },
                };

                this.Logger.LogMetrics(EventIds.LogsToProcessMetrics, logsToProcessMetrics);
                splittingStopwatch.Stop();

                for (int splitFileIndex = 0; splitFileIndex < logsToProcess.Count; splitFileIndex++)
                {
                    SarifLog splitLog = logsToProcess[splitFileIndex];
                    FileWorkItemsHelper(splitLog, this.FilingContext, this.FilingClient);
                }
            }
            return sarifLog;
        }

        internal const string PROGRAMMABLE_URIS_PROPERTY_NAME = "programmableWorkItemUris";

        private void FileWorkItemsHelper(SarifLog sarifLog, SarifWorkItemContext filingContext, FilingClient filingClient)
        {
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

                foreach (SarifWorkItemModelTransformer transformer in sarifWorkItemModel.Context.Transformers)
                {
                    transformer.Transform(sarifWorkItemModel);
                }

                Task<IEnumerable<WorkItemModel>> task = filingClient.FileWorkItems(new[] { sarifWorkItemModel });
                task.Wait();
                this.FiledWorkItems.AddRange(task.Result);

                LogMetricsForFiledWorkItem(sarifLog, sarifWorkItemModel);

                // IMPORTANT: as we update our partitioned logs, we are actually modifying the input log file 
                // as well. That's because our partitioning is configured to reuse references to existing
                // run and result objects, even though they are partitioned into a separate log file. 
                // This approach also us to update the original log file with the filed work item details
                // without requiring us to build a map of results between the original log and its
                // partioned log files.
                //
                UpdateLogWithWorkItemDetails(sarifLog, sarifWorkItemModel.HtmlUri, sarifWorkItemModel.Uri);

                this.FilingSucceeded = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void UpdateLogWithWorkItemDetails(SarifLog sarifLog, Uri htmlUri, Uri uri)
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

        private void LogMetricsForFiledWorkItem(SarifLog sarifLog, SarifWorkItemModel sarifWorkItemModel)
        {
            string guid = Guid.NewGuid().ToString();
            string tags = string.Join(",", sarifWorkItemModel.LabelsOrTags);

            var workItemMetrics = new Dictionary<string, object>
                {
                    { "correlatingGuid", guid },
                    { "area", sarifWorkItemModel.Area },
                    { "bodyOrDescription", sarifWorkItemModel.BodyOrDescription },
                    { "commentOrDiscussion", sarifWorkItemModel.CommentOrDiscussion },
                    { "htmlUri", sarifWorkItemModel.HtmlUri },
                    { "iteration", sarifWorkItemModel.Iteration },
                    { "labelsOrTags", tags },
                    { "locationUri", sarifWorkItemModel.LocationUri },
                    { "milestone", sarifWorkItemModel.Milestone },
                    { "ownerOrAccount", sarifWorkItemModel.OwnerOrAccount },
                    { "repositoryOrProject", sarifWorkItemModel.RepositoryOrProject },
                    { "title", sarifWorkItemModel.Title },
                    { "uri", sarifWorkItemModel.Uri },
                };

            this.Logger.LogMetrics(EventIds.WorkItemFiledCoreMetrics, workItemMetrics);

            Dictionary<string, RuleMetrics> ruleIdToMetricsMap = CreateRuleMetricsMap(sarifLog);

            foreach (string tool in ruleIdToMetricsMap.Keys)
            {
                RuleMetrics ruleMetrics = ruleIdToMetricsMap[tool];

                var workItemDetailMetrics = new Dictionary<string, object>
                {
                    { "correlatingGuid", guid },
                    { "tool", sarifWorkItemModel.Area },
                    { "ruleId", sarifWorkItemModel.Assignees },
                    { "errorCount", ruleMetrics.ErrorCount },
                    { "warningCount", ruleMetrics.WarningCount },
                    { "noteCount", ruleMetrics.NoteCount },
                    { "openCount", ruleMetrics.OpenCount },
                    { "reviewCount", ruleMetrics.ReviewCount }
                };

                this.Logger.LogMetrics(EventIds.WorkItemFiledDetailMetrics, workItemMetrics);
            }
        }

        private static Dictionary<string, RuleMetrics> CreateRuleMetricsMap(SarifLog sarifLog)
        {
            var ruleIdToRuleMetricsMap = new Dictionary<string, RuleMetrics>();

            foreach (Run run in sarifLog.Runs)
            {
                if (run.Results == null) { continue; }

                string toolName = run.Tool.Driver.Name;

                foreach (Result result in run.Results)
                {
                    // Our system expects that results which should not be
                    // filed have been filtered from the log file
                    Debug.Assert(result.ShouldBeFiled());

                    string ruleId = result.GetRule(run).Id;
                    string key = toolName + ruleId;

                    if (!ruleIdToRuleMetricsMap.TryGetValue(key, out RuleMetrics ruleMetrics))
                    {
                        ruleIdToRuleMetricsMap[key] = ruleMetrics = new RuleMetrics();
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
            public int ErrorCount;
            public int WarningCount;
            public int NoteCount;
            public int OpenCount;
            public int ReviewCount;
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