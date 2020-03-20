// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
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
        /// <param name="filingClient">
        /// A client for communicating with a work item filing host (for example, GitHub or Azure DevOps).
        /// </param>
        /// <param name="filingContext">
        /// A starting context object that configures the work item filing operation. In the
        /// current implementation, this context is copied for each SARIF file (if any) split
        /// from the input log and then further elaborated upon.
        /// </param>
        public SarifWorkItemFiler(Uri filingUri)
        {
            if (filingUri == null) { throw new ArgumentOutOfRangeException(nameof(filingUri)); };

            this.FilingContext = new SarifWorkItemContext
            {
                HostUri = filingUri
            };

            this.FilingClient = FilingClientFactory.Create(this.FilingContext.HostUri);

            this.Logger = ServiceProviderFactory.ServiceProvider.GetService<ILogger<FilingClient>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifWorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="filingClient">
        /// A client for communicating with a work item filing host (for example, GitHub or Azure DevOps).
        /// </param>
        /// <param name="filingContext">
        /// A starting context object that configures the work item filing operation. In the
        /// current implementation, this context is copied for each SARIF file (if any) split
        /// from the input log and then further elaborated upon.
        /// </param>
        public SarifWorkItemFiler(SarifWorkItemContext filingContext) : this(filingContext.HostUri)
        {
            this.FilingContext = filingContext ?? throw new ArgumentNullException(nameof(filingContext));

            this.FilingClient = FilingClientFactory.Create(filingContext.HostUri);
        }

        public FilingClient FilingClient { get; set; }

        public SarifWorkItemContext FilingContext { get; }

        public List<WorkItemModel> FiledWorkItems { get; private set; }

        public bool FilingSucceeded { get; private set; }

        internal ILogger Logger { get; }

        public virtual void FileWorkItems(Uri sarifLogFileLocation)
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
                        FileWorkItems(sarifLog);
                    }
                }
            }
            throw new ArgumentException($"Specified URI was not an absolute file URI: {sarifLogFileLocation}");
        }

        public virtual void FileWorkItems(string sarifLogFileContents)
        {
            sarifLogFileContents = sarifLogFileContents ?? throw new ArgumentNullException(nameof(sarifLogFileContents));

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLogFileContents);
            FileWorkItems(sarifLog);
        }

        public virtual void FileWorkItems(SarifLog sarifLog)
        {
            using (IDisposable loggingScope = Logger.BeginScope("FileWorkItems"))
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
                    return;
                }

                Stopwatch splittingStopwatch = Stopwatch.StartNew();

                for (int runIndex = 0; runIndex < sarifLog.Runs?.Count; ++runIndex)
                {
                    if (sarifLog.Runs[runIndex]?.Results?.Count > 0)
                    {
                        IList<SarifLog> logsToProcess = new List<SarifLog>(new SarifLog[] { sarifLog });

                        if (splittingStrategy != SplittingStrategy.PerRun)
                        {
                            SplittingVisitor visitor;
                            switch (splittingStrategy)
                            {
                                case SplittingStrategy.PerRunPerRule:
                                    visitor = new PerRunPerRuleSplittingVisitor();
                                    break;

                                case SplittingStrategy.PerRunPerTarget:
                                    visitor = new PerRunPerTargetSplittingVisitor();
                                    break;

                                case SplittingStrategy.PerRunPerTargetPerRule:
                                    visitor = new PerRunPerTargetPerRuleSplittingVisitor();
                                    break;

                                // TODO: Implement PerResult and PerRun splittings strategies
                                //
                                //       https://github.com/microsoft/sarif-sdk/issues/1763
                                //       https://github.com/microsoft/sarif-sdk/issues/1762
                                //
                                case SplittingStrategy.PerResult:
                                case SplittingStrategy.PerRun:
                                default:
                                    throw new ArgumentOutOfRangeException($"SplittingStrategy: {splittingStrategy}");
                            }

                            visitor.VisitRun(sarifLog.Runs[runIndex]);
                            logsToProcess = visitor.SplitSarifLogs;
                        }

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
                }
            }
        }

        private void FileWorkItemsHelper(SarifLog sarifLog, SarifWorkItemContext filingContext, FilingClient filingClient)
        {
            // The helper below will initialize the sarif work item model with a copy
            // of the root pipeline filing context. This context will then be initialized
            // based on the current sarif log file that we're processing.

            var workItemModel = new SarifWorkItemModel(sarifLog, filingContext);

            try
            {
                // Populate the work item with the target organization/repository information.
                // In ADO, certain fields (such as the area path) will defaut to the 
                // project name and so this information is used in at least that context. 
                workItemModel.OwnerOrAccount = filingClient.AccountOrOrganization;
                workItemModel.RepositoryOrProject = filingClient.ProjectOrRepository;

                foreach (SarifWorkItemModelTransformer transformer in workItemModel.Context.Transformers)
                {
                    transformer.Transform(workItemModel);
                }

                Task<IEnumerable<WorkItemModel>> task = filingClient.FileWorkItems(new[] { workItemModel });
                task.Wait();

                this.FiledWorkItems.AddRange(task.Result);

                // TODO: We need to process updated work item models to persist filing
                //       details back to the input SARIF file, if that was specified.
                //       This code should either return or persist the updated models
                //       via a property, so that the file work items command can do
                //       this work.
                //
                //       https://github.com/microsoft/sarif-sdk/issues/1774

                this.FilingSucceeded = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        public void Dispose()
        {
            if (this.FilingClient != null)
            {
                this.FilingClient.Dispose();
                this.FilingClient = null;
            }
        }
    }
}
