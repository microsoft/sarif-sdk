// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Sarif.Visitors;
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
        /// A starting context object that configures the work item filing operation. In the
        /// current implementation, this context is copied for each SARIF file (if any) split
        /// from the input log and then further elaborated upon.
        /// </param>
        public SarifWorkItemFiler(SarifWorkItemContext filingContext)
        {
            this.FilingContext = filingContext ?? throw new ArgumentNullException(nameof(filingContext));
            this.FilingClient = FilingClientFactory.Create(filingContext.ProjectUri);
        }

        public SarifWorkItemContext FilingContext { get; }

        public FilingClient FilingClient { get; set; }

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
            sarifLog = sarifLog ?? throw new ArgumentNullException(nameof(sarifLog));

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

                    for (int splitFileIndex = 0; splitFileIndex < logsToProcess.Count; splitFileIndex++)
                    {
                        SarifLog splitLog = logsToProcess[splitFileIndex];
                        FileWorkItemsHelper(splitLog, this.FilingContext, this.FilingClient);
                    }
                }
            }
        }

        internal static void FileWorkItemsHelper(SarifLog sarifLog, SarifWorkItemContext filingContext, FilingClient filingClient)
        {
            // The helper below will initialize the sarif work item model with a copy
            // of the root pipeline filing context. This context will then be initialized
            // based on the current sarif log file that we're processing.
            
            SarifWorkItemModel workItemModel = new SarifWorkItemModel(sarifLog, filingContext);

            try
            {
                // Populate the work item with the target organization/repository information.
                // In ADO, certain fields (such as the area path) will defaut to the 
                // project name and so this information is used in at least that context. 
                workItemModel.RepositoryOrProject = filingClient.ProjectOrRepository;
                workItemModel.OwnerOrAccount = filingClient.AccountOrOrganization;

                foreach (SarifWorkItemModelTransformer transformer in workItemModel.Context.Transformers)
                {
                    transformer.Transform(workItemModel);
                }

                filingClient.FileWorkItems(new[] { workItemModel }).Wait();

                // TODO: We need to process updated work item models to persist filing
                //       details back to the input SARIF file, if that was specified.
                //       This code should either return or persist the updated models
                //       via a property, so that the file work items command can do
                //       this work.
                //
                //       https://github.com/microsoft/sarif-sdk/issues/1774
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
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
