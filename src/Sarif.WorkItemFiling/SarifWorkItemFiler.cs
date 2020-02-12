// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WorkItemFiling;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
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
        public SarifWorkItemFiler(FilingClient<SarifWorkItemData> filingTarget)
        {
            FilingTarget = filingTarget ?? throw new ArgumentNullException(nameof(filingTarget));
        }

        public FilingClient<SarifWorkItemData> FilingTarget { get; set; }

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
        internal async virtual Task<IEnumerable<WorkItemModel<SarifWorkItemData>>> FileWorkItems(
            Uri projectUri,
            IList<WorkItemModel<SarifWorkItemData>> workItemFilingModels,
            string personalAccessToken = null)
        {
            if (projectUri == null) { throw new ArgumentNullException(nameof(projectUri)); }
            if (workItemFilingModels == null) { throw new ArgumentNullException(nameof(workItemFilingModels)); }

            await FilingTarget.Connect(personalAccessToken);

            IEnumerable<WorkItemModel<SarifWorkItemData>> filedResults = await FilingTarget.FileWorkItems(workItemFilingModels);

            return filedResults;
        }
    }
}
