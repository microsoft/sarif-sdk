// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WorkItems;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public class SarifWorkItemFiler
    {
        public SarifWorkItemFiler()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifWorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="filingTarget">
        /// An object that represents the system (for example, GitHub or Azure DevOps)
        /// to which the work items will be filed.
        /// </param>
        public SarifWorkItemFiler(FilingClient<SarifWorkItemContext> filingTarget) // init with configuration
        {
            FilingClient = filingTarget ?? throw new ArgumentNullException(nameof(filingTarget));
        }

        public FilingClient<SarifWorkItemContext> FilingClient { get; set; }

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

            // TODO populate the files

            /*
                                   SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(logFileContents);

                                   OptionallyEmittedData optionallyEmittedData = options.DataToRemove.ToFlags();
                                   if (optionallyEmittedData != OptionallyEmittedData.None)
                                   {
                                       var dataRemovingVisitor = new RemoveOptionalDataVisitor(options.DataToRemove.ToFlags());
                                       dataRemovingVisitor.Visit(sarifLog);
                                   }

                                   for (int runIndex = 0; runIndex < sarifLog.Runs.Count; ++runIndex)
                                   {
                                       if (sarifLog.Runs[runIndex]?.Results?.Count > 0)
                                       {
                                           IList<SarifLog> logsToProcess = new List<SarifLog>(new SarifLog[] { sarifLog });

                                           if (options.GroupingStrategy != GroupingStrategy.PerRun)
                                           {
                                               SplittingVisitor visitor;
                                               switch(options.GroupingStrategy)
                                               {
                                                   case GroupingStrategy.PerRunPerRule:
                                                       visitor = new PerRunPerRuleSplittingVisitor();
                                                       break;
                                                   case GroupingStrategy.PerRunPerTarget:
                                                       visitor = new PerRunPerTargetSplittingVisitor();
                                                       break;
                                                   case GroupingStrategy.PerRunPerTargetPerRule:
                                                       visitor = new PerRunPerTargetPerRuleSplittingVisitor();
                                                       break;
                                                   default:
                                                       throw new ArgumentOutOfRangeException($"GroupingStrategy: {options.GroupingStrategy}");
                                               }

                                               visitor.VisitRun(sarifLog.Runs[runIndex]);
                                               logsToProcess = visitor.SplitSarifLogs;
                                           }

                                           IList<WorkItemModel> workItemModels = new List<WorkItemModel>(logsToProcess.Count);

                                           for (int splitFileIndex = 0; splitFileIndex < logsToProcess.Count; splitFileIndex++)
                                           {
                                               SarifLog splitLog = logsToProcess[splitFileIndex];
                                               WorkItemModel workItemModel = splitLog.CreateWorkItemModel(filingClient.ProjectOrRepository);
                                               workItemModels.Add(workItemModel);
                                           }

                                           try
                                           {
                                               string securityToken = Environment.GetEnvironmentVariable("SarifWorkItemFilingSecurityToken");

                                               if (string.IsNullOrEmpty(securityToken))
                                               {
                                                   throw new InvalidOperationException("'SarifWorkItemFilingSecurityToken' environment variable is not initialized with a personal access token.");
                                               }

                                               IEnumerable<WorkItemModel> filedWorkItems = filer.FileWorkItems(options.ProjectUri, workItemModels, securityToken).Result;
                                               Console.WriteLine($"Created {filedWorkItems.Count()} work items for run {runIndex}.");
                                           }
                                           catch (Exception ex)
                                           {
                                               Console.Error.WriteLine(ex);
                                           }
                                       }
                                   }

                                   Console.WriteLine($"Writing log with work item Ids to {options.OutputFilePath}.");
                                   WriteSarifFile<SarifLog>(fileSystem, sarifLog, options.OutputFilePath, (options.PrettyPrint ? Formatting.Indented : Formatting.None));*/
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
        internal async virtual Task<IEnumerable<WorkItemModel<SarifWorkItemContext>>> FileWorkItems(
            Uri projectUri,
            IList<WorkItemModel<SarifWorkItemContext>> workItemFilingModels,
            string personalAccessToken = null)
        {
            if (projectUri == null) { throw new ArgumentNullException(nameof(projectUri)); }
            if (workItemFilingModels == null) { throw new ArgumentNullException(nameof(workItemFilingModels)); }

            await FilingClient.Connect(personalAccessToken);

            IEnumerable<WorkItemModel<SarifWorkItemContext>> filedResults = await FilingClient.FileWorkItems(workItemFilingModels);

            return filedResults;
        }
    }
}
