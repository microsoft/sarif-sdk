// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.VisualStudio.Services.CircuitBreaker;
using Microsoft.WorkItems;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFiler
    {
        public SarifWorkItemFiler() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifWorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="filingClient">
        /// A client for communicating with a work item filing host (for example, GitHub or Azure DevOps).
        /// </param>
        /// <param name="filingContext">
        /// A root context object that configures the work item filing operation.
        /// </param>
        public SarifWorkItemFiler(FilingClient filingClient, SarifWorkItemContext filingContext)
        {
            FilingClient = filingClient ?? throw new ArgumentNullException(nameof(filingClient));
            FilingContext = filingContext ?? throw new ArgumentNullException(nameof(filingContext));
        }

        public SarifWorkItemContext FilingContext { get; set; }

        public FilingClient FilingClient { get; set; }

        public virtual void FileWorkItems(Uri sarifLogFileLocation)
        {
            sarifLogFileLocation = sarifLogFileLocation ?? throw new ArgumentNullException(nameof(sarifLogFileLocation));

            if (sarifLogFileLocation.IsAbsoluteUri && sarifLogFileLocation.Scheme == "file:")
            {
                FileWorkItems(File.ReadAllText(sarifLogFileLocation.LocalPath));
            }

            throw new NotImplementedException();
        }

        public virtual void FileWorkItems(string sarifLogFileContents)
        {
            sarifLogFileContents = sarifLogFileContents ?? throw new ArgumentNullException(nameof(sarifLogFileContents));

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifLogFileContents);
            FileWorkItems(sarifLog);
        }

        public virtual void FileWorkItems(SarifLog sarifLog)
        {
            sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));

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
                FileWorkItemsForSarifLog(sarifLog);
                return;
            }

            for (int runIndex = 0; runIndex < sarifLog.Runs.Count; ++runIndex)
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
                            
                                // TODO
                            case SplittingStrategy.PerResult:
                            case SplittingStrategy.PerRun:                           
                            default:
                                throw new ArgumentOutOfRangeException($"GroupingStrategy: {splittingStrategy}");
                        }

                        visitor.VisitRun(sarifLog.Runs[runIndex]);
                        logsToProcess = visitor.SplitSarifLogs;
                    }

                    for (int splitFileIndex = 0; splitFileIndex < logsToProcess.Count; splitFileIndex++)
                    {
                        SarifLog splitLog = logsToProcess[splitFileIndex];
                        WorkItemModel<SarifWorkItemContext> workItemModel = splitLog.CreateWorkItemModel(this.FilingContext);

                        try
                        {
                            this.FilingClient.FileWorkItems(new[] { workItemModel });
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(ex.ToString());
                        }

                        // TODO capture work item uris
                    }
                }
            }

        }

        private void FileWorkItemsForSarifLog(SarifLog sarifLog)
        {
            throw new NotImplementedException();
        }

        private IList<SarifLog> SplitSarifLog(SplittingStrategy splittingStrategy)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="projectUri">
        /// The URI of the project in which the work items are to be filed.
        /// </param>
        /// <param name="workItemFilingModels">
        /// Describes the work items to be filed.
        /// </param>
        /// <param name="personalAccessToken">
        /// Specifes the personal access used to access the project. Default: null.
        /// </param>
        /// <returns>
        /// The set of results that were filed as work items.
        /// </returns>
        internal async virtual Task<IEnumerable<WorkItemModel>> FileWorkItems(
            Uri projectUri,
            IList<WorkItemModel<SarifWorkItemContext>> workItemFilingModels,
            string personalAccessToken = null)
        {
            if (projectUri == null) { throw new ArgumentNullException(nameof(projectUri)); }
            if (workItemFilingModels == null) { throw new ArgumentNullException(nameof(workItemFilingModels)); }

            await FilingClient.Connect(personalAccessToken);

            IEnumerable<WorkItemModel> filedResults = await FilingClient.FileWorkItems(workItemFilingModels);

            return filedResults;
        }
    }
}
